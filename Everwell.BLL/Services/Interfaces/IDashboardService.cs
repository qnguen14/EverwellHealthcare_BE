using Everwell.DAL.Data.Responses.Dashboard;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardDataAsync();
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<IEnumerable<UserRoleCount>> GetUsersByRoleAsync();
        Task<IEnumerable<AppointmentStatusCount>> GetAppointmentsByStatusAsync();
    }
} 