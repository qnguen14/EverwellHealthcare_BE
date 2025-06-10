using System.ComponentModel.DataAnnotations;

namespace Everwell.BLL.Models.Reports
{
    public enum ReportType
    {
        UserStatistics,           // Admin: Monitor user registrations, activity, roles
        AppointmentStatistics,    // Admin: Track appointment trends, consultant utilization
        ServiceUtilization,       // Admin: Revenue analysis, popular services
        FeedbackAnalysis,         // Admin: Customer satisfaction metrics
        STITestingReports,        // Admin: Health service utilization, compliance
        MenstrualTrackingReports, // Admin: Women's health program effectiveness
        PostEngagement           // Admin: Content performance, user engagement
    }

    public enum ExportFormat
    {
        Excel,
        PDF
    }

    public class ReportRequest
    {
        [Required]
        public ReportType ReportType { get; set; }
        
        [Required]
        public ExportFormat Format { get; set; }
    }

    public class ReportResult
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public ReportMetadata Metadata { get; set; } = new();
    }

    public class ReportMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }

    // User Statistics Report Models
    public class UserStatisticsReportData
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalPosts { get; set; }
        public int TotalQuestions { get; set; }
    }

    // Appointment Statistics Report Models
    public class AppointmentStatisticsReportData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string ConsultantName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // Service Utilization Report Models
    public class ServiceUtilizationReportData
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsActive { get; set; }
    }

    // Feedback Analysis Report Models
    public class FeedbackAnalysisReportData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string ConsultantName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // STI Testing Report Models
    public class STITestingReportData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // Menstrual Tracking Report Models
    public class MenstrualTrackingReportData
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CycleLength { get; set; }
        public string FlowIntensity { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // Post Engagement Report Models
    public class PostEngagementReportData
    {
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsActive { get; set; }
    }
} 