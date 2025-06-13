using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Requests.MenstrualCycle;
using Everwell.DAL.Data.Responses.MenstrualCycle;

namespace Everwell.BLL.Services.Implements;

public class MenstrualCycleTrackingService : BaseService<MenstrualCycleTrackingService>, IMenstrualCycleTrackingService
{
    public MenstrualCycleTrackingService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<MenstrualCycleTrackingService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
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
                
                await _unitOfWork.GetRepository<MenstrualCycleTracking>().InsertAsync(tracking);
                
                // Schedule notifications if enabled
                if (tracking.NotificationEnabled)
                {
                    await ScheduleNotificationsAsync(tracking.TrackingId);
                }
                
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

            var cycleLengths = history
                .Where(h => h.CycleEndDate.HasValue)
                .Select(h => (h.CycleEndDate!.Value - h.CycleStartDate).TotalDays)
                .ToList();

            if (!cycleLengths.Any())
                throw new InvalidOperationException("No completed cycles found for prediction");

            var averageCycleLength = cycleLengths.Average();
            var lastCycle = history.OrderByDescending(h => h.CycleStartDate).First();
            
            return new CyclePredictionResponse
            {
                PredictedNextPeriodStart = lastCycle.CycleEndDate?.AddDays(averageCycleLength) ?? DateTime.UtcNow.AddDays(28),
                PredictedNextPeriodEnd = lastCycle.CycleEndDate?.AddDays(averageCycleLength + 5) ?? DateTime.UtcNow.AddDays(33),
                PredictedCycleLength = (int)Math.Round(averageCycleLength),
                PredictedPeriodLength = 5,
                ConfidenceScore = CalculateConfidenceScore(cycleLengths),
                ConfidenceLevel = GetConfidenceLevel(CalculateConfidenceScore(cycleLengths)),
                IsRegularCycle = cycleLengths.All(l => Math.Abs(l - averageCycleLength) <= 3),
                Factors = new List<string> { "Historical cycle data", "Average cycle length calculation" }
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
            var ovulationDate = prediction.PredictedNextPeriodStart.AddDays(-14);
            
            return new FertilityWindowResponse
            {
                FertileWindowStart = ovulationDate.AddDays(-5),
                FertileWindowEnd = ovulationDate.AddDays(1),
                OvulationDate = ovulationDate,
                DaysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days,
                FertilityScore = CalculateFertilityScore(ovulationDate),
                FertilityPhase = GetFertilityPhase(ovulationDate),
                IsHighFertilityPeriod = Math.Abs((ovulationDate - DateTime.UtcNow).Days) <= 2,
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

    public async Task<bool> ScheduleNotificationsAsync(Guid trackingId)
    {
        try
        {
            var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(predicate: m => m.TrackingId == trackingId);

            if (tracking == null || !tracking.NotificationEnabled)
                return false;

            // Implement notification scheduling logic here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while scheduling notifications for tracking: {TrackingId}", trackingId);
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

    public async Task<ValidationResult> ValidateCycleDataAsync(CreateMenstrualCycleRequest request, Guid customerId)
    {
        try
        {
            var result = new ValidationResult { IsValid = true };

            if (request.CycleEndDate.HasValue && request.CycleStartDate >= request.CycleEndDate)
            {
                result.IsValid = false;
                result.Errors.Add("Cycle end date must be after start date");
            }

            if (request.CycleStartDate > DateTime.UtcNow)
            {
                result.IsValid = false;
                result.Errors.Add("Cycle start date cannot be in the future");
            }

            var existingCycle = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .FirstOrDefaultAsync(predicate: m =>
                    m.CustomerId == customerId &&
                    m.CycleStartDate <= (request.CycleEndDate ?? request.CycleStartDate.AddDays(7)) &&
                    (m.CycleEndDate == null || m.CycleEndDate >= request.CycleStartDate));

            if (existingCycle != null)
            {
                result.IsValid = false;
                result.Errors.Add("Cycle dates overlap with existing cycle");
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

    // Helper methods
    private double CalculateConfidenceScore(List<double> cycleLengths)
    {
        if (cycleLengths.Count < 2) return 0.5;
        
        var average = cycleLengths.Average();
        var variance = cycleLengths.Sum(l => Math.Pow(l - average, 2)) / cycleLengths.Count;
        var standardDeviation = Math.Sqrt(variance);
        
        return Math.Max(0.1, Math.Min(1.0, 1.0 - (standardDeviation / average)));
    }

    private string GetConfidenceLevel(double score)
    {
        return score switch
        {
            >= 0.8 => "High",
            >= 0.6 => "Medium",
            _ => "Low"
        };
    }

    private double CalculateFertilityScore(DateTime ovulationDate)
    {
        var daysFromOvulation = Math.Abs((DateTime.UtcNow - ovulationDate).Days);
        return Math.Max(0, 1.0 - (daysFromOvulation / 7.0));
    }

    private string GetFertilityPhase(DateTime ovulationDate)
    {
        var daysDiff = (DateTime.UtcNow - ovulationDate).Days;
        return daysDiff switch
        {
            < -5 => "Follicular",
            >= -5 and <= 1 => "Fertile Window",
            > 1 and <= 14 => "Luteal",
            _ => "Menstrual"
        };
    }

    private List<string> GenerateFertilityRecommendations(DateTime ovulationDate)
    {
        var recommendations = new List<string>();
        var daysToOvulation = (ovulationDate - DateTime.UtcNow).Days;

        if (daysToOvulation >= -1 && daysToOvulation <= 1)
        {
            recommendations.Add("You are in your most fertile period");
            recommendations.Add("Consider tracking basal body temperature");
        }
        else if (daysToOvulation > 1 && daysToOvulation <= 5)
        {
            recommendations.Add("Approaching fertile window");
            recommendations.Add("Start monitoring cervical mucus changes");
        }

        return recommendations;
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

    private float CalculateCycleRegularity(List<int> cycleLengths)
    {
        if (cycleLengths.Count < 2)
            return 1.0f;

        var average = cycleLengths.Average();
        var variance = cycleLengths.Sum(l => Math.Pow(l - average, 2)) / cycleLengths.Count;
        var standardDeviation = Math.Sqrt(variance);

        return (float)Math.Max(0.0, Math.Min(1.0, 1.0 - (standardDeviation / average)));
    }
}