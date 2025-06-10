using Everwell.BLL.Models.Reports;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services
{
    public interface IReportService
    {
        Task<ReportResult> GenerateReportAsync(ReportRequest request, string generatedBy);
        Task<List<T>> GetReportDataAsync<T>(ReportType reportType, ReportRequest request);
    }

    public class ReportService : IReportService
    {
        private readonly EverwellDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(EverwellDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReportResult> GenerateReportAsync(ReportRequest request, string generatedBy)
        {
            var reportData = await GetReportDataByTypeAsync(request);

            var metadata = new ReportMetadata
            {
                Title = GetReportTitle(request.ReportType),
                Description = GetReportDescription(request.ReportType),
                TotalRecords = GetRecordCount(reportData),
                GeneratedBy = generatedBy,
                GeneratedAt = DateTime.Now
            };

            return request.Format switch
            {
                ExportFormat.Excel => await GenerateExcelReportAsync(reportData, metadata, request.ReportType),
                ExportFormat.PDF => await GenerateSimplePdfReportAsync(reportData, metadata, request.ReportType),
                _ => throw new NotSupportedException($"Export format {request.Format} is not supported")
            };
        }

        public async Task<List<T>> GetReportDataAsync<T>(ReportType reportType, ReportRequest request)
        {
            var data = await GetReportDataByTypeAsync(request);
            return (List<T>)data;
        }

        private async Task<object> GetReportDataByTypeAsync(ReportRequest request)
        {
            return request.ReportType switch
            {
                ReportType.UserStatistics => await GetUserStatisticsDataAsync(),
                ReportType.AppointmentStatistics => await GetAppointmentStatisticsDataAsync(),
                ReportType.ServiceUtilization => await GetServiceUtilizationDataAsync(),
                ReportType.FeedbackAnalysis => await GetFeedbackAnalysisDataAsync(),
                ReportType.STITestingReports => await GetSTITestingDataAsync(),
                ReportType.MenstrualTrackingReports => await GetMenstrualTrackingDataAsync(),
                ReportType.PostEngagement => await GetPostEngagementDataAsync(),
                _ => throw new NotSupportedException($"Report type {request.ReportType} is not supported")
            };
        }

        // Export ALL users - no filters
        private async Task<List<UserStatisticsReportData>> GetUserStatisticsDataAsync()
        {
            return await _context.Users.Select(u => new UserStatisticsReportData
            {
                FullName = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = DateTime.Now.AddDays(-30), // Placeholder since User doesn't have CreatedAt
                IsActive = u.IsActive,
                TotalAppointments = _context.Appointments.Count(a => a.CustomerId == u.Id),
                TotalPosts = _context.Posts.Count(p => p.StaffId == u.Id),
                TotalQuestions = _context.Questions.Count(q => q.CustomerId == u.Id)
            }).ToListAsync();
        }

        // Export ALL appointments - no filters
        private async Task<List<AppointmentStatisticsReportData>> GetAppointmentStatisticsDataAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                .Include(a => a.Service)
                .Select(a => new
                {
                    CustomerName = a.Customer.Name,
                    ConsultantName = a.Consultant.Name,
                    ServiceName = a.Service.Name,
                    AppointmentDate = a.AppointmentDate,
                    Slot = a.Slot,
                    Status = a.Status,
                    Notes = a.Notes ?? ""
                }).ToListAsync();

            return appointments.Select(a => new AppointmentStatisticsReportData
            {
                CustomerName = a.CustomerName,
                ConsultantName = a.ConsultantName,
                ServiceName = a.ServiceName,
                AppointmentDate = a.AppointmentDate.ToDateTime(TimeOnly.MinValue),
                StartTime = GetTimeSpanFromSlot(a.Slot, true),
                EndTime = GetTimeSpanFromSlot(a.Slot, false),
                Status = a.Status.ToString(),
                Notes = a.Notes
            }).ToList();
        }

        // Export ALL services - no filters
        private async Task<List<ServiceUtilizationReportData>> GetServiceUtilizationDataAsync()
        {
            var services = await _context.Services.ToListAsync();
            var result = new List<ServiceUtilizationReportData>();
            
            foreach (var service in services)
            {
                var appointmentCount = await _context.Appointments
                    .Where(a => a.ServiceId == service.Id)
                    .CountAsync();
                
                var totalRevenue = appointmentCount * service.Price;
                
                result.Add(new ServiceUtilizationReportData
                {
                    ServiceName = service.Name,
                    Description = service.Description ?? "",
                    Price = service.Price,
                    TotalAppointments = appointmentCount,
                    TotalRevenue = totalRevenue,
                    IsActive = service.IsActive
                });
            }
            
            return result;
        }

        // Export ALL feedback - no filters
        private async Task<List<FeedbackAnalysisReportData>> GetFeedbackAnalysisDataAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Consultant)
                .Include(f => f.Service)
                .Select(f => new FeedbackAnalysisReportData
                {
                    CustomerName = f.Customer.Name,
                    ConsultantName = f.Consultant.Name,
                    ServiceName = f.Service.Name,
                    Rating = f.Rating,
                    Comments = f.Comment,
                    CreatedAt = f.CreatedAt.ToDateTime(TimeOnly.MinValue)
                }).ToListAsync();
        }

        // Export ALL STI testing records - no filters
        private async Task<List<STITestingReportData>> GetSTITestingDataAsync()
        {
            return await _context.STITests
                .Include(s => s.Customer)
                .Select(s => new STITestingReportData
                {
                    CustomerName = s.Customer.Name,
                    TestType = s.TestType.ToString(),
                    TestDate = s.CollectedDate.HasValue ? s.CollectedDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    Result = "Pending",
                    Status = s.Status.ToString(),
                    Notes = s.Method.ToString()
                }).ToListAsync();
        }

        // Export ALL menstrual tracking records - no filters
        private async Task<List<MenstrualTrackingReportData>> GetMenstrualTrackingDataAsync()
        {
            return await _context.MenstrualCycleTrackings
                .Include(m => m.Customer)
                .Select(m => new MenstrualTrackingReportData
                {
                    CustomerName = m.Customer.Name,
                    StartDate = m.CycleStartDate,
                    EndDate = m.CycleEndDate,
                    CycleLength = m.CycleEndDate.HasValue ? 
                        (int)(m.CycleEndDate.Value - m.CycleStartDate).TotalDays : 0,
                    FlowIntensity = "Normal",
                    Symptoms = m.Symptoms ?? "",
                    Notes = m.Notes ?? ""
                }).ToListAsync();
        }

        // Export ALL posts - no filters
        private async Task<List<PostEngagementReportData>> GetPostEngagementDataAsync()
        {
            return await _context.Posts
                .Include(p => p.Staff)
                .Select(p => new PostEngagementReportData
                {
                    Title = p.Title,
                    AuthorName = p.Staff.Name,
                    CreatedAt = p.CreatedAt.ToDateTime(TimeOnly.MinValue),
                    Category = p.Category.ToString(),
                    ViewCount = 0, // Post doesn't have Views property
                    LikeCount = 0, // Post doesn't have LikeCount property
                    CommentCount = 0, // Post doesn't have CommentCount property
                    IsActive = p.Status == PostStatus.Approved // Use Status instead of IsActive
                }).ToListAsync();
        }

        private static TimeSpan GetTimeSpanFromSlot(ShiftSlot slot, bool isStartTime)
        {
            return slot switch
            {
                ShiftSlot.Morning1 => isStartTime ? new TimeSpan(8, 0, 0) : new TimeSpan(10, 0, 0),
                ShiftSlot.Morning2 => isStartTime ? new TimeSpan(10, 0, 0) : new TimeSpan(12, 0, 0),
                ShiftSlot.Afternoon1 => isStartTime ? new TimeSpan(13, 0, 0) : new TimeSpan(15, 0, 0),
                ShiftSlot.Afternoon2 => isStartTime ? new TimeSpan(15, 0, 0) : new TimeSpan(17, 0, 0),
                _ => TimeSpan.Zero
            };
        }

        private async Task<ReportResult> GenerateExcelReportAsync(object data, ReportMetadata metadata, ReportType reportType)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"{metadata.Title}");

            worksheet.Cell(1, 1).Value = metadata.Title;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            worksheet.Cell(2, 1).Value = $"Generated: {metadata.GeneratedAt:yyyy-MM-dd HH:mm}";
            worksheet.Cell(3, 1).Value = $"Total Records: {metadata.TotalRecords}";
            worksheet.Cell(4, 1).Value = $"Generated By: {metadata.GeneratedBy}";

            await PopulateExcelWorksheetAsync(worksheet, data, reportType);

            worksheet.ColumnsUsed().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReportResult
            {
                FileName = $"{metadata.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                FileContent = stream.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                GeneratedAt = DateTime.Now,
                Metadata = metadata
            };
        }

        private async Task PopulateExcelWorksheetAsync(IXLWorksheet worksheet, object data, ReportType reportType)
        {
            var startRow = 6;

            switch (reportType)
            {
                case ReportType.UserStatistics:
                    var userStats = (List<UserStatisticsReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Full Name";
                    worksheet.Cell(startRow, 2).Value = "Email";
                    worksheet.Cell(startRow, 3).Value = "Role";
                    worksheet.Cell(startRow, 4).Value = "Created At";
                    worksheet.Cell(startRow, 5).Value = "Is Active";
                    worksheet.Cell(startRow, 6).Value = "Total Appointments";
                    worksheet.Cell(startRow, 7).Value = "Total Posts";
                    worksheet.Cell(startRow, 8).Value = "Total Questions";

                    for (int i = 0; i < userStats.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var user = userStats[i];
                        worksheet.Cell(row, 1).Value = user.FullName;
                        worksheet.Cell(row, 2).Value = user.Email;
                        worksheet.Cell(row, 3).Value = user.Role;
                        worksheet.Cell(row, 4).Value = user.CreatedAt;
                        worksheet.Cell(row, 5).Value = user.IsActive;
                        worksheet.Cell(row, 6).Value = user.TotalAppointments;
                        worksheet.Cell(row, 7).Value = user.TotalPosts;
                        worksheet.Cell(row, 8).Value = user.TotalQuestions;
                    }
                    break;

                case ReportType.AppointmentStatistics:
                    var appointments = (List<AppointmentStatisticsReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Customer Name";
                    worksheet.Cell(startRow, 2).Value = "Consultant Name";
                    worksheet.Cell(startRow, 3).Value = "Service Name";
                    worksheet.Cell(startRow, 4).Value = "Appointment Date";
                    worksheet.Cell(startRow, 5).Value = "Start Time";
                    worksheet.Cell(startRow, 6).Value = "End Time";
                    worksheet.Cell(startRow, 7).Value = "Status";
                    worksheet.Cell(startRow, 8).Value = "Notes";

                    for (int i = 0; i < appointments.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var appointment = appointments[i];
                        worksheet.Cell(row, 1).Value = appointment.CustomerName;
                        worksheet.Cell(row, 2).Value = appointment.ConsultantName;
                        worksheet.Cell(row, 3).Value = appointment.ServiceName;
                        worksheet.Cell(row, 4).Value = appointment.AppointmentDate;
                        worksheet.Cell(row, 5).Value = appointment.StartTime.ToString();
                        worksheet.Cell(row, 6).Value = appointment.EndTime.ToString();
                        worksheet.Cell(row, 7).Value = appointment.Status;
                        worksheet.Cell(row, 8).Value = appointment.Notes;
                    }
                    break;

                case ReportType.ServiceUtilization:
                    var services = (List<ServiceUtilizationReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Service Name";
                    worksheet.Cell(startRow, 2).Value = "Description";
                    worksheet.Cell(startRow, 3).Value = "Price";
                    worksheet.Cell(startRow, 4).Value = "Total Appointments";
                    worksheet.Cell(startRow, 5).Value = "Total Revenue";
                    worksheet.Cell(startRow, 6).Value = "Is Active";

                    for (int i = 0; i < services.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var service = services[i];
                        worksheet.Cell(row, 1).Value = service.ServiceName;
                        worksheet.Cell(row, 2).Value = service.Description;
                        worksheet.Cell(row, 3).Value = service.Price;
                        worksheet.Cell(row, 4).Value = service.TotalAppointments;
                        worksheet.Cell(row, 5).Value = service.TotalRevenue;
                        worksheet.Cell(row, 6).Value = service.IsActive;
                    }
                    break;

                case ReportType.FeedbackAnalysis:
                    var feedbacks = (List<FeedbackAnalysisReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Customer Name";
                    worksheet.Cell(startRow, 2).Value = "Consultant Name";
                    worksheet.Cell(startRow, 3).Value = "Service Name";
                    worksheet.Cell(startRow, 4).Value = "Rating";
                    worksheet.Cell(startRow, 5).Value = "Comments";
                    worksheet.Cell(startRow, 6).Value = "Created At";

                    for (int i = 0; i < feedbacks.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var feedback = feedbacks[i];
                        worksheet.Cell(row, 1).Value = feedback.CustomerName;
                        worksheet.Cell(row, 2).Value = feedback.ConsultantName;
                        worksheet.Cell(row, 3).Value = feedback.ServiceName;
                        worksheet.Cell(row, 4).Value = feedback.Rating;
                        worksheet.Cell(row, 5).Value = feedback.Comments;
                        worksheet.Cell(row, 6).Value = feedback.CreatedAt;
                    }
                    break;

                case ReportType.STITestingReports:
                    var stiTests = (List<STITestingReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Customer Name";
                    worksheet.Cell(startRow, 2).Value = "Test Type";
                    worksheet.Cell(startRow, 3).Value = "Test Date";
                    worksheet.Cell(startRow, 4).Value = "Result";
                    worksheet.Cell(startRow, 5).Value = "Status";
                    worksheet.Cell(startRow, 6).Value = "Notes";

                    for (int i = 0; i < stiTests.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var test = stiTests[i];
                        worksheet.Cell(row, 1).Value = test.CustomerName;
                        worksheet.Cell(row, 2).Value = test.TestType;
                        worksheet.Cell(row, 3).Value = test.TestDate;
                        worksheet.Cell(row, 4).Value = test.Result;
                        worksheet.Cell(row, 5).Value = test.Status;
                        worksheet.Cell(row, 6).Value = test.Notes;
                    }
                    break;

                case ReportType.MenstrualTrackingReports:
                    var trackings = (List<MenstrualTrackingReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Customer Name";
                    worksheet.Cell(startRow, 2).Value = "Start Date";
                    worksheet.Cell(startRow, 3).Value = "End Date";
                    worksheet.Cell(startRow, 4).Value = "Cycle Length";
                    worksheet.Cell(startRow, 5).Value = "Flow Intensity";
                    worksheet.Cell(startRow, 6).Value = "Symptoms";
                    worksheet.Cell(startRow, 7).Value = "Notes";

                    for (int i = 0; i < trackings.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var tracking = trackings[i];
                        worksheet.Cell(row, 1).Value = tracking.CustomerName;
                        worksheet.Cell(row, 2).Value = tracking.StartDate;
                        worksheet.Cell(row, 3).Value = tracking.EndDate?.ToString() ?? "";
                        worksheet.Cell(row, 4).Value = tracking.CycleLength;
                        worksheet.Cell(row, 5).Value = tracking.FlowIntensity;
                        worksheet.Cell(row, 6).Value = tracking.Symptoms;
                        worksheet.Cell(row, 7).Value = tracking.Notes;
                    }
                    break;

                case ReportType.PostEngagement:
                    var posts = (List<PostEngagementReportData>)data;
                    worksheet.Cell(startRow, 1).Value = "Title";
                    worksheet.Cell(startRow, 2).Value = "Author Name";
                    worksheet.Cell(startRow, 3).Value = "Created At";
                    worksheet.Cell(startRow, 4).Value = "Category";
                    worksheet.Cell(startRow, 5).Value = "Status";
                    worksheet.Cell(startRow, 6).Value = "Is Active";

                    for (int i = 0; i < posts.Count; i++)
                    {
                        var row = startRow + i + 1;
                        var post = posts[i];
                        worksheet.Cell(row, 1).Value = post.Title;
                        worksheet.Cell(row, 2).Value = post.AuthorName;
                        worksheet.Cell(row, 3).Value = post.CreatedAt;
                        worksheet.Cell(row, 4).Value = post.Category;
                        worksheet.Cell(row, 5).Value = post.IsActive ? "Approved" : "Not Approved";
                        worksheet.Cell(row, 6).Value = post.IsActive;
                    }
                    break;
            }

            var headerRange = worksheet.Range(startRow, 1, startRow, worksheet.LastColumnUsed().ColumnNumber());
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private async Task<ReportResult> GenerateSimplePdfReportAsync(object data, ReportMetadata metadata, ReportType reportType)
        {
            try
            {
                using var stream = new MemoryStream();
                using var writer = new PdfWriter(stream);
                using var pdf = new PdfDocument(writer);
                using var document = new iText.Layout.Document(pdf);

                // Add title
                document.Add(new iText.Layout.Element.Paragraph(metadata.Title)
                    .SetFontSize(20)
                    .SetBold());

                // Add metadata
                document.Add(new iText.Layout.Element.Paragraph($"Generated: {metadata.GeneratedAt:yyyy-MM-dd HH:mm}"));
                document.Add(new iText.Layout.Element.Paragraph($"Total Records: {metadata.TotalRecords}"));
                document.Add(new iText.Layout.Element.Paragraph($"Generated By: {metadata.GeneratedBy}"));
                document.Add(new iText.Layout.Element.Paragraph(" ")); // Spacer

                // Add data
                await AddSimpleDataToPdf(document, data, reportType);

                document.Close();

                return new ReportResult
                {
                    FileName = $"{metadata.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    FileContent = stream.ToArray(),
                    ContentType = "application/pdf",
                    GeneratedAt = DateTime.Now,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                throw;
            }
        }

        private async Task AddSimpleDataToPdf(iText.Layout.Document document, object data, ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.UserStatistics:
                    var userStats = (List<UserStatisticsReportData>)data;
                    foreach (var user in userStats.Take(100)) // Limit for PDF
                    {
                        document.Add(new iText.Layout.Element.Paragraph($"User: {user.FullName} | Email: {user.Email} | Role: {user.Role} | Active: {user.IsActive}"));
                    }
                    break;

                case ReportType.AppointmentStatistics:
                    var appointments = (List<AppointmentStatisticsReportData>)data;
                    foreach (var appointment in appointments.Take(100))
                    {
                        document.Add(new iText.Layout.Element.Paragraph($"Customer: {appointment.CustomerName} | Consultant: {appointment.ConsultantName} | Service: {appointment.ServiceName} | Date: {appointment.AppointmentDate:yyyy-MM-dd} | Status: {appointment.Status}"));
                    }
                    break;

                case ReportType.ServiceUtilization:
                    var services = (List<ServiceUtilizationReportData>)data;
                    foreach (var service in services.Take(100))
                    {
                        document.Add(new iText.Layout.Element.Paragraph($"Service: {service.ServiceName} | Price: ${service.Price} | Appointments: {service.TotalAppointments} | Revenue: ${service.TotalRevenue} | Active: {service.IsActive}"));
                    }
                    break;

                case ReportType.FeedbackAnalysis:
                    var feedbacks = (List<FeedbackAnalysisReportData>)data;
                    foreach (var feedback in feedbacks.Take(100))
                    {
                        document.Add(new iText.Layout.Element.Paragraph($"Customer: {feedback.CustomerName} | Consultant: {feedback.ConsultantName} | Rating: {feedback.Rating}/5 | Service: {feedback.ServiceName}"));
                    }
                    break;

                case ReportType.PostEngagement:
                    var posts = (List<PostEngagementReportData>)data;
                    foreach (var post in posts.Take(100))
                    {
                        document.Add(new iText.Layout.Element.Paragraph($"Post: {post.Title} | Author: {post.AuthorName} | Category: {post.Category} | Status: {(post.IsActive ? "Approved" : "Not Approved")}"));
                    }
                    break;

                default:
                    document.Add(new iText.Layout.Element.Paragraph("Report data not available for PDF format"));
                    break;
            }
        }

        private string GetReportTitle(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.UserStatistics => "User Statistics Report",
                ReportType.AppointmentStatistics => "Appointment Statistics Report",
                ReportType.ServiceUtilization => "Service Utilization Report",
                ReportType.FeedbackAnalysis => "Feedback Analysis Report",
                ReportType.STITestingReports => "STI Testing Reports",
                ReportType.MenstrualTrackingReports => "Menstrual Tracking Reports",
                ReportType.PostEngagement => "Post Engagement Report",
                _ => "Unknown Report"
            };
        }

        private string GetReportDescription(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.UserStatistics => "Complete export of all user data and statistics",
                ReportType.AppointmentStatistics => "Complete export of all appointment data",
                ReportType.ServiceUtilization => "Complete export of all service utilization data",
                ReportType.FeedbackAnalysis => "Complete export of all feedback data",
                ReportType.STITestingReports => "Complete export of all STI testing records",
                ReportType.MenstrualTrackingReports => "Complete export of all menstrual tracking data",
                ReportType.PostEngagement => "Complete export of all post engagement data",
                _ => "Complete data export"
            };
        }

        private int GetRecordCount(object data)
        {
            return data switch
            {
                List<UserStatisticsReportData> userStats => userStats.Count,
                List<AppointmentStatisticsReportData> appointments => appointments.Count,
                List<ServiceUtilizationReportData> services => services.Count,
                List<FeedbackAnalysisReportData> feedbacks => feedbacks.Count,
                List<STITestingReportData> stiTests => stiTests.Count,
                List<MenstrualTrackingReportData> trackings => trackings.Count,
                List<PostEngagementReportData> posts => posts.Count,
                _ => 0
            };
        }
    }
}