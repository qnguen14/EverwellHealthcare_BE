using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Microsoft.AspNetCore.Http;
using Everwell.DAL.Data.Requests.Feedback;
using Everwell.DAL.Data.Responses.Feedback;

namespace Everwell.BLL.Services.Implements;

public class FeedbackService : BaseService<FeedbackService>, IFeedbackService
{
    public FeedbackService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<FeedbackService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    #region Legacy Methods (Backward Compatibility)
    public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
    {
        try
        {
            var feedbacks = await _unitOfWork.GetRepository<Feedback>()
                .GetListAsync(
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment));
            
            return feedbacks ?? new List<Feedback>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all feedbacks");
            throw;
        }
    }

    public async Task<Feedback?> GetFeedbackByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<Feedback>()
                .FirstOrDefaultAsync(
                    predicate: f => f.Id == id,
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting feedback by id: {Id}", id);
            throw;
        }
    }

    public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                feedback.Id = Guid.NewGuid();
                feedback.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                
                await _unitOfWork.GetRepository<Feedback>().InsertAsync(feedback);
                return feedback;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating feedback");
            throw;
        }
    }

    public async Task<Feedback?> UpdateFeedbackAsync(Guid id, Feedback feedback)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingFeedback = await _unitOfWork.GetRepository<Feedback>()
                    .FirstOrDefaultAsync(predicate: f => f.Id == id);
                
                if (existingFeedback == null) return null;

                existingFeedback.Rating = feedback.Rating;
                existingFeedback.Comment = feedback.Comment;
                
                _unitOfWork.GetRepository<Feedback>().UpdateAsync(existingFeedback);
                return existingFeedback;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating feedback with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var feedback = await _unitOfWork.GetRepository<Feedback>()
                    .FirstOrDefaultAsync(predicate: f => f.Id == id);
                
                if (feedback == null) return false;

                _unitOfWork.GetRepository<Feedback>().DeleteAsync(feedback);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting feedback with id: {Id}", id);
            throw;
        }
    }
    #endregion

    #region New DTO-based Methods
    public async Task<CreateFeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            _logger.LogInformation("Creating feedback for customer {CustomerId} to consultant {ConsultantId} for appointment {AppointmentId}", 
                currentUserId, request.ConsultantId, request.AppointmentId);

            // Validate that customer can provide feedback for this appointment
            var canProvideFeedback = await CanCustomerProvideFeedbackAsync(request.AppointmentId, currentUserId);
            if (!canProvideFeedback)
            {
                throw new InvalidOperationException("You are not authorized to provide feedback for this appointment or appointment is not completed");
            }

            // Check if feedback already exists
            var hasAlreadyProvidedFeedback = await HasCustomerAlreadyProvidedFeedbackAsync(request.AppointmentId, currentUserId);
            if (hasAlreadyProvidedFeedback)
            {
                throw new InvalidOperationException("You have already provided feedback for this appointment");
            }

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var feedback = _mapper.Map<Feedback>(request);
                feedback.Id = Guid.NewGuid();
                feedback.CustomerId = currentUserId;
                feedback.CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

                await _unitOfWork.GetRepository<Feedback>().InsertAsync(feedback);
                
                _logger.LogInformation("Successfully created feedback {FeedbackId}", feedback.Id);
                
                return _mapper.Map<CreateFeedbackResponse>(feedback);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating feedback for customer {CustomerId}", GetCurrentUserId());
            throw;
        }
    }

    public async Task<FeedbackResponse?> UpdateFeedbackAsync(Guid id, UpdateFeedbackRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingFeedback = await _unitOfWork.GetRepository<Feedback>()
                    .FirstOrDefaultAsync(
                        predicate: f => f.Id == id && f.CustomerId == currentUserId,
                        include: f => f.Include(fb => fb.Customer)
                                      .Include(fb => fb.Consultant)
                                      .Include(fb => fb.Appointment));
                
                if (existingFeedback == null) 
                {
                    _logger.LogWarning("Feedback {FeedbackId} not found or not owned by customer {CustomerId}", id, currentUserId);
                    return null;
                }

                // Update feedback
                existingFeedback.Rating = request.Rating;
                existingFeedback.Comment = request.Comment;
                
                _unitOfWork.GetRepository<Feedback>().UpdateAsync(existingFeedback);
                
                _logger.LogInformation("Successfully updated feedback {FeedbackId}", id);
                
                return _mapper.Map<FeedbackResponse>(existingFeedback);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating feedback {FeedbackId} for customer {CustomerId}", id, GetCurrentUserId());
            throw;
        }
    }

    public async Task<FeedbackResponse?> GetFeedbackResponseByIdAsync(Guid id)
    {
        try
        {
            var feedback = await _unitOfWork.GetRepository<Feedback>()
                .FirstOrDefaultAsync(
                    predicate: f => f.Id == id,
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment));
            
            return feedback != null ? _mapper.Map<FeedbackResponse>(feedback) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting feedback response by id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<FeedbackResponse>> GetAllFeedbackResponsesAsync()
    {
        try
        {
            var feedbacks = await _unitOfWork.GetRepository<Feedback>()
                .GetListAsync(
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment),
                    orderBy: q => q.OrderByDescending(f => f.CreatedAt));
            
            return _mapper.Map<IEnumerable<FeedbackResponse>>(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all feedback responses");
            throw;
        }
    }
    #endregion

    #region User-specific Methods
    public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByCustomerAsync(Guid? customerId = null)
    {
        try
        {
            var targetCustomerId = customerId ?? GetCurrentUserId();
            
            var feedbacks = await _unitOfWork.GetRepository<Feedback>()
                .GetListAsync(
                    predicate: f => f.CustomerId == targetCustomerId,
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment),
                    orderBy: q => q.OrderByDescending(f => f.CreatedAt));
            
            return _mapper.Map<IEnumerable<FeedbackResponse>>(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting feedbacks by customer {CustomerId}", customerId ?? GetCurrentUserId());
            throw;
        }
    }

    public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByConsultantAsync(Guid? consultantId = null)
    {
        try
        {
            var targetConsultantId = consultantId ?? GetCurrentUserId();
            
            var feedbacks = await _unitOfWork.GetRepository<Feedback>()
                .GetListAsync(
                    predicate: f => f.ConsultantId == targetConsultantId,
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment),
                    orderBy: q => q.OrderByDescending(f => f.CreatedAt));
            
            return _mapper.Map<IEnumerable<FeedbackResponse>>(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting feedbacks by consultant {ConsultantId}", consultantId ?? GetCurrentUserId());
            throw;
        }
    }

    public async Task<FeedbackResponse?> GetFeedbackByAppointmentAsync(Guid appointmentId)
    {
        try
        {
            var feedback = await _unitOfWork.GetRepository<Feedback>()
                .FirstOrDefaultAsync(
                    predicate: f => f.AppointmentId == appointmentId,
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  .Include(fb => fb.Appointment));
            
            return feedback != null ? _mapper.Map<FeedbackResponse>(feedback) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting feedback by appointment {AppointmentId}", appointmentId);
            throw;
        }
    }
    #endregion

    #region Validation Methods
    public async Task<bool> CanCustomerProvideFeedbackAsync(Guid appointmentId, Guid? customerId = null)
    {
        try
        {
            var targetCustomerId = customerId ?? GetCurrentUserId();
            
            var appointment = await _unitOfWork.GetRepository<Appointment>()
                .FirstOrDefaultAsync(predicate: a => a.Id == appointmentId && a.CustomerId == targetCustomerId);
            
            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found or not owned by customer {CustomerId}", appointmentId, targetCustomerId);
                return false;
            }

            // Customer can only provide feedback for completed appointments
            return appointment.Status == AppointmentStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if customer can provide feedback for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<bool> HasCustomerAlreadyProvidedFeedbackAsync(Guid appointmentId, Guid? customerId = null)
    {
        try
        {
            var targetCustomerId = customerId ?? GetCurrentUserId();
            
            var existingFeedback = await _unitOfWork.GetRepository<Feedback>()
                .FirstOrDefaultAsync(predicate: f => f.AppointmentId == appointmentId && f.CustomerId == targetCustomerId);
            
            return existingFeedback != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if customer has already provided feedback for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }
    #endregion
} 