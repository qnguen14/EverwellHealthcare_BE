using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Everwell.BLL.Services;
using Everwell.BLL.Models.Reports;
using System.Security.Claims;
using System.Collections;

namespace Everwell.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Generate and download a report in Excel or PDF format
        /// </summary>
        /// <param name="request">Report generation parameters</param>
        /// <returns>File download with the requested report</returns>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
        {
            try
            {
                // Get the current user's name for audit purposes
                var generatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
                
                _logger.LogInformation($"Generating {request.ReportType} report in {request.Format} format by {generatedBy}");
                
                var result = await _reportService.GenerateReportAsync(request, generatedBy);
                
                _logger.LogInformation($"Successfully generated report: {result.FileName}");
                
                return File(
                    result.FileContent,
                    result.ContentType,
                    result.FileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { message = "Error generating report", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available report types
        /// </summary>
        /// <returns>List of available report types</returns>
        [HttpGet("types")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetReportTypes()
        {
            var reportTypes = Enum.GetValues<ReportType>()
                .Select(rt => new
                {
                    Value = rt.ToString(),
                    DisplayName = GetReportDisplayName(rt),
                    Description = GetReportTypeDescription(rt)
                })
                .ToList();

            return Ok(reportTypes);
        }

        /// <summary>
        /// Get report data in JSON format (without file download)
        /// </summary>
        /// <param name="reportType">Type of report to generate</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <param name="userId">User ID filter (optional)</param>
        /// <param name="serviceId">Service ID filter (optional)</param>
        /// <param name="consultantId">Consultant ID filter (optional)</param>
        /// <returns>Report data in JSON format</returns>
        [HttpGet("data/{reportType}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportData(
            ReportType reportType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? serviceId = null,
            [FromQuery] int? consultantId = null)
        {
            try
            {
                var request = new ReportRequest
                {
                    ReportType = reportType,
                    Format = ExportFormat.Excel, // Format doesn't matter for data-only requests
                };

                object data = reportType switch
                {
                    ReportType.UserStatistics => await _reportService.GetReportDataAsync<UserStatisticsReportData>(reportType, request),
                    ReportType.AppointmentStatistics => await _reportService.GetReportDataAsync<AppointmentStatisticsReportData>(reportType, request),
                    ReportType.ServiceUtilization => await _reportService.GetReportDataAsync<ServiceUtilizationReportData>(reportType, request),
                    ReportType.FeedbackAnalysis => await _reportService.GetReportDataAsync<FeedbackAnalysisReportData>(reportType, request),
                    ReportType.STITestingReports => await _reportService.GetReportDataAsync<STITestingReportData>(reportType, request),
                    ReportType.MenstrualTrackingReports => await _reportService.GetReportDataAsync<MenstrualTrackingReportData>(reportType, request),
                    ReportType.PostEngagement => await _reportService.GetReportDataAsync<PostEngagementReportData>(reportType, request),
                    _ => throw new NotSupportedException($"Report type {reportType} is not supported")
                };

                return Ok(new 
                { 
                    reportType = reportType.ToString(), 
                    data, 
                    totalRecords = data is IList list ? list.Count : 0,
                    generatedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting report data for {reportType}");
                return StatusCode(500, new { message = "Error retrieving report data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get quick statistics for dashboard
        /// </summary>
        /// <returns>Quick statistics summary</returns>
        
        /*[HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // You can implement these methods to get actual counts from your database
                var stats = new
                {
                    TotalUsers = await GetUserCountAsync(),
                    TotalAppointments = await GetAppointmentCountAsync(),
                    TotalServices = await GetServiceCountAsync(),
                    TotalFeedbacks = await GetFeedbackCountAsync(),
                    GeneratedAt = DateTime.Now
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { message = "Error retrieving dashboard statistics", error = ex.Message });
            }
        }
        */

        // Helper methods for dashboard stats - you can implement these properly later
        private async Task<int> GetUserCountAsync()
        {
            // TODO: Implement actual user count from your service
            return 100; // Placeholder
        }

        private async Task<int> GetAppointmentCountAsync()
        {
            // TODO: Implement actual appointment count from your service
            return 250; // Placeholder
        }

        private async Task<int> GetServiceCountAsync()
        {
            // TODO: Implement actual service count from your service
            return 15; // Placeholder
        }

        private async Task<int> GetFeedbackCountAsync()
        {
            // TODO: Implement actual feedback count from your service
            return 75; // Placeholder
        }

        private string GetReportDisplayName(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.UserStatistics => "User Statistics",
                ReportType.AppointmentStatistics => "Appointment Statistics",
                ReportType.ServiceUtilization => "Service Utilization",
                ReportType.FeedbackAnalysis => "Feedback Analysis",
                ReportType.STITestingReports => "STI Testing Reports",
                ReportType.MenstrualTrackingReports => "Menstrual Tracking Reports",
                ReportType.PostEngagement => "Post Engagement",
                _ => reportType.ToString()
            };
        }

        private string GetReportTypeDescription(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.UserStatistics => "User registration, activity, and engagement statistics",
                ReportType.AppointmentStatistics => "Appointment booking trends and consultant utilization",
                ReportType.ServiceUtilization => "Service usage patterns and revenue analysis",
                ReportType.FeedbackAnalysis => "Customer satisfaction and feedback analytics",
                ReportType.STITestingReports => "STI testing records and health outcomes",
                ReportType.MenstrualTrackingReports => "Menstrual health tracking and patterns",
                ReportType.PostEngagement => "Content performance and user engagement metrics",
                _ => "Detailed system report"
            };
        }
    }
} 