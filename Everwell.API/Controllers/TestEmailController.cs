using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IMenstrualCycleNotificationService _notificationService;
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IEmailService emailService,
            IMenstrualCycleNotificationService notificationService,
            IUnitOfWork<EverwellDbContext> unitOfWork,
            ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("check-notifications")]
        public async Task<IActionResult> CheckNotifications()
        {
            try
            {
                var notifications = await _unitOfWork.GetRepository<MenstrualCycleNotification>()
                    .GetListAsync();
                
                var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .GetListAsync();

                return Ok(new { 
                    totalNotifications = notifications.Count(),
                    totalTrackings = trackings.Count(),
                    notifications = notifications.Select(n => new {
                        n.NotificationId,
                        n.TrackingId,
                        n.Phase,
                        n.SentAt,
                        n.Message,
                        n.IsSent
                    }).ToList(),
                    trackings = trackings.Select(t => new {
                        t.TrackingId,
                        t.CustomerId,
                        t.NotificationEnabled,
                        t.NotifyBeforeDays,
                        t.CycleStartDate,
                        t.CycleEndDate
                    }).ToList(),
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check notifications");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("send-basic-email")]
        public async Task<IActionResult> SendBasicEmail([FromBody] BasicEmailRequest request)
        {
            try
            {
                await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Body);
                return Ok(new { message = "Email sent successfully!", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send basic email");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("send-period-reminder")]
        public async Task<IActionResult> SendPeriodReminder([FromBody] PeriodReminderRequest request)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                await _notificationService.SendPeriodReminderAsync(customerId, request.PredictedDate);
                return Ok(new { message = "Period reminder sent successfully!", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send period reminder");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("send-ovulation-reminder")]
        public async Task<IActionResult> SendOvulationReminder([FromBody] OvulationReminderRequest request)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                await _notificationService.SendOvulationReminderAsync(customerId, request.OvulationDate);
                return Ok(new { message = "Ovulation reminder sent successfully!", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ovulation reminder");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("send-fertility-window-reminder")]
        public async Task<IActionResult> SendFertilityWindowReminder([FromBody] FertilityWindowRequest request)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                await _notificationService.SendFertilityWindowReminderAsync(
                    customerId, 
                    request.WindowStart, 
                    request.WindowEnd);
                return Ok(new { message = "Fertility window reminder sent successfully!", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send fertility window reminder");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("process-pending-notifications")]
        public async Task<IActionResult> ProcessPendingNotifications()
        {
            try
            {
                await _notificationService.ProcessPendingNotificationsAsync();
                return Ok(new { message = "Pending notifications processed successfully!", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process pending notifications");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("smtp-test")]
        public async Task<IActionResult> TestSmtpConfiguration()
        {
            try
            {
                var testEmail = "test@example.com"; // Replace with your test email
                var subject = "SMTP Configuration Test - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var body = @"
                    <html>
                    <body>
                        <h2>SMTP Test Email</h2>
                        <p>If you receive this email, your SMTP configuration is working correctly!</p>
                        <p>Sent at: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(testEmail, subject, body);
                return Ok(new { 
                    message = "SMTP test email sent successfully!", 
                    sentTo = testEmail,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP test failed");
                return BadRequest(new { 
                    error = "SMTP test failed: " + ex.Message, 
                    timestamp = DateTime.UtcNow,
                    troubleshooting = new[]
                    {
                        "Check your email configuration in appsettings.json",
                        "Verify SMTP server and port settings",
                        "Ensure username and password are correct",
                        "Check if 'Less secure app access' is enabled for Gmail",
                        "For Gmail, use App Password instead of regular password"
                    }
                });
            }
        }
    }

    // Request models
    public class BasicEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class PeriodReminderRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public DateTime PredictedDate { get; set; }
    }

    public class OvulationReminderRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public DateTime OvulationDate { get; set; }
    }

    public class FertilityWindowRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
    }
}