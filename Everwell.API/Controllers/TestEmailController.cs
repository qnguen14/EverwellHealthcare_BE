using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.MenstrualCycle;
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

        [HttpPost("quick-email-test")]
        public async Task<IActionResult> QuickEmailTest()
        {
            try
            {
                // Test 1: Send a basic test email
                var testEmail = "manh310104@example.com"; // Replace with your email
                var subject = "🔴 Everwell Test - Period Reminder";
                var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #e74c3c; color: white; padding: 20px; text-align: center;'>
                    <h2>🔴 Period Reminder Test</h2>
                </div>
                <div style='padding: 30px; background-color: #f9f9f9;'>
                    <p>Hi there!</p>
                    <p>This is a <strong>test email</strong> from your Everwell automatic notification system.</p>
                    <p>Your period is expected to start soon. Make sure you're prepared!</p>
                    <p><strong>Test sent at:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    
                    <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd;'>
                        <p style='color: #888; font-size: 12px;'>
                            ✅ If you received this email, your automatic notification system is working!<br>
                            Best regards,<br>Everwell Health Team
                        </p>
                    </div>
                </div>
            </body>
            </html>";

                await _emailService.SendEmailAsync(testEmail, subject, body);

                // Test 2: Process any pending notifications
                await _notificationService.ProcessPendingNotificationsAsync();

                return Ok(new
                {
                    message = "✅ Quick email test completed successfully!",
                    emailSentTo = testEmail,
                    instructions = new[]
                    {
                "1. Check your email inbox for the test message",
                "2. If received, your automatic system is working",
                "3. Background service runs every 6 hours automatically",
                "4. Users will get reminders when cycles are created with notifications enabled"
            },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quick email test failed");
                return BadRequest(new
                {
                    error = ex.Message,
                    troubleshooting = new[]
                    {
                "Check email configuration in appsettings.json",
                "Verify Gmail app password is correct",
                "Ensure SMTP settings are valid"
            },
                    timestamp = DateTime.UtcNow
                });
            }
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