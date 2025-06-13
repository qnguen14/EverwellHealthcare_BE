using System;
using System.Collections.Generic;

namespace Everwell.DAL.Data.Responses.MenstrualCycle
{
    public class CreateMenstrualCycleResponse
    {
        public Guid TrackingId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
        public string? Symptoms { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool NotificationEnabled { get; set; }
        public int? NotifyBeforeDays { get; set; }
    }

    public class GetMenstrualCycleResponse
    {
        public Guid TrackingId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
        public string? Symptoms { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool NotificationEnabled { get; set; }
        public int? NotifyBeforeDays { get; set; }
        public List<NotificationResponse> Notifications { get; set; } = new();
    }

    public class CyclePredictionResponse
    {
        public DateTime PredictedNextPeriodStart { get; set; }
        public DateTime PredictedNextPeriodEnd { get; set; }
        public int PredictedCycleLength { get; set; }
        public int PredictedPeriodLength { get; set; }
        public double ConfidenceScore { get; set; }
        public string ConfidenceLevel { get; set; }
        public List<string> Factors { get; set; } = new();
        public bool IsRegularCycle { get; set; }
        public DateTime CalculatedOn { get; set; } = DateTime.UtcNow;
    }

    public class FertilityWindowResponse
    {
        public DateTime FertileWindowStart { get; set; }
        public DateTime FertileWindowEnd { get; set; }
        public DateTime OvulationDate { get; set; }
        public int DaysUntilOvulation { get; set; }
        public double FertilityScore { get; set; }
        public string FertilityPhase { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public bool IsHighFertilityPeriod { get; set; }
    }

    public class CycleAnalyticsResponse
    {
        public double AverageCycleLength { get; set; }
        public double AveragePeriodLength { get; set; }
        public int TotalCyclesTracked { get; set; }
        public DateTime FirstCycleDate { get; set; }
        public DateTime LastCycleDate { get; set; }
        public double CycleRegularityScore { get; set; }
        public List<string> CommonSymptoms { get; set; } = new();
        public Dictionary<string, int> SymptomFrequency { get; set; } = new();
        public List<CycleLengthData> CycleLengthHistory { get; set; } = new();
    }

    public class CycleInsightsResponse
    {
        public string OverallHealthStatus { get; set; }
        public List<string> Insights { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public List<string> HealthAlerts { get; set; } = new();
        public Dictionary<string, object> HealthMetrics { get; set; } = new();
        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
    }

    public class CycleTrendData
    {
        public DateTime Date { get; set; }
        public int CycleLength { get; set; }
        public int PeriodLength { get; set; }
        public List<string> Symptoms { get; set; } = new();
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CycleLengthData
    {
        public DateTime CycleStart { get; set; }
        public int Length { get; set; }
        public bool IsComplete { get; set; }
    }

    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; }
        public DateTime NotificationDate { get; set; }
        public string Message { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
