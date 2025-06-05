using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IMenstrualCycleTrackingService
{
    Task<IEnumerable<MenstrualCycleTracking>> GetAllMenstrualCycleTrackingsAsync();
    Task<MenstrualCycleTracking?> GetMenstrualCycleTrackingByIdAsync(Guid id);
    Task<MenstrualCycleTracking> CreateMenstrualCycleTrackingAsync(MenstrualCycleTracking tracking);
    Task<MenstrualCycleTracking?> UpdateMenstrualCycleTrackingAsync(Guid id, MenstrualCycleTracking tracking);
    Task<bool> DeleteMenstrualCycleTrackingAsync(Guid id);
}