using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class FeedbackService : BaseService<FeedbackService>, IFeedbackService
{
    public FeedbackService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<FeedbackService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
    {
        try
        {
            var feedbacks = await _unitOfWork.GetRepository<Feedback>()
                .GetListAsync(
                    include: f => f.Include(fb => fb.Customer)
                                  .Include(fb => fb.Consultant)
                                  // .Include(fb => fb.Service)
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
                                  // .Include(fb => fb.Service)
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
} 