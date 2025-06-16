using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IMenstrualCycleNotificationService
    {
        Task ProcessPendingNotificationsAsync();
        Task ScheduleNotificationsForTrackingAsync(Guid trackingId);
        Task ScheduleNotificationsForTrackingAsync(MenstrualCycleTracking tracking);
        Task SendPeriodReminderAsync(Guid customerId, DateTime predictedDate);
        Task SendOvulationReminderAsync(Guid customerId, DateTime ovulationDate);
            Task SendFertilityWindowReminderAsync(Guid customerId, DateTime windowStart, DateTime windowEnd);
    Task<bool> CreateNotificationAsync(Guid trackingId, MenstrualCyclePhase phase, DateTime scheduledDate, string message);
    
    // Debug method
    Task<bool> TestCreateNotificationDirectlyAsync(Guid trackingId);
}
}
