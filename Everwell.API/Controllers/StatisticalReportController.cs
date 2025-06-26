using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Responses.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    public class StatisticalReportController : ControllerBase
    {
        private readonly IStatisticalReportService _reportService;
        private readonly ILogger<StatisticalReportController> _logger;

        public StatisticalReportController(
            IStatisticalReportService reportService,
            ILogger<StatisticalReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpPost(ApiEndpointConstants.Reports.GenerateReportEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<StatisticalReportResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<StatisticalReportResponse>> GenerateReport([FromBody] ReportFilter filter)
        {
            try
            {
                var report = await _reportService.GenerateReportAsync(filter);
                
                var apiResponse = new ApiResponse<StatisticalReportResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Báo cáo được tạo thành công",
                    IsSuccess = true,
                    Data = report
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating statistical report");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo báo cáo", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.GenerateDailyReportEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<StatisticalReportResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<StatisticalReportResponse>> GenerateDailyReport([FromRoute] DateTime date)
        {
            try
            {
                var report = await _reportService.GenerateDailyReportAsync(date);
                
                var apiResponse = new ApiResponse<StatisticalReportResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Báo cáo ngày được tạo thành công",
                    IsSuccess = true,
                    Data = report
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily report");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo báo cáo ngày", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.GenerateMonthlyReportEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<StatisticalReportResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<StatisticalReportResponse>> GenerateMonthlyReport([FromRoute] int year, [FromRoute] int month)
        {
            try
            {
                var report = await _reportService.GenerateMonthlyReportAsync(year, month);
                
                var apiResponse = new ApiResponse<StatisticalReportResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Báo cáo tháng được tạo thành công",
                    IsSuccess = true,
                    Data = report
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly report");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo báo cáo tháng", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.GenerateYearlyReportEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<StatisticalReportResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<StatisticalReportResponse>> GenerateYearlyReport([FromRoute] int year)
        {
            try
            {
                var report = await _reportService.GenerateYearlyReportAsync(year);
                
                var apiResponse = new ApiResponse<StatisticalReportResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Báo cáo năm được tạo thành công",
                    IsSuccess = true,
                    Data = report
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating yearly report");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo báo cáo năm", details = ex.Message });
            }
        }

        [HttpPost(ApiEndpointConstants.Reports.ExportToPdfEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportToPdf([FromBody] ReportFilter filter)
        {
            try
            {
                var pdfBytes = await _reportService.GenerateAndExportToPdfAsync(filter);
                var fileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to PDF");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất PDF", details = ex.Message });
            }
        }

        [HttpPost(ApiEndpointConstants.Reports.ExportToExcelEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportToExcel([FromBody] ReportFilter filter)
        {
            try
            {
                var excelBytes = await _reportService.GenerateAndExportToExcelAsync(filter);
                var fileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to Excel");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất Excel", details = ex.Message });
            }
        }

        [HttpPost(ApiEndpointConstants.Reports.ExportReportToPdfEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportReportToPdf([FromBody] StatisticalReportResponse report)
        {
            try
            {
                var pdfBytes = await _reportService.ExportReportToPdfAsync(report);
                var fileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting existing report to PDF");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất báo cáo PDF", details = ex.Message });
            }
        }

        [HttpPost(ApiEndpointConstants.Reports.ExportReportToExcelEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportReportToExcel([FromBody] StatisticalReportResponse report)
        {
            try
            {
                var excelBytes = await _reportService.ExportReportToExcelAsync(report);
                var fileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting existing report to Excel");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất báo cáo Excel", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.GetReportTemplatesEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<List<string>>> GetReportTemplates()
        {
            try
            {
                var templates = await _reportService.GetAvailableReportTemplatesAsync();
                
                var apiResponse = new ApiResponse<List<string>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Danh sách mẫu báo cáo được tải thành công",
                    IsSuccess = true,
                    Data = templates
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report templates");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải mẫu báo cáo", details = ex.Message });
            }
        }

        [HttpPost(ApiEndpointConstants.Reports.GenerateFromTemplateEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<StatisticalReportResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<StatisticalReportResponse>> GenerateFromTemplate([FromQuery] string templateName, [FromBody] ReportFilter filter)
        {
            try
            {
                var report = await _reportService.GenerateReportFromTemplateAsync(templateName, filter);
                
                var apiResponse = new ApiResponse<StatisticalReportResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Báo cáo từ mẫu được tạo thành công",
                    IsSuccess = true,
                    Data = report
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report from template");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo báo cáo từ mẫu", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.ExportMonthlyToPdfEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportMonthlyToPdf([FromRoute] int year, [FromRoute] int month)
        {
            try
            {
                var report = await _reportService.GenerateMonthlyReportAsync(year, month);
                var pdfBytes = await _reportService.ExportReportToPdfAsync(report);
                var fileName = $"BaoCaoThang_{month:D2}_{year}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting monthly report to PDF");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất báo cáo tháng PDF", details = ex.Message });
            }
        }

        [HttpGet(ApiEndpointConstants.Reports.ExportMonthlyToExcelEndpoint)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> ExportMonthlyToExcel([FromRoute] int year, [FromRoute] int month)
        {
            try
            {
                var report = await _reportService.GenerateMonthlyReportAsync(year, month);
                var excelBytes = await _reportService.ExportReportToExcelAsync(report);
                var fileName = $"BaoCaoThang_{month:D2}_{year}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting monthly report to Excel");
                return StatusCode(500, new { message = "Lỗi hệ thống khi xuất báo cáo tháng Excel", details = ex.Message });
            }
        }
    }
} 