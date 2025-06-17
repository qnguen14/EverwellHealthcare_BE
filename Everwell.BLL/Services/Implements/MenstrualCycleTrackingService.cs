using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Requests.MenstrualCycle;
using Everwell.DAL.Data.Responses.MenstrualCycle;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class MenstrualCycleTrackingService : BaseService<MenstrualCycleTrackingService>, IMenstrualCycleTrackingService
{
    private readonly IMenstrualCycleNotificationService _notificationService;

    public MenstrualCycleTrackingService(
        IUnitOfWork<EverwellDbContext> unitOfWork, 
        ILogger<MenstrualCycleTrackingService> logger, 
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IMenstrualCycleNotificationService notificationService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<GetMenstrualCycleResponse>> GetAllMenstrualCycleTrackingsAsync()
    {
        try
        {
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    include: m => m.Include(mct => mct.Customer)
                                   .Include(mct => mct.Notifications));

            return _mapper.Map<IEnumerable<GetMenstrualCycleResponse>>(trackings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all menstrual cycle trackings");
            throw;
        }
    }

    public async Task<GetMenstrualCycleResponse?> GetMenstrualCycleTrackingByIdAsync(Guid id)
    {
        try
        {
            var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(
                    predicate: m => m.TrackingId == id,
                    include: m => m.Include(mct => mct.Customer)
                                   .Include(mct => mct.Notifications));

            return _mapper.Map<GetMenstrualCycleResponse>(tracking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting menstrual cycle tracking by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateMenstrualCycleResponse> CreateMenstrualCycleTrackingAsync(CreateMenstrualCycleRequest request, Guid customerId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var tracking = _mapper.Map<MenstrualCycleTracking>(request);
                tracking.TrackingId = Guid.NewGuid();
                tracking.CustomerId = customerId;
                tracking.CreatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Creating tracking with ID {TrackingId}, NotificationEnabled: {NotificationEnabled}, NotifyBeforeDays: {NotifyBeforeDays}", 
                    tracking.TrackingId, tracking.NotificationEnabled, tracking.NotifyBeforeDays);
                
                await _unitOfWork.GetRepository<MenstrualCycleTracking>().InsertAsync(tracking);
                
                // Force save to make tracking available for notification service
                await _unitOfWork.SaveChangesAsync();
                
                // Schedule notifications if enabled
                if (tracking.NotificationEnabled)
                {
                    _logger.LogInformation("About to schedule notifications for tracking {TrackingId}", tracking.TrackingId);
                    await _notificationService.ScheduleNotificationsForTrackingAsync(tracking);
                    _logger.LogInformation("Notifications scheduled for tracking {TrackingId}", tracking.TrackingId);
                    
                    // Check context state after notifications
                    var contextStateAfterNotifications = _unitOfWork.Context.ChangeTracker.Entries().Count();
                    _logger.LogInformation("Context has {Count} tracked entities after adding notifications", contextStateAfterNotifications);
                    
                    // Log all tracked entities
                    var trackedEntities = _unitOfWork.Context.ChangeTracker.Entries()
                        .Select(e => new { Type = e.Entity.GetType().Name, State = e.State })
                        .ToList();
                    
                    foreach (var entity in trackedEntities)
                    {
                        _logger.LogInformation("Tracked entity: {Type} - {State}", entity.Type, entity.State);
                    }
                }
                else
                {
                    _logger.LogInformation("Notifications disabled for tracking {TrackingId}", tracking.TrackingId);
                }
                
                _logger.LogInformation("About to commit transaction for tracking {TrackingId}", tracking.TrackingId);
                
                return _mapper.Map<CreateMenstrualCycleResponse>(tracking);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating menstrual cycle tracking");
            throw;
        }
    }

    public async Task<CreateMenstrualCycleResponse?> UpdateMenstrualCycleTrackingAsync(Guid id, UpdateMenstrualCycleRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingTracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: m => m.TrackingId == id);

                if (existingTracking == null) return null;

                _mapper.Map(request, existingTracking);

                _unitOfWork.GetRepository<MenstrualCycleTracking>().UpdateAsync(existingTracking);
                return _mapper.Map<CreateMenstrualCycleResponse>(existingTracking);
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

    public async Task<List<GetMenstrualCycleResponse>> GetCycleHistoryAsync(Guid customerId, int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    predicate: m => m.CustomerId == customerId && m.CycleStartDate >= startDate,
                    include: m => m.Include(mct => mct.Customer)
                                   .Include(mct => mct.Notifications));

            return _mapper.Map<List<GetMenstrualCycleResponse>>(trackings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting cycle history for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CyclePredictionResponse> PredictNextCycleAsync(Guid customerId)
    {
        try
        {
            var history = await GetCycleHistoryAsync(customerId, 6);
            if (!history.Any())
                throw new InvalidOperationException("Not enough cycle data to make prediction");

            // Calculate cycle lengths correctly (start to start, not start to end)
            var cycleLengths = new List<double>();
            var sortedHistory = history.OrderBy(h => h.CycleStartDate).ToList();
            
            for (int i = 0; i < sortedHistory.Count - 1; i++)
            {
                var currentStart = sortedHistory[i].CycleStartDate;
                var nextStart = sortedHistory[i + 1].CycleStartDate;
                var cycleLength = (nextStart - currentStart).TotalDays;
                
                // Validate reasonable cycle length
                if (cycleLength >= 21 && cycleLength <= 45)
                    cycleLengths.Add(cycleLength);
            }

            if (!cycleLengths.Any())
                throw new InvalidOperationException("No valid cycle lengths found for prediction");

            var averageCycleLength = cycleLengths.Average();
            var lastCycle = sortedHistory.OrderByDescending(h => h.CycleStartDate).First();
            
            // Calculate next period start correctly
            var nextPeriodStart = lastCycle.CycleStartDate.AddDays(averageCycleLength);
            var nextPeriodEnd = nextPeriodStart.AddDays(5); // Average period length
            
            // Calculate confidence and regularity
            var confidenceScore = CalculateConfidenceScore(cycleLengths);
            var isRegular = IsRegularCycle(cycleLengths);
            
            return new CyclePredictionResponse
            {
                PredictedNextPeriodStart = nextPeriodStart,
                PredictedNextPeriodEnd = nextPeriodEnd,
                PredictedCycleLength = (int)Math.Round(averageCycleLength),
                PredictedPeriodLength = 5,
                ConfidenceScore = confidenceScore,
                ConfidenceLevel = GetConfidenceLevel(confidenceScore),
                IsRegularCycle = isRegular,
                Factors = GeneratePredictionFactors(cycleLengths.Count, isRegular, confidenceScore)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while predicting next cycle for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<FertilityWindowResponse> GetFertilityWindowAsync(Guid customerId)
    {
        try
        {
            var prediction = await PredictNextCycleAsync(customerId);
            
            // Calculate ovulation date with improved logic
            var lutealPhaseLength = CalculateLutealPhaseLength(prediction.PredictedCycleLength);
            var ovulationDate = prediction.PredictedNextPeriodStart.AddDays(-lutealPhaseLength);
            
            // Calculate fertility window (5 days before + ovulation day)
            var fertilityStart = ovulationDate.AddDays(-5);
            var fertilityEnd = ovulationDate;
            
            return new FertilityWindowResponse
            {
                FertileWindowStart = fertilityStart,
                FertileWindowEnd = fertilityEnd,
                OvulationDate = ovulationDate,
                DaysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days,
                FertilityScore = CalculateFertilityScore(ovulationDate),
                FertilityPhase = GetFertilityPhase(ovulationDate),
                IsHighFertilityPeriod = IsHighFertilityPeriod(ovulationDate),
                Recommendations = GenerateFertilityRecommendations(ovulationDate)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating fertility window for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CycleAnalyticsResponse> GetCycleAnalyticsAsync(Guid customerId)
    {
        try
        {
            var history = await GetCycleHistoryAsync(customerId, 12);
            if (!history.Any())
                throw new InvalidOperationException("Not enough cycle data for analytics");

            var completedCycles = history.Where(h => h.CycleEndDate.HasValue).ToList();
            var cycleLengths = completedCycles.Select(h => (h.CycleEndDate!.Value - h.CycleStartDate).TotalDays).ToList();
            
            var symptoms = history
                .Where(h => !string.IsNullOrEmpty(h.Symptoms))
                .SelectMany(h => h.Symptoms!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .ToList();

            var symptomFrequency = symptoms
                .GroupBy(s => s)
                .ToDictionary(g => g.Key, g => g.Count());

            return new CycleAnalyticsResponse
            {
                AverageCycleLength = cycleLengths.Any() ? cycleLengths.Average() : 0,
                AveragePeriodLength = 5, // Default period length
                TotalCyclesTracked = history.Count,
                FirstCycleDate = history.Min(h => h.CycleStartDate),
                LastCycleDate = history.Max(h => h.CycleStartDate),
                CycleRegularityScore = CalculateCycleRegularity(cycleLengths.Select(l => (int)l).ToList()),
                CommonSymptoms = symptomFrequency.OrderByDescending(kv => kv.Value).Take(5).Select(kv => kv.Key).ToList(),
                SymptomFrequency = symptomFrequency,
                CycleLengthHistory = completedCycles.Select(h => new CycleLengthData
                {
                    CycleStart = h.CycleStartDate,
                    Length = (int)(h.CycleEndDate!.Value - h.CycleStartDate).TotalDays,
                    IsComplete = true
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting cycle analytics for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<List<NotificationResponse>> GetUpcomingNotificationsAsync(Guid customerId)
    {
        try
        {
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    predicate: m => m.CustomerId == customerId && m.NotificationEnabled,
                    include: m => m.Include(mct => mct.Notifications));

            return _mapper.Map<List<NotificationResponse>>(trackings.SelectMany(t => t.Notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting upcoming notifications for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<bool> UpdateNotificationPreferencesAsync(Guid customerId, NotificationPreferencesRequest request)
    {
        try
        {
            var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(predicate: m => m.CustomerId == customerId);
            
            if (tracking == null)
                return false;

            tracking.NotificationEnabled = request.EnablePeriodReminders || request.EnableOvulationReminders;
            tracking.NotifyBeforeDays = request.PeriodReminderDays;
            
            _unitOfWork.GetRepository<MenstrualCycleTracking>().UpdateAsync(tracking);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating notification preferences for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CycleValidationResult> ValidateCycleDataAsync(CreateMenstrualCycleRequest request, Guid customerId)
    {
        try
        {
            var result = new CycleValidationResult { IsValid = true };

            // Period length validation
            if (request.CycleEndDate.HasValue)
            {
                var periodLength = (request.CycleEndDate.Value - request.CycleStartDate).TotalDays;
                if (periodLength < 1 || periodLength > 10)
                {
                    result.IsValid = false;
                    result.Errors.Add("Period length should be between 1-10 days");
                }
            }

            // Cycle start date validation
            if (request.CycleStartDate > DateTime.UtcNow.AddDays(1))
            {
                result.IsValid = false;
                result.Errors.Add("Cycle start date cannot be in the future");
            }

            // Check if cycle start is too far in the past (1 year)
            if (request.CycleStartDate < DateTime.UtcNow.AddYears(-1))
            {
                result.IsValid = false;
                result.Errors.Add("Cycle start date cannot be more than 1 year in the past");
            }

            // Check for overlapping or too close cycles
            var lastCycle = await GetLastCycleAsync(customerId);
            if (lastCycle != null)
            {
                var daysBetween = (request.CycleStartDate - lastCycle.CycleStartDate).TotalDays;
                if (Math.Abs(daysBetween) < 15) // Minimum 15 days between cycles
                {
                    result.IsValid = false;
                    result.Errors.Add("Cycles must be at least 15 days apart");
                }
                
                if (daysBetween > 60) // Maximum 60 days between cycles
                {
                    result.IsValid = false;
                    result.Errors.Add("Gap between cycles is too large (>60 days). Please ensure dates are correct.");
                }
            }

            // Validate symptoms format
            if (!string.IsNullOrEmpty(request.Symptoms))
            {
                var symptoms = request.Symptoms.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (symptoms.Length > 10)
                {
                    result.IsValid = false;
                    result.Errors.Add("Maximum 10 symptoms allowed");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating cycle data for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<bool> CanCreateNewCycleAsync(Guid customerId, DateTime startDate)
    {
        try
        {
            var existingCycle = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(predicate: m =>
                    m.CustomerId == customerId &&
                    m.CycleStartDate <= startDate &&
                    (m.CycleEndDate == null || m.CycleEndDate >= startDate));

            return existingCycle == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if new cycle can be created for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CycleInsightsResponse> GetCycleInsightsAsync(Guid customerId)
    {
        try
        {
            var history = await GetCycleHistoryAsync(customerId, 12);
            if (!history.Any())
                throw new InvalidOperationException("Not enough cycle data for insights");

            var analytics = await GetCycleAnalyticsAsync(customerId);
            
            var insights = new List<string>();
            var recommendations = new List<string>();
            var healthAlerts = new List<string>();

            // Generate insights based on analytics
            if (analytics.CycleRegularityScore < 0.7)
            {
                insights.Add("Your cycles show some irregularity");
                recommendations.Add("Consider tracking your symptoms more closely");
            }

            if (analytics.AverageCycleLength < 21 || analytics.AverageCycleLength > 35)
            {
                healthAlerts.Add("Cycle length outside normal range");
                recommendations.Add("Consider consulting a healthcare provider");
            }

            return new CycleInsightsResponse
            {
                OverallHealthStatus = GetOverallHealthStatus(analytics),
                Insights = insights,
                Recommendations = recommendations,
                HealthAlerts = healthAlerts,
                HealthMetrics = new Dictionary<string, object>
                {
                    { "CycleRegularity", analytics.CycleRegularityScore },
                    { "AverageCycleLength", analytics.AverageCycleLength },
                    { "TotalCycles", analytics.TotalCyclesTracked }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting cycle insights for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<List<CycleTrendData>> GetCycleTrendsAsync(Guid customerId, int months = 6)
    {
        try
        {
            var history = await GetCycleHistoryAsync(customerId, months);
            if (!history.Any())
                throw new InvalidOperationException("Not enough cycle data for trends");

            return history.Select(h => new CycleTrendData
            {
                Date = h.CycleStartDate,
                CycleLength = h.CycleEndDate.HasValue ? (int)(h.CycleEndDate.Value - h.CycleStartDate).TotalDays : 0,
                PeriodLength = 5, // Default period length
                Symptoms = !string.IsNullOrEmpty(h.Symptoms) 
                    ? h.Symptoms.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
                    : new List<string>(),
                Month = h.CycleStartDate.Month,
                Year = h.CycleStartDate.Year
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting cycle trends for customer: {CustomerId}", customerId);
            throw;
        }
    }

    private async Task<GetMenstrualCycleResponse?> GetLastCycleAsync(Guid customerId)
    {
        var cycles = await GetCycleHistoryAsync(customerId, 1);
        return cycles.FirstOrDefault();
    }

    private double CalculateConfidenceScore(List<double> cycleLengths)
    {
        if (cycleLengths.Count < 2) return 30; // Low confidence for insufficient data
        
        var mean = cycleLengths.Average();
        var variance = cycleLengths.Select(x => Math.Pow(x - mean, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        
        // Base confidence on regularity and data points
        var regularityScore = Math.Max(0, 100 - (standardDeviation * 8));
        var dataPointBonus = Math.Min(25, cycleLengths.Count * 5);
        
        var confidence = regularityScore * 0.75 + dataPointBonus;
        return Math.Max(30, Math.Min(95, confidence));
    }

    private string GetConfidenceLevel(double score)
    {
        return score switch
        {
            >= 80 => "High",
            >= 60 => "Medium",
            >= 40 => "Low",
            _ => "Very Low"
        };
    }

    private bool IsRegularCycle(List<double> cycleLengths)
    {
        if (cycleLengths.Count < 3) return false;
        
        var mean = cycleLengths.Average();
        var variance = cycleLengths.Select(x => Math.Pow(x - mean, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        
        return standardDeviation <= 6; // Cycles within 6 days variation = regular
    }

    private int CalculateLutealPhaseLength(int cycleLength)
    {
        // Luteal phase is typically 12-16 days (average 14)
        return cycleLength switch
        {
            < 25 => 12, // Shorter cycles have shorter luteal phase
            > 35 => 16, // Longer cycles might have longer luteal phase
            _ => 14     // Standard luteal phase
        };
    }

    private bool IsHighFertilityPeriod(DateTime ovulationDate)
    {
        var daysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days;
        return Math.Abs(daysUntilOvulation) <= 2; // Within 2 days of ovulation
    }

    private List<string> GeneratePredictionFactors(int cycleCount, bool isRegular, double confidence)
    {
        var factors = new List<string>();
        
        factors.Add($"Based on {cycleCount} cycle(s) of data");
        factors.Add(isRegular ? "Regular cycle pattern detected" : "Irregular cycle pattern");
        factors.Add($"Prediction confidence: {confidence:F0}%");
        
        if (cycleCount >= 6)
            factors.Add("Sufficient historical data for accurate prediction");
        else if (cycleCount >= 3)
            factors.Add("Moderate historical data available");
        else
            factors.Add("Limited historical data - predictions may be less accurate");
        
        return factors;
    }

    private double CalculateFertilityScore(DateTime ovulationDate)
    {
        var daysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days;
        
        if (Math.Abs(daysUntilOvulation) <= 1) return 95; // Peak fertility
        if (Math.Abs(daysUntilOvulation) <= 2) return 85; // High fertility
        if (Math.Abs(daysUntilOvulation) <= 5) return 60; // Moderate fertility
        
        return 20; // Low fertility
    }

    private string GetFertilityPhase(DateTime ovulationDate)
    {
        var daysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days;
        
        return daysUntilOvulation switch
        {
            <= -2 => "Luteal Phase",
            <= 0 => "Ovulation",
            <= 5 => "Fertile Window",
            _ => "Follicular Phase"
        };
    }

    private List<string> GenerateFertilityRecommendations(DateTime ovulationDate)
    {
        var recommendations = new List<string>();
        var daysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days;
        
        if (daysUntilOvulation >= -1 && daysUntilOvulation <= 1)
        {
            recommendations.Add("Peak fertility period - ideal time for conception");
            recommendations.Add("Track cervical mucus and basal body temperature");
        }
        else if (daysUntilOvulation >= -5 && daysUntilOvulation <= 5)
        {
            recommendations.Add("Fertile window - conception is possible");
            recommendations.Add("Monitor ovulation signs and symptoms");
        }
        else
        {
            recommendations.Add("Low fertility period");
            recommendations.Add("Focus on cycle tracking and health maintenance");
        }
        
        recommendations.Add("Maintain a healthy diet and regular exercise");
        recommendations.Add("Take prenatal vitamins if trying to conceive");
        
        return recommendations;
    }

    private float CalculateCycleRegularity(List<int> cycleLengths)
    {
        if (cycleLengths.Count < 2) return 0f;
        
        var mean = cycleLengths.Average();
        var variance = cycleLengths.Select(x => Math.Pow(x - mean, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        
        // Convert to percentage (lower deviation = higher regularity)
        var regularity = Math.Max(0, 100 - (standardDeviation * 10));
        return (float)Math.Min(100, regularity);
    }

    private string GetOverallHealthStatus(CycleAnalyticsResponse analytics)
    {
        if (analytics.CycleRegularityScore >= 0.8 && 
            analytics.AverageCycleLength >= 21 && 
            analytics.AverageCycleLength <= 35)
        {
            return "Excellent";
        }
        else if (analytics.CycleRegularityScore >= 0.6)
        {
            return "Good";
        }
        else
        {
            return "Needs Attention";
        }
    }
}