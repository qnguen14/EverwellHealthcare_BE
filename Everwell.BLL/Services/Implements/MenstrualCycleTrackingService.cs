using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class MenstrualCycleTrackingService : BaseService<MenstrualCycleTrackingService>, IMenstrualCycleTrackingService
{
    public MenstrualCycleTrackingService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<MenstrualCycleTrackingService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<MenstrualCycleTracking>> GetAllMenstrualCycleTrackingsAsync()
    {
        try
        {
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    include: m => m.Include(mct => mct.Customer)
                                   .Include(mct => mct.Notifications));
            
            return trackings ?? new List<MenstrualCycleTracking>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all menstrual cycle trackings");
            throw;
        }
    }

    public async Task<MenstrualCycleTracking?> GetMenstrualCycleTrackingByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(
                    predicate: m => m.TrackingId == id,
                    include: m => m.Include(mct => mct.Customer)
                                   .Include(mct => mct.Notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting menstrual cycle tracking by id: {Id}", id);
            throw;
        }
    }

    public async Task<MenstrualCycleTracking> CreateMenstrualCycleTrackingAsync(MenstrualCycleTracking tracking)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                tracking.TrackingId = Guid.NewGuid();
                tracking.CreatedAt = DateTime.UtcNow;
                
                await _unitOfWork.GetRepository<MenstrualCycleTracking>().InsertAsync(tracking);
                return tracking;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating menstrual cycle tracking");
            throw;
        }
    }

    public async Task<MenstrualCycleTracking?> UpdateMenstrualCycleTrackingAsync(Guid id, MenstrualCycleTracking tracking)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingTracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: m => m.TrackingId == id);
                
                if (existingTracking == null) return null;

                existingTracking.CycleStartDate = tracking.CycleStartDate;
                existingTracking.CycleEndDate = tracking.CycleEndDate;
                existingTracking.Symptoms = tracking.Symptoms;
                existingTracking.Notes = tracking.Notes;
                existingTracking.NotifyBeforeDays = tracking.NotifyBeforeDays;
                existingTracking.NotificationEnabled = tracking.NotificationEnabled;
                
                _unitOfWork.GetRepository<MenstrualCycleTracking>().UpdateAsync(existingTracking);
                return existingTracking;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating menstrual cycle tracking with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteMenstrualCycleTrackingAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: m => m.TrackingId == id);
                
                if (tracking == null) return false;

                _unitOfWork.GetRepository<MenstrualCycleTracking>().DeleteAsync(tracking);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting menstrual cycle tracking with id: {Id}", id);
            throw;
        }
    }
} 