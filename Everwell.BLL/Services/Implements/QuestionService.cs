using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Requests.Questions;
using Everwell.DAL.Data.Responses.Questions;
using Everwell.DAL.Data.Requests.Notifications;
using System.Security.Claims;

namespace Everwell.BLL.Services.Implements;

public class QuestionService : BaseService<QuestionService>, IQuestionService
{
    private readonly INotificationService _notificationService;

    public QuestionService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<QuestionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<QuestionResponse>> GetAllQuestionsAsync()
    {
        try
        {
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions ?? new List<Question>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all questions");
            throw;
        }
    }

    public async Task<QuestionResponse?> GetQuestionByIdAsync(Guid id)
    {
        try
        {
            var question = await _unitOfWork.GetRepository<Question>()
                .FirstOrDefaultAsync(
                    predicate: q => q.QuestionId == id,
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return question != null ? _mapper.Map<QuestionResponse>(question) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting question by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateQuestionResponse> CreateQuestionAsync(CreateQuestionRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var question = _mapper.Map<Question>(request);
                question.QuestionId = Guid.NewGuid();
                question.CustomerId = currentUserId.Value;
                question.CreatedAt = DateTime.UtcNow;
                question.Status = QuestionStatus.Pending;
                // ConsultantId remains null for unassigned questions
                
                await _unitOfWork.GetRepository<Question>().InsertAsync(question);
                
                // Notify all consultants about the new question
                await NotifyConsultantsAboutNewQuestion(question);
                
                return _mapper.Map<CreateQuestionResponse>(question);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating question");
            throw;
        }
    }

    public async Task<QuestionResponse?> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingQuestion = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(
                        predicate: q => q.QuestionId == id,
                        include: q => q.Include(question => question.Customer)
                                       .Include(question => question.Consultant));
                
                if (existingQuestion == null) return null;

                _mapper.Map(request, existingQuestion);
                
                if (request.Status == QuestionStatus.Answered && existingQuestion.AnsweredAt == null)
                {
                    existingQuestion.AnsweredAt = DateTime.UtcNow;
                }
                
                _unitOfWork.GetRepository<Question>().UpdateAsync(existingQuestion);
                return _mapper.Map<QuestionResponse>(existingQuestion);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating question with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var question = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(predicate: q => q.QuestionId == id);
                
                if (question == null) return false;

                _unitOfWork.GetRepository<Question>().DeleteAsync(question);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting question with id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<QuestionResponse>> GetQuestionsByCustomerAsync(Guid customerId)
    {
        try
        {
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    predicate: q => q.CustomerId == customerId,
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions ?? new List<Question>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting questions by customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<QuestionResponse>> GetQuestionsByConsultantAsync(Guid consultantId)
    {
        try
        {
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    predicate: q => q.ConsultantId == consultantId,
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions ?? new List<Question>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting questions by consultant: {ConsultantId}", consultantId);
            throw;
        }
    }

    public async Task<IEnumerable<QuestionResponse>> GetUnassignedQuestionsAsync()
    {
        try
        {
            // Get questions that have no consultant assigned (truly unassigned)
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    predicate: q => q.ConsultantId == null && q.Status == QuestionStatus.Pending,
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions ?? new List<Question>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting unassigned questions");
            throw;
        }
    }



    public async Task<QuestionResponse?> AssignQuestionToConsultantAsync(Guid questionId, Guid consultantId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var question = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(
                        predicate: q => q.QuestionId == questionId,
                        include: q => q.Include(question => question.Customer)
                                       .Include(question => question.Consultant));
                
                if (question == null) return null;

                // Only allow assignment if question is unassigned or pending
                if (question.ConsultantId != null && question.Status != QuestionStatus.Pending)
                {
                    throw new InvalidOperationException("Question is already assigned or not available for assignment");
                }

                question.ConsultantId = consultantId;
                question.Status = QuestionStatus.Assigned;
                
                _unitOfWork.GetRepository<Question>().UpdateAsync(question);

                // Re-fetch to get consultant info
                var updatedQuestion = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(
                        predicate: q => q.QuestionId == questionId,
                        include: q => q.Include(question => question.Customer)
                                       .Include(question => question.Consultant));

                return _mapper.Map<QuestionResponse>(updatedQuestion);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning question {QuestionId} to consultant {ConsultantId}", questionId, consultantId);
            throw;
        }
    }

    public async Task<QuestionResponse?> AnswerQuestionAsync(Guid id, string answer)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var question = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(
                        predicate: q => q.QuestionId == id,
                        include: q => q.Include(question => question.Customer)
                                       .Include(question => question.Consultant));
                
                if (question == null) return null;

                question.AnswerText = answer;
                question.Status = QuestionStatus.Answered;
                question.AnsweredAt = DateTime.UtcNow;
                
                _unitOfWork.GetRepository<Question>().UpdateAsync(question);
                return _mapper.Map<QuestionResponse>(question);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while answering question with id: {Id}", id);
            throw;
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private async Task NotifyConsultantsAboutNewQuestion(Question question)
    {
        try
        {
            _logger.LogInformation("Starting notification process for question {QuestionId}", question.QuestionId);
            
            // Get all consultants (users with consultant role)
            // Note: Looking for roleId = 2 based on database inspection
            var consultants = await _unitOfWork.GetRepository<User>()
                .GetListAsync(
                    predicate: u => u.RoleId == 2 && u.IsActive, // Consultant role ID in database
                    include: u => u.Include(user => user.Role));

            _logger.LogInformation("Found {ConsultantCount} consultants with RoleId {RoleId}", 
                consultants.Count(), (int)RoleName.Consultant);

            if (consultants.Count() == 0)
            {
                _logger.LogWarning("No consultants found to notify about question {QuestionId}", question.QuestionId);
                return;
            }

            // Create notifications for all consultants
            foreach (var consultant in consultants)
            {
                try
                {
                    _logger.LogInformation("Creating notification for consultant {ConsultantId} ({ConsultantName})", 
                        consultant.Id, consultant.Name);

                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = consultant.Id,
                        Title = "New Question Available",
                        Message = $"A new question titled '{question.Title}' has been submitted and is available for you to answer.",
                        Type = NotificationType.Question,
                        Priority = NotificationPriority.Medium,
                        QuestionId = question.QuestionId
                    };

                    await _notificationService.CreateNotification(notificationRequest);
                    _logger.LogInformation("Successfully created notification for consultant {ConsultantId}", consultant.Id);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, "Failed to create notification for consultant {ConsultantId}", consultant.Id);
                }
            }

            _logger.LogInformation("Completed notification process for {ConsultantCount} consultants about question {QuestionId}", 
                consultants.Count(), question.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify consultants about new question {QuestionId}", question.QuestionId);
            // Don't throw here - notification failure shouldn't prevent question creation
        }
    }
} 