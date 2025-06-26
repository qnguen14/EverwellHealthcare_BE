using Everwell.DAL.Data.Responses.Dashboard;

namespace Everwell.DAL.Data.Responses.Reports
{
    public class StatisticalReportResponse
    {
        public ReportHeader Header { get; set; }
        public ReportSummary Summary { get; set; }
        public UserStatistics UserStats { get; set; }
        public AppointmentStatistics AppointmentStats { get; set; }
        public STITestingStatistics STIStats { get; set; }
        public PaymentStatistics PaymentStats { get; set; }
        public MonthlyTrends MonthlyTrends { get; set; }
        public List<TopConsultant> TopConsultants { get; set; }
        public SystemPerformance Performance { get; set; }
    }

    public class ReportHeader
    {
        public string Title { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GeneratedBy { get; set; }
        public string ReportType { get; set; }
    }

    public class ReportSummary
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisPeriod { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int TotalSTITests { get; set; }
        public int CompletedSTITests { get; set; }
        public decimal RevenueTotalEstimate { get; set; }
        public double AppointmentCompletionRate { get; set; }
        public double UserRetentionRate { get; set; }
    }

    public class UserStatistics
    {
        public List<UserRoleCount> UsersByRole { get; set; }
        public List<UserAgeGroup> UsersByAge { get; set; }
        public List<UserGenderCount> UsersByGender { get; set; }
        public List<UserLocationCount> UsersByLocation { get; set; }
        public List<UserRegistrationTrend> RegistrationTrends { get; set; }
    }

    public class AppointmentStatistics
    {
        public List<AppointmentStatusCount> AppointmentsByStatus { get; set; }
        public List<AppointmentByConsultant> AppointmentsByConsultant { get; set; }
        public List<AppointmentByTimeSlot> AppointmentsByTimeSlot { get; set; }
        public List<AppointmentMonthlyCount> MonthlyAppointments { get; set; }
        public List<AppointmentTypeCount> AppointmentsByType { get; set; }
    }

    public class STITestingStatistics
    {
        public List<STITestByType> TestsByType { get; set; }
        public List<STITestByStatus> TestsByStatus { get; set; }
        public List<STITestMonthlyCount> MonthlyTests { get; set; }
        public List<STIResultStatistic> ResultStatistics { get; set; }
    }

    public class PaymentStatistics
    {
        public PaymentOverview Overview { get; set; }
        public List<PaymentByService> PaymentsByService { get; set; }
        public List<PaymentByMethod> PaymentsByMethod { get; set; }
        public List<PaymentSuccessRate> SuccessRates { get; set; }
        public List<MonthlyPaymentMetric> MonthlyPayments { get; set; }
        public List<PaymentFailureReason> FailureReasons { get; set; }
        public RevenueAnalytics RevenueAnalytics { get; set; }
    }

    public class MonthlyTrends
    {
        public List<MonthlyMetric> UserRegistrations { get; set; }
        public List<MonthlyMetric> AppointmentBookings { get; set; }
        public List<MonthlyMetric> STITestRequests { get; set; }
        public List<MonthlyMetric> CompletedServices { get; set; }
    }

    public class SystemPerformance
    {
        public double AverageResponseTime { get; set; }
        public double SystemUptime { get; set; }
        public int TotalAPIRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public double SuccessRate { get; set; }
    }

    // Supporting classes
    public class UserAgeGroup
    {
        public string AgeRange { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class UserGenderCount
    {
        public string Gender { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class UserLocationCount
    {
        public string Location { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class UserRegistrationTrend
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int CumulativeCount { get; set; }
    }

    public class AppointmentByConsultant
    {
        public string ConsultantName { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public double CompletionRate { get; set; }
        public double AverageRating { get; set; }
    }

    public class AppointmentByTimeSlot
    {
        public string TimeSlot { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class AppointmentMonthlyCount
    {
        public DateTime Month { get; set; }
        public int Count { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class AppointmentTypeCount
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class STITestByType
    {
        public string TestType { get; set; }
        public int Count { get; set; }
        public int PositiveResults { get; set; }
        public double PositiveRate { get; set; }
    }

    public class STITestByStatus
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class STITestMonthlyCount
    {
        public DateTime Month { get; set; }
        public int TestsRequested { get; set; }
        public int TestsCompleted { get; set; }
        public int PositiveResults { get; set; }
    }

    public class STIResultStatistic
    {
        public string ResultType { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopConsultant
    {
        public string Name { get; set; }
        public string Specialization { get; set; }
        public int TotalAppointments { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal EstimatedRevenue { get; set; }
    }

    public class MonthlyMetric
    {
        public DateTime Month { get; set; }
        public int Count { get; set; }
        public double PercentageChange { get; set; }
    }

    public enum ReportType
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Custom
    }

    public class ReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ReportType ReportType { get; set; }
        public List<string> IncludeRoles { get; set; } = new List<string>();
        public List<string> IncludeConsultants { get; set; } = new List<string>();
        public bool IncludeUserStats { get; set; } = true;
        public bool IncludeAppointmentStats { get; set; } = true;
        public bool IncludeSTIStats { get; set; } = true;
        public bool IncludePaymentStats { get; set; } = true;
        public bool IncludeTrends { get; set; } = true;
        public bool IncludePerformance { get; set; } = true;
    }

    // Payment-specific classes
    public class PaymentOverview
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public double SuccessRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal STIRevenue { get; set; }
        public decimal MenstrualRevenue { get; set; }
        public decimal AppointmentRevenue { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }

    public class PaymentByService
    {
        public string ServiceType { get; set; } // STI, Menstrual, Appointment
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double SuccessRate { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class PaymentByMethod
    {
        public string PaymentMethod { get; set; } // VnPay, Cash, etc.
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public double SuccessRate { get; set; }
        public double Percentage { get; set; }
    }

    public class PaymentSuccessRate
    {
        public string ServiceCategory { get; set; }
        public string Period { get; set; } // Daily, Weekly, Monthly
        public double SuccessRate { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
    }

    public class MonthlyPaymentMetric
    {
        public DateTime Month { get; set; }
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public decimal Revenue { get; set; }
        public double SuccessRate { get; set; }
        public int STIPayments { get; set; }
        public int MenstrualPayments { get; set; }
        public int AppointmentPayments { get; set; }
    }

    public class PaymentFailureReason
    {
        public string Reason { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal LostRevenue { get; set; }
    }

    public class RevenueAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public decimal STITestingRevenue { get; set; }
        public decimal MenstrualCycleRevenue { get; set; }
        public decimal AppointmentRevenue { get; set; }
        public decimal MonthOverMonthGrowth { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public TopRevenueService TopService { get; set; }
        public List<RevenueByPackage> STIPackageRevenue { get; set; }
    }

    public class TopRevenueService
    {
        public string ServiceName { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
    }

    public class RevenueByPackage
    {
        public string PackageName { get; set; }
        public decimal Revenue { get; set; }
        public int SalesCount { get; set; }
        public decimal AveragePrice { get; set; }
        public double PopularityPercentage { get; set; }
    }
} 