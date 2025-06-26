using Everwell.DAL.Data.Responses.Reports;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IStatisticalReportService
    {
        // Generate statistical reports
        Task<StatisticalReportResponse> GenerateReportAsync(ReportFilter filter);
        Task<StatisticalReportResponse> GenerateDailyReportAsync(DateTime date);
        Task<StatisticalReportResponse> GenerateWeeklyReportAsync(DateTime weekStart);
        Task<StatisticalReportResponse> GenerateMonthlyReportAsync(int year, int month);
        Task<StatisticalReportResponse> GenerateQuarterlyReportAsync(int year, int quarter);
        Task<StatisticalReportResponse> GenerateYearlyReportAsync(int year);

        // Export functionality
        Task<byte[]> ExportReportToPdfAsync(StatisticalReportResponse report);
        Task<byte[]> ExportReportToExcelAsync(StatisticalReportResponse report);
        
        // Quick export methods
        Task<byte[]> GenerateAndExportToPdfAsync(ReportFilter filter);
        Task<byte[]> GenerateAndExportToExcelAsync(ReportFilter filter);
        
        // Template management
        Task<List<string>> GetAvailableReportTemplatesAsync();
        Task<StatisticalReportResponse> GenerateReportFromTemplateAsync(string templateName, ReportFilter filter);
    }
} 