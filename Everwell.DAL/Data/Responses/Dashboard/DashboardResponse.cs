using Everwell.DAL.Data.Responses.User;
using Everwell.DAL.Data.Responses.Appointments;

namespace Everwell.DAL.Data.Responses.Dashboard
{
    public class DashboardResponse
    {
        public DashboardStats Stats { get; set; }
        public IEnumerable<CreateUserResponse> RecentUsers { get; set; }
        public IEnumerable<CreateAppointmentsResponse> RecentAppointments { get; set; }
        public IEnumerable<UserRoleCount> UsersByRole { get; set; }
        public IEnumerable<AppointmentStatusCount> AppointmentsByStatus { get; set; }
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int AppointmentsThisMonth { get; set; }
    }

    public class UserRoleCount
    {
        public string Role { get; set; }
        public int Count { get; set; }
    }

    public class AppointmentStatusCount
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
} 