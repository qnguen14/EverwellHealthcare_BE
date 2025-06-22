using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Requests.Questions;
using Everwell.DAL.Data.Responses.Questions;
using System.Security.Claims;

namespace Everwell.BLL.Services.Implements;

public class QuestionService : BaseService<QuestionService>, IQuestionService
{
    public QuestionService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<QuestionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
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
                
                await _unitOfWork.GetRepository<Question>().InsertAsync(question);
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

    public async Task<IEnumerable<QuestionResponse>> GetQuestionsByCategoryAsync(string category)
    {
        try
        {
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    predicate: q => q.Category != null && q.Category.ToLower().Contains(category.ToLower()),
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions ?? new List<Question>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting questions by category: {Category}", category);
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
} 