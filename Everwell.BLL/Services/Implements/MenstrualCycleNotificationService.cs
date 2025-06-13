using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements
{
    public class MenstrualCycleNotificationService : BaseService<MenstrualCycleNotificationService>, IMenstrualCycleNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IMenstrualCycleTrackingService _cycleTrackingService;

        public MenstrualCycleNotificationService(
            IUnitOfWork<EverwellDbContext> unitOfWork,
            ILogger<MenstrualCycleNotificationService> logger,
            AutoMapper.IMapper mapper,
            IEmailService emailService,
            IMenstrualCycleTrackingService cycleTrackingService)
            : base(unitOfWork, logger, mapper)
        {
            _emailService = emailService;
            _cycleTrackingService = cycleTrackingService;
        }

        public async Task ProcessPendingNotificationsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                
                // Get all notifications scheduled for today that haven't been sent
                var pendingNotifications = await _unitOfWork.GetRepository<MenstrualCycleNotification>()
                    .GetListAsync(
                        predicate: n => n.SentAt.Date == today && !n.IsSent,
                        include: q => q.Include(n => n.Tracking)
                                      .ThenInclude(t => t.Customer));

                foreach (var notification in pendingNotifications)
                {
                    await SendNotificationEmail(notification);
                    
                    // Mark as sent
                    notification.IsSent = true;
                    _unitOfWork.GetRepository<MenstrualCycleNotification>().UpdateAsync(notification);
                }

                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Processed {pendingNotifications.Count} pending notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending notifications");
                throw;
            }
        }

        public async Task ScheduleNotificationsForTrackingAsync(Guid trackingId)
        {
            try
            {
                var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(
                        predicate: t => t.TrackingId == trackingId,
                        include: q => q.Include(t => t.Customer));

                if (tracking == null || !tracking.NotificationEnabled)
                    return;

                // Get cycle prediction
                var prediction = await _cycleTrackingService.PredictNextCycleAsync(tracking.CustomerId);
                var fertilityWindow = await _cycleTrackingService.GetFertilityWindowAsync(tracking.CustomerId);

                // Schedule period reminder
                if (tracking.NotifyBeforeDays.HasValue)
                {
                    var reminderDate = prediction.PredictedNextPeriodStart.AddDays(-tracking.NotifyBeforeDays.Value);
                    await CreateNotificationAsync(
                        trackingId,
                        MenstrualCyclePhase.Menstrual,
                        reminderDate,
                        $"Your period is expected to start in {tracking.NotifyBeforeDays} days on {prediction.PredictedNextPeriodStart:MMM dd, yyyy}");
                }

                // Schedule ovulation reminder
                var ovulationReminderDate = fertilityWindow.OvulationDate.AddDays(-2);
                await CreateNotificationAsync(
                    trackingId,
                    MenstrualCyclePhase.Ovulation,
                    ovulationReminderDate,
                    $"Your ovulation is expected on {fertilityWindow.OvulationDate:MMM dd, yyyy}. Your fertile window starts soon!");

                // Schedule fertility window reminder
                var fertilityReminderDate = fertilityWindow.FertileWindowStart.AddDays(-1);
                await CreateNotificationAsync(
                    trackingId,
                    MenstrualCyclePhase.Follicular,
                    fertilityReminderDate,
                    $"Your fertile window starts tomorrow ({fertilityWindow.FertileWindowStart:MMM dd, yyyy})");

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notifications for tracking {TrackingId}", trackingId);
                throw;
            }
        }

        public async Task<bool> CreateNotificationAsync(Guid trackingId, MenstrualCyclePhase phase, DateTime scheduledDate, string message)
        {
            try
            {
                var notification = new MenstrualCycleNotification
                {
                    NotificationId = Guid.NewGuid(),
                    TrackingId = trackingId,
                    Phase = phase,
                    SentAt = scheduledDate,
                    Message = message,
                    IsSent = false
                };

                await _unitOfWork.GetRepository<MenstrualCycleNotification>().InsertAsync(notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return false;
            }
        }

        public async Task SendPeriodReminderAsync(Guid customerId, DateTime predictedDate)
        {
            var user = await _unitOfWork.GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.Id == customerId);

            if (user != null)
            {
                var subject = "Period Reminder - Everwell Healthcare";
                var body = GeneratePeriodReminderEmail(user.Name, predictedDate);
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        public async Task SendOvulationReminderAsync(Guid customerId, DateTime ovulationDate)
        {
            var user = await _unitOfWork.GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.Id == customerId);

            if (user != null)
            {
                var subject = "Ovulation Reminder - Everwell Healthcare";
                var body = GenerateOvulationReminderEmail(user.Name, ovulationDate);
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        public async Task SendFertilityWindowReminderAsync(Guid customerId, DateTime windowStart, DateTime windowEnd)
        {
            var user = await _unitOfWork.GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.Id == customerId);

            if (user != null)
            {
                var subject = "Fertility Window Alert - Everwell Healthcare";
                var body = GenerateFertilityWindowEmail(user.Name, windowStart, windowEnd);
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        private async Task SendNotificationEmail(MenstrualCycleNotification notification)
        {
            var user = notification.Tracking.Customer;
            var subject = GetEmailSubject(notification.Phase);
            var body = GenerateEmailBody(user.Name, notification.Message, notification.Phase);

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        private string GetEmailSubject(MenstrualCyclePhase phase)
        {
            return phase switch
            {
                MenstrualCyclePhase.Menstrual => "Period Reminder - Everwell Healthcare",
                MenstrualCyclePhase.Ovulation => "Ovulation Alert - Everwell Healthcare",
                MenstrualCyclePhase.Follicular => "Fertility Window Alert - Everwell Healthcare",
                MenstrualCyclePhase.Luteal => "Cycle Update - Everwell Healthcare",
                _ => "Menstrual Cycle Reminder - Everwell Healthcare"
            };
        }

        private string GenerateEmailBody(string userName, string message, MenstrualCyclePhase phase)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌸 Everwell Healthcare</h1>
            <h2>Menstrual Cycle Reminder</h2>
        </div>
        <div class='content'>
            <h3>Hi {userName},</h3>
            <p>{message}</p>
            
            <p>Remember to:</p>
            <ul>
                <li>Track your symptoms in the app</li>
                <li>Stay hydrated and maintain a healthy diet</li>
                <li>Get adequate rest and exercise</li>
                <li>Take care of your mental health</li>
            </ul>
            
            <a href='#' class='button'>Open Everwell App</a>
            
            <p>If you have any concerns about your cycle, don't hesitate to consult with our healthcare professionals.</p>
            
            <p>Best regards,<br>The Everwell Healthcare Team</p>
        </div>
        <div class='footer'>
            <p>You're receiving this email because you have notifications enabled for menstrual cycle tracking.</p>
            <p>To update your preferences, please log into your account.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePeriodReminderEmail(string userName, DateTime predictedDate)
        {
            return GenerateEmailBody(
                userName,
                $"Your period is expected to start on {predictedDate:MMMM dd, yyyy}. Make sure you're prepared!",
                MenstrualCyclePhase.Menstrual);
        }

        private string GenerateOvulationReminderEmail(string userName, DateTime ovulationDate)
        {
            return GenerateEmailBody(
                userName,
                $"Your ovulation is expected on {ovulationDate:MMMM dd, yyyy}. This is your most fertile time!",
                MenstrualCyclePhase.Ovulation);
        }

        private string GenerateFertilityWindowEmail(string userName, DateTime windowStart, DateTime windowEnd)
        {
            return GenerateEmailBody(
                userName,
                $"Your fertile window is from {windowStart:MMM dd} to {windowEnd:MMM dd, yyyy}. Track your symptoms carefully during this time!",
                MenstrualCyclePhase.Follicular);
        }
    }
}
