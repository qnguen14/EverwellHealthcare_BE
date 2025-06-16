using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.MenstrualCycle;
using Everwell.DAL.Data.Responses.MenstrualCycle;

namespace Everwell.BLL.Services.Interfaces;

public interface IMenstrualCycleTrackingService
{
    // Basic CRUD operations
    Task<IEnumerable<GetMenstrualCycleResponse>> GetAllMenstrualCycleTrackingsAsync();
    Task<GetMenstrualCycleResponse?> GetMenstrualCycleTrackingByIdAsync(Guid id);
    Task<CreateMenstrualCycleResponse> CreateMenstrualCycleTrackingAsync(CreateMenstrualCycleRequest request, Guid customerId);
    Task<CreateMenstrualCycleResponse?> UpdateMenstrualCycleTrackingAsync(Guid id, UpdateMenstrualCycleRequest request);
    Task<bool> DeleteMenstrualCycleTrackingAsync(Guid id);
    
    // Enhanced cycle tracking functionality
    Task<List<GetMenstrualCycleResponse>> GetCycleHistoryAsync(Guid customerId, int months = 12);
    Task<CyclePredictionResponse> PredictNextCycleAsync(Guid customerId);
    Task<FertilityWindowResponse> GetFertilityWindowAsync(Guid customerId);
    Task<CycleAnalyticsResponse> GetCycleAnalyticsAsync(Guid customerId);
    
    // Notification management
    Task<List<NotificationResponse>> GetUpcomingNotificationsAsync(Guid customerId);
    Task<bool> UpdateNotificationPreferencesAsync(Guid customerId, NotificationPreferencesRequest request);
    
    // Validation and business rules
    Task<CycleValidationResult> ValidateCycleDataAsync(CreateMenstrualCycleRequest request, Guid customerId);
    Task<bool> CanCreateNewCycleAsync(Guid customerId, DateTime startDate);
    
    // Statistics and insights
    Task<CycleInsightsResponse> GetCycleInsightsAsync(Guid customerId);
    Task<List<CycleTrendData>> GetCycleTrendsAsync(Guid customerId, int months = 6);
}