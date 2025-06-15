using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Responses.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet(ApiEndpointConstants.Dashboard.GetDashboardDataEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant")]
        public async Task<ActionResult<DashboardResponse>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                
                var apiResponse = new ApiResponse<DashboardResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Dashboard data retrieved successfully",
                    IsSuccess = true,
                    Data = dashboardData
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard data");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Dashboard.GetDashboardStatsEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<DashboardStats>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                
                var apiResponse = new ApiResponse<DashboardStats>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Dashboard stats retrieved successfully",
                    IsSuccess = true,
                    Data = stats
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard stats");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Dashboard.GetUsersByRoleEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserRoleCount>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserRoleCount>>> GetUsersByRole()
        {
            try
            {
                var usersByRole = await _dashboardService.GetUsersByRoleAsync();
                
                var apiResponse = new ApiResponse<IEnumerable<UserRoleCount>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Users by role retrieved successfully",
                    IsSuccess = true,
                    Data = usersByRole
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users by role");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Dashboard.GetAppointmentsByStatusEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentStatusCount>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AppointmentStatusCount>>> GetAppointmentsByStatus()
        {
            try
            {
                var appointmentsByStatus = await _dashboardService.GetAppointmentsByStatusAsync();
                
                var apiResponse = new ApiResponse<IEnumerable<AppointmentStatusCount>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Appointments by status retrieved successfully",
                    IsSuccess = true,
                    Data = appointmentsByStatus
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting appointments by status");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
} 