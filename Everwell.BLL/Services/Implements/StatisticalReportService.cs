using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.Reports;
using Everwell.DAL.Data.Responses.Dashboard;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Security.Claims;

namespace Everwell.BLL.Services.Implements
{
    public class StatisticalReportService : BaseService<StatisticalReportService>, IStatisticalReportService
    {
        public StatisticalReportService(
            IUnitOfWork<EverwellDbContext> unitOfWork, 
            ILogger<StatisticalReportService> logger, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<StatisticalReportResponse> GenerateReportAsync(ReportFilter filter)
        {
            try
            {
                var fromDate = filter.FromDate ?? DateTime.Now.AddMonths(-1);
                var toDate = filter.ToDate ?? DateTime.Now;

                var report = new StatisticalReportResponse
                {
                    Header = new ReportHeader
                    {
                        Title = $"Báo cáo thống kê từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}",
                        GeneratedAt = DateTime.Now,
                        FromDate = fromDate,
                        ToDate = toDate,
                        GeneratedBy = GetCurrentUserName(),
                        ReportType = filter.ReportType.ToString()
                    },
                    Summary = await GenerateReportSummaryAsync(fromDate, toDate),
                    UserStats = await GenerateUserStatisticsAsync(fromDate, toDate),
                    AppointmentStats = await GenerateAppointmentStatisticsAsync(fromDate, toDate),
                    PaymentStats = filter.IncludePaymentStats ? await GetPaymentStatisticsAsync(fromDate, toDate) : null,
                    TopConsultants = await GenerateTopConsultantsAsync(fromDate, toDate)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating statistical report");
                throw;
            }
        }

        public async Task<StatisticalReportResponse> GenerateDailyReportAsync(DateTime date)
        {
            var filter = new ReportFilter
            {
                FromDate = date.Date,
                ToDate = date.Date.AddDays(1).AddTicks(-1),
                ReportType = ReportType.Daily
            };
            return await GenerateReportAsync(filter);
        }

        public async Task<StatisticalReportResponse> GenerateWeeklyReportAsync(DateTime weekStart)
        {
            var filter = new ReportFilter
            {
                FromDate = weekStart.Date,
                ToDate = weekStart.Date.AddDays(7).AddTicks(-1),
                ReportType = ReportType.Weekly
            };
            return await GenerateReportAsync(filter);
        }

        public async Task<StatisticalReportResponse> GenerateMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);
            
            var filter = new ReportFilter
            {
                FromDate = startDate,
                ToDate = endDate,
                ReportType = ReportType.Monthly
            };
            return await GenerateReportAsync(filter);
        }

        public async Task<StatisticalReportResponse> GenerateQuarterlyReportAsync(int year, int quarter)
        {
            var startMonth = (quarter - 1) * 3 + 1;
            var startDate = new DateTime(year, startMonth, 1);
            var endDate = startDate.AddMonths(3).AddTicks(-1);
            
            var filter = new ReportFilter
            {
                FromDate = startDate,
                ToDate = endDate,
                ReportType = ReportType.Quarterly
            };
            return await GenerateReportAsync(filter);
        }

        public async Task<StatisticalReportResponse> GenerateYearlyReportAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);
            
            var filter = new ReportFilter
            {
                FromDate = startDate,
                ToDate = endDate,
                ReportType = ReportType.Yearly
            };
            return await GenerateReportAsync(filter);
        }

        public async Task<byte[]> ExportReportToPdfAsync(StatisticalReportResponse report)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 36, 36, 54, 54);
                    var writer = PdfWriter.GetInstance(document, stream);
                    
                    document.Open();

                    // Add title
                    var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                    var title = new Paragraph(report.Header.Title, titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(title);
                    document.Add(new Paragraph(" "));

                    // Add report info
                    var normalFont = FontFactory.GetFont("Arial", 10);
                    document.Add(new Paragraph($"Ngày tạo: {report.Header.GeneratedAt:dd/MM/yyyy HH:mm}", normalFont));
                    document.Add(new Paragraph($"Người tạo: {report.Header.GeneratedBy}", normalFont));
                    document.Add(new Paragraph(" "));

                    // Add summary section
                    if (report.Summary != null)
                    {
                        var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                        document.Add(new Paragraph("TỔNG QUAN", headerFont));
                        
                        var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
                        AddTableRow(summaryTable, "Tổng số người dùng", report.Summary.TotalUsers.ToString(), normalFont);
                        AddTableRow(summaryTable, "Người dùng mới", report.Summary.NewUsersThisPeriod.ToString(), normalFont);
                        AddTableRow(summaryTable, "Tổng lịch hẹn", report.Summary.TotalAppointments.ToString(), normalFont);
                        AddTableRow(summaryTable, "Lịch hẹn hoàn thành", report.Summary.CompletedAppointments.ToString(), normalFont);
                        
                        document.Add(summaryTable);
                    }

                    // Add payment statistics section
                    if (report.PaymentStats != null)
                    {
                        document.Add(new Paragraph(" "));
                        var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                        document.Add(new Paragraph("THỐNG KÊ THANH TOÁN", headerFont));
                        
                        var paymentTable = new PdfPTable(2) { WidthPercentage = 100 };
                        AddTableRow(paymentTable, "Tổng giao dịch", report.PaymentStats.Overview.TotalPayments.ToString(), normalFont);
                        AddTableRow(paymentTable, "Giao dịch thành công", report.PaymentStats.Overview.SuccessfulPayments.ToString(), normalFont);
                        AddTableRow(paymentTable, "Tỷ lệ thành công", $"{report.PaymentStats.Overview.SuccessRate:F1}%", normalFont);
                        AddTableRow(paymentTable, "Tổng doanh thu", $"{report.PaymentStats.Overview.TotalRevenue:N0} VNĐ", normalFont);
                        AddTableRow(paymentTable, "Doanh thu STI", $"{report.PaymentStats.Overview.STIRevenue:N0} VNĐ", normalFont);
                        AddTableRow(paymentTable, "Doanh thu lịch hẹn", $"{report.PaymentStats.Overview.AppointmentRevenue:N0} VNĐ", normalFont);
                        
                        document.Add(paymentTable);
                    }

                    document.Close();
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to PDF");
                throw;
            }
        }

        public async Task<byte[]> ExportReportToExcelAsync(StatisticalReportResponse report)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    // Summary sheet
                    var summarySheet = workbook.Worksheets.Add("Tổng quan");
                    
                    summarySheet.Cell(1, 1).Value = report.Header.Title;
                    summarySheet.Cell(1, 1).Style.Font.FontSize = 16;
                    summarySheet.Cell(1, 1).Style.Font.Bold = true;

                    summarySheet.Cell(3, 1).Value = "Ngày tạo:";
                    summarySheet.Cell(3, 2).Value = report.Header.GeneratedAt.ToString("dd/MM/yyyy HH:mm");

                    if (report.Summary != null)
                    {
                        var row = 5;
                        summarySheet.Cell(row, 1).Value = "Tổng số người dùng:";
                        summarySheet.Cell(row, 2).Value = report.Summary.TotalUsers;
                        
                        row++;
                        summarySheet.Cell(row, 1).Value = "Người dùng mới:";
                        summarySheet.Cell(row, 2).Value = report.Summary.NewUsersThisPeriod;
                        
                        row++;
                        summarySheet.Cell(row, 1).Value = "Tổng lịch hẹn:";
                        summarySheet.Cell(row, 2).Value = report.Summary.TotalAppointments;
                        
                        row++;
                        summarySheet.Cell(row, 1).Value = "Lịch hẹn hoàn thành:";
                        summarySheet.Cell(row, 2).Value = report.Summary.CompletedAppointments;
                    }

                    // Payment Statistics sheet
                    if (report.PaymentStats != null)
                    {
                        var paymentSheet = workbook.Worksheets.Add("Thống kê thanh toán");
                        
                        paymentSheet.Cell(1, 1).Value = "THỐNG KÊ THANH TOÁN";
                        paymentSheet.Cell(1, 1).Style.Font.FontSize = 14;
                        paymentSheet.Cell(1, 1).Style.Font.Bold = true;

                        var row = 3;
                        paymentSheet.Cell(row, 1).Value = "Tổng quan";
                        paymentSheet.Cell(row, 1).Style.Font.Bold = true;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Tổng giao dịch:";
                        paymentSheet.Cell(row, 2).Value = report.PaymentStats.Overview.TotalPayments;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Giao dịch thành công:";
                        paymentSheet.Cell(row, 2).Value = report.PaymentStats.Overview.SuccessfulPayments;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Tỷ lệ thành công (%):";
                        paymentSheet.Cell(row, 2).Value = report.PaymentStats.Overview.SuccessRate;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Tổng doanh thu (VNĐ):";
                        paymentSheet.Cell(row, 2).Value = (double)report.PaymentStats.Overview.TotalRevenue;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Doanh thu STI (VNĐ):";
                        paymentSheet.Cell(row, 2).Value = (double)report.PaymentStats.Overview.STIRevenue;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Doanh thu lịch hẹn (VNĐ):";
                        paymentSheet.Cell(row, 2).Value = (double)report.PaymentStats.Overview.AppointmentRevenue;

                        // Payment by Service
                        row += 2;
                        paymentSheet.Cell(row, 1).Value = "Thanh toán theo dịch vụ";
                        paymentSheet.Cell(row, 1).Style.Font.Bold = true;
                        
                        row++;
                        paymentSheet.Cell(row, 1).Value = "Dịch vụ";
                        paymentSheet.Cell(row, 2).Value = "Tổng giao dịch";
                        paymentSheet.Cell(row, 3).Value = "Thành công";
                        paymentSheet.Cell(row, 4).Value = "Tỷ lệ thành công (%)";
                        paymentSheet.Cell(row, 5).Value = "Doanh thu (VNĐ)";
                        
                        foreach (var service in report.PaymentStats.PaymentsByService)
                        {
                            row++;
                            paymentSheet.Cell(row, 1).Value = service.ServiceType;
                            paymentSheet.Cell(row, 2).Value = service.TotalPayments;
                            paymentSheet.Cell(row, 3).Value = service.SuccessfulPayments;
                            paymentSheet.Cell(row, 4).Value = service.SuccessRate;
                            paymentSheet.Cell(row, 5).Value = (double)service.TotalRevenue;
                        }
                    }

                    // User statistics sheet
                    if (report.UserStats?.UsersByRole != null)
                    {
                        var userSheet = workbook.Worksheets.Add("Thống kê người dùng");
                        
                        userSheet.Cell(1, 1).Value = "Vai trò";
                        userSheet.Cell(1, 2).Value = "Số lượng";
                        userSheet.Range(1, 1, 1, 2).Style.Font.Bold = true;

                        var row = 2;
                        foreach (var role in report.UserStats.UsersByRole)
                        {
                            userSheet.Cell(row, 1).Value = role.Role;
                            userSheet.Cell(row, 2).Value = role.Count;
                            row++;
                        }
                    }

                    summarySheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to Excel");
                throw;
            }
        }

        public async Task<byte[]> GenerateAndExportToPdfAsync(ReportFilter filter)
        {
            var report = await GenerateReportAsync(filter);
            return await ExportReportToPdfAsync(report);
        }

        public async Task<byte[]> GenerateAndExportToExcelAsync(ReportFilter filter)
        {
            var report = await GenerateReportAsync(filter);
            return await ExportReportToExcelAsync(report);
        }

        public async Task<List<string>> GetAvailableReportTemplatesAsync()
        {
            return new List<string>
            {
                "Báo cáo tổng quan hệ thống",
                "Báo cáo hiệu suất tư vấn viên",
                "Báo cáo thống kê xét nghiệm STI",
                "Báo cáo xu hướng người dùng",
                "Báo cáo thống kê thanh toán"
            };
        }

        public async Task<StatisticalReportResponse> GenerateReportFromTemplateAsync(string templateName, ReportFilter filter)
        {
            return await GenerateReportAsync(filter);
        }

        // Helper methods
        private async Task<ReportSummary> GenerateReportSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var appointmentRepo = _unitOfWork.GetRepository<Appointment>();

            var totalUsers = await userRepo.CountAsync(u => u.IsActive);
            var totalAppointments = await appointmentRepo.CountAsync(a => 
                a.AppointmentDate >= DateOnly.FromDateTime(fromDate) && 
                a.AppointmentDate <= DateOnly.FromDateTime(toDate));

            var completedAppointments = await appointmentRepo.CountAsync(a => 
                a.Status == AppointmentStatus.Completed &&
                a.AppointmentDate >= DateOnly.FromDateTime(fromDate) && 
                a.AppointmentDate <= DateOnly.FromDateTime(toDate));

            return new ReportSummary
            {
                TotalUsers = totalUsers,
                NewUsersThisPeriod = 10, // Mock data
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                TotalSTITests = 50, // Mock data
                CompletedSTITests = 45, // Mock data
                AppointmentCompletionRate = totalAppointments > 0 ? (double)completedAppointments / totalAppointments : 0,
                UserRetentionRate = 0.85,
                RevenueTotalEstimate = completedAppointments * 500000
            };
        }

        private async Task<UserStatistics> GenerateUserStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var users = await userRepo.GetListAsync(
                predicate: u => u.IsActive,
                include: u => u.Include(user => user.Role)
            );

            return new UserStatistics
            {
                UsersByRole = users.GroupBy(u => u.Role.Name)
                    .Select(g => new UserRoleCount
                    {
                        Role = g.Key.ToString(),
                        Count = g.Count()
                    }).ToList(),
                UsersByAge = new List<UserAgeGroup>(),
                UsersByGender = new List<UserGenderCount>(),
                UsersByLocation = new List<UserLocationCount>(),
                RegistrationTrends = new List<UserRegistrationTrend>()
            };
        }

        private async Task<AppointmentStatistics> GenerateAppointmentStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            var appointmentRepo = _unitOfWork.GetRepository<Appointment>();
            var appointments = await appointmentRepo.GetListAsync(
                predicate: a => a.AppointmentDate >= DateOnly.FromDateTime(fromDate) && 
                            a.AppointmentDate <= DateOnly.FromDateTime(toDate),
                include: a => a.Include(ap => ap.Consultant)
            );

            return new AppointmentStatistics
            {
                AppointmentsByStatus = appointments.GroupBy(a => a.Status.ToString())
                    .Select(g => new AppointmentStatusCount
                    {
                        Status = g.Key,
                        Count = g.Count()
                    }).ToList(),
                AppointmentsByConsultant = new List<AppointmentByConsultant>(),
                AppointmentsByTimeSlot = new List<AppointmentByTimeSlot>(),
                MonthlyAppointments = new List<AppointmentMonthlyCount>(),
                AppointmentsByType = new List<AppointmentTypeCount>()
            };
        }

        private async Task<List<TopConsultant>> GenerateTopConsultantsAsync(DateTime fromDate, DateTime toDate)
        {
            var appointmentRepo = _unitOfWork.GetRepository<Appointment>();
            var appointments = await appointmentRepo.GetListAsync(
                predicate: a => a.AppointmentDate >= DateOnly.FromDateTime(fromDate) && 
                            a.AppointmentDate <= DateOnly.FromDateTime(toDate) &&
                            a.Status == AppointmentStatus.Completed,
                include: a => a.Include(ap => ap.Consultant)
            );

            return appointments.GroupBy(a => new { a.ConsultantId, a.Consultant.Name })
                .Select(g => new TopConsultant
                {
                    Name = g.Key.Name,
                    Specialization = "Tư vấn sức khỏe",
                    TotalAppointments = g.Count(),
                    AverageRating = 4.5, // Mock data
                    TotalReviews = g.Count(),
                    EstimatedRevenue = g.Count() * 500000 // Mock calculation
                })
                .OrderByDescending(tc => tc.TotalAppointments)
                .Take(10)
                .ToList();
        }

        private async Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            // Get all payment transactions
            var paymentTransactions = await _unitOfWork.GetRepository<PaymentTransaction>()
                .GetListAsync(
                    predicate: pt => pt.CreatedAt >= fromDate && pt.CreatedAt <= toDate,
                    include: pt => pt.Include(p => p.StiTesting).ThenInclude(st => st.Customer)
                );

            // Get STI tests data for payment analysis
            var stiTests = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    predicate: st => st.CreatedAt >= fromDate && st.CreatedAt <= toDate,
                    include: st => st.Include(s => s.Customer)
                );

            // Get appointments for revenue calculation
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .GetListAsync(
                    predicate: a => a.CreatedAt >= fromDate && a.CreatedAt <= toDate,
                    include: a => a.Include(ap => ap.Customer)
                );

            var totalPayments = paymentTransactions.Count();
            var successfulPayments = paymentTransactions.Count(pt => pt.Status == PaymentStatus.Success);
            var failedPayments = paymentTransactions.Count(pt => pt.Status == PaymentStatus.Failed);

            // Calculate overview
            var overview = new PaymentOverview
            {
                TotalPayments = totalPayments,
                SuccessfulPayments = successfulPayments,
                FailedPayments = failedPayments,
                SuccessRate = totalPayments > 0 ? (double)successfulPayments / totalPayments * 100 : 0,
                TotalRevenue = paymentTransactions.Where(pt => pt.Status == PaymentStatus.Success).Sum(pt => pt.Amount),
                STIRevenue = paymentTransactions.Where(pt => pt.Status == PaymentStatus.Success).Sum(pt => pt.Amount),
                AppointmentRevenue = appointments.Sum(a => 100000), // Estimated appointment revenue
                MenstrualRevenue = 0, // No separate menstrual payments tracked
                AverageTransactionValue = successfulPayments > 0 ? 
                    paymentTransactions.Where(pt => pt.Status == PaymentStatus.Success).Average(pt => pt.Amount) : 0
            };

            // Payment by service type
            var paymentsByService = new List<PaymentByService>
            {
                new PaymentByService
                {
                    ServiceType = "STI Testing",
                    TotalPayments = totalPayments,
                    SuccessfulPayments = successfulPayments,
                    TotalRevenue = overview.STIRevenue,
                    SuccessRate = overview.SuccessRate,
                    AverageAmount = successfulPayments > 0 ? overview.STIRevenue / successfulPayments : 0
                },
                new PaymentByService
                {
                    ServiceType = "Appointments",
                    TotalPayments = appointments.Count(),
                    SuccessfulPayments = appointments.Count(), // Assume all appointments are paid
                    TotalRevenue = overview.AppointmentRevenue,
                    SuccessRate = 100,
                    AverageAmount = appointments.Count() > 0 ? overview.AppointmentRevenue / appointments.Count() : 0
                }
            };

            // Payment by method
            var paymentsByMethod = paymentTransactions.GroupBy(pt => pt.PaymentMethod)
                .Select(g => new PaymentByMethod
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Where(pt => pt.Status == PaymentStatus.Success).Sum(pt => pt.Amount),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(pt => pt.Status == PaymentStatus.Success) / g.Count() * 100 : 0,
                    Percentage = totalPayments > 0 ? (double)g.Count() / totalPayments * 100 : 0
                }).ToList();

            // Success rates by period
            var successRates = paymentTransactions.GroupBy(pt => new { pt.CreatedAt.Year, pt.CreatedAt.Month })
                .Select(g => new PaymentSuccessRate
                {
                    ServiceCategory = "All Services",
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalTransactions = g.Count(),
                    SuccessfulTransactions = g.Count(pt => pt.Status == PaymentStatus.Success),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(pt => pt.Status == PaymentStatus.Success) / g.Count() * 100 : 0
                }).ToList();

            // Monthly payment metrics
            var monthlyPayments = paymentTransactions.GroupBy(pt => new { pt.CreatedAt.Year, pt.CreatedAt.Month })
                .Select(g => new MonthlyPaymentMetric
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalPayments = g.Count(),
                    SuccessfulPayments = g.Count(pt => pt.Status == PaymentStatus.Success),
                    Revenue = g.Where(pt => pt.Status == PaymentStatus.Success).Sum(pt => pt.Amount),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(pt => pt.Status == PaymentStatus.Success) / g.Count() * 100 : 0,
                    STIPayments = g.Count(), // All payments are STI payments currently
                    MenstrualPayments = 0,
                    AppointmentPayments = 0
                }).ToList();

            // Failure reasons
            var failureReasons = paymentTransactions.Where(pt => pt.Status == PaymentStatus.Failed)
                .GroupBy(pt => pt.ResponseCode ?? "Unknown")
                .Select(g => new PaymentFailureReason
                {
                    Reason = GetFailureReasonFromCode(g.Key),
                    Count = g.Count(),
                    Percentage = failedPayments > 0 ? (double)g.Count() / failedPayments * 100 : 0,
                    LostRevenue = g.Sum(pt => pt.Amount)
                }).ToList();

            // Revenue analytics
            var revenueAnalytics = new RevenueAnalytics
            {
                TotalRevenue = overview.TotalRevenue,
                STITestingRevenue = overview.STIRevenue,
                AppointmentRevenue = overview.AppointmentRevenue,
                MenstrualCycleRevenue = overview.MenstrualRevenue,
                AverageRevenuePerUser = stiTests.Count() > 0 ? overview.TotalRevenue / stiTests.GroupBy(st => st.CustomerId).Count() : 0,
                TopService = new TopRevenueService
                {
                    ServiceName = "STI Testing",
                    Revenue = overview.STIRevenue,
                    TransactionCount = successfulPayments,
                    Percentage = 100
                },
                STIPackageRevenue = stiTests.Where(st => st.IsPaid).GroupBy(st => st.TestPackage.ToString())
                    .Select(g => new RevenueByPackage
                    {
                        PackageName = g.Key,
                        Revenue = g.Sum(st => st.TotalPrice),
                        SalesCount = g.Count(),
                        AveragePrice = g.Average(st => st.TotalPrice),
                        PopularityPercentage = stiTests.Count() > 0 ? (double)g.Count() / stiTests.Count() * 100 : 0
                    }).ToList()
            };

            return new PaymentStatistics
            {
                Overview = overview,
                PaymentsByService = paymentsByService,
                PaymentsByMethod = paymentsByMethod,
                SuccessRates = successRates,
                MonthlyPayments = monthlyPayments,
                FailureReasons = failureReasons,
                RevenueAnalytics = revenueAnalytics
            };
        }

        private string GetFailureReasonFromCode(string code)
        {
            return code switch
            {
                "24" => "Giao dịch bị hủy bởi người dùng",
                "51" => "Số dư tài khoản không đủ",
                "65" => "Vượt quá hạn mức giao dịch",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Số tiền giao dịch vượt quá hạn mức ngày",
                "99" => "Lỗi không xác định",
                _ => $"Mã lỗi: {code}"
            };
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font font)
        {
            table.AddCell(new PdfPCell(new Phrase(label, font)));
            table.AddCell(new PdfPCell(new Phrase(value, font)));
        }

        private string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Hệ thống";
        }
    }
} 