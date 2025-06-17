using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.Dashboard;
using Everwell.DAL.Data.Responses.User;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements
{
    public class DashboardService : BaseService<DashboardService>, IDashboardService
    {
        public DashboardService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<DashboardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<DashboardResponse> GetDashboardDataAsync()
        {
            try
            {
                var stats = await GetDashboardStatsAsync();
                var recentUsers = await GetRecentUsersAsync();
                var recentAppointments = await GetRecentAppointmentsAsync();
                var usersByRole = await GetUsersByRoleAsync();
                var appointmentsByStatus = await GetAppointmentsByStatusAsync();

                return new DashboardResponse
                {
                    Stats = stats,
                    RecentUsers = recentUsers,
                    RecentAppointments = recentAppointments,
                    UsersByRole = usersByRole,
                    AppointmentsByStatus = appointmentsByStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard data");
                throw;
            }
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

                var userRepo = _unitOfWork.GetRepository<User>();
                var appointmentRepo = _unitOfWork.GetRepository<Appointment>();

                // Đếm tổng số users
                var totalUsers = await userRepo.CountAsync(predicate: u => u.IsActive);
                var totalActiveUsers = await userRepo.CountAsync(predicate: u => u.IsActive);
                
                // Tạm thời sử dụng appointments trong tháng làm proxy cho users mới
                // Hoặc có thể đếm users có appointments mới trong tháng
                var usersWithAppointmentsThisMonth = await appointmentRepo
                    .GetListAsync(
                        predicate: a => a.AppointmentDate >= firstDayOfMonth && a.AppointmentDate <= today,
                        include: a => a.Include(ap => ap.Customer))
                    .ContinueWith(t => t.Result.Select(a => a.CustomerId).Distinct().Count());

                // Đếm tổng số appointments
                var totalAppointments = await appointmentRepo.CountAsync();
                
                // Đếm appointments hôm nay
                var todayAppointments = await appointmentRepo.CountAsync(
                    predicate: a => a.AppointmentDate == today);

                // Đếm appointments theo status
                var pendingAppointments = await appointmentRepo.CountAsync(
                    predicate: a => a.Status == AppointmentStatus.Scheduled);
                
                var completedAppointments = await appointmentRepo.CountAsync(
                    predicate: a => a.Status == AppointmentStatus.Completed);
                    
                var cancelledAppointments = await appointmentRepo.CountAsync(
                    predicate: a => a.Status == AppointmentStatus.Cancelled);

                // Đếm appointments trong tháng này
                var appointmentsThisMonth = await appointmentRepo.CountAsync(
                    predicate: a => a.AppointmentDate >= firstDayOfMonth && a.AppointmentDate <= today);

                return new DashboardStats
                {
                    TotalUsers = totalUsers,
                    TotalAppointments = totalAppointments,
                    TotalActiveUsers = totalActiveUsers,
                    TodayAppointments = todayAppointments,
                    PendingAppointments = pendingAppointments,
                    CompletedAppointments = completedAppointments,
                    CancelledAppointments = cancelledAppointments,
                    NewUsersThisMonth = usersWithAppointmentsThisMonth, // Tạm thời
                    AppointmentsThisMonth = appointmentsThisMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard stats");
                throw;
            }
        }

        public async Task<IEnumerable<UserRoleCount>> GetUsersByRoleAsync()
        {
            try
            {
                var usersByRole = await _unitOfWork.GetRepository<User>()
                    .GetListAsync(predicate: u => u.IsActive);

                return usersByRole
                    .GroupBy(u => u.Role)
                    .Select(g => new UserRoleCount
                    {
                        Role = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users by role");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentStatusCount>> GetAppointmentsByStatusAsync()
        {
            try
            {
                var appointments = await _unitOfWork.GetRepository<Appointment>()
                    .GetListAsync();

                return appointments
                    .GroupBy(a => a.Status)
                    .Select(g => new AppointmentStatusCount
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting appointments by status");
                throw;
            }
        }

        private async Task<IEnumerable<CreateUserResponse>> GetRecentUsersAsync()
        {
            try
            {
                var recentUsers = await _unitOfWork.GetRepository<User>()
                    .GetListAsync(
                        predicate: u => u.IsActive,
                        orderBy: u => u.OrderByDescending(x => x.Id),
                        take: 10
                    );

                return _mapper.Map<IEnumerable<CreateUserResponse>>(recentUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recent users");
                throw;
            }
        }

        private async Task<IEnumerable<CreateAppointmentsResponse>> GetRecentAppointmentsAsync()
        {
            try
            {
                var recentAppointments = await _unitOfWork.GetRepository<Appointment>()
                    .GetListAsync(
                        include: a => a.Include(ap => ap.Customer)
                                     .Include(ap => ap.Consultant),
                                     // .Include(ap => ap.Service),
                        orderBy: a => a.OrderByDescending(x => x.CreatedAt),
                        take: 10
                    );

                return _mapper.Map<IEnumerable<CreateAppointmentsResponse>>(recentAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recent appointments");
                throw;
            }
        }
    }
} 