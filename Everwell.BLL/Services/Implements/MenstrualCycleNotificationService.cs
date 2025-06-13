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

        public MenstrualCycleNotificationService(
            IUnitOfWork<EverwellDbContext> unitOfWork,
            ILogger<MenstrualCycleNotificationService> logger,
            AutoMapper.IMapper mapper,
            IEmailService emailService)
            : base(unitOfWork, logger, mapper)
        {
            _emailService = emailService;
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
                {
                    _logger.LogWarning("Tracking not found or notifications disabled for trackingId: {TrackingId}", trackingId);
                    return;
                }

                _logger.LogInformation("Scheduling notifications for tracking {TrackingId}, Customer {CustomerId}", trackingId, tracking.CustomerId);

                // Calculate next cycle prediction with fallback for new users
                DateTime nextPeriodStart;
                DateTime ovulationDate;
                
                try
                {
                    // Try to use existing cycle history for prediction
                    var prediction = await CalculateNextCyclePrediction(tracking.CustomerId);
                    nextPeriodStart = prediction;
                    ovulationDate = prediction.AddDays(-14); // Standard ovulation calculation
                    _logger.LogInformation("Using cycle history for prediction. Next period: {NextPeriod}", nextPeriodStart);
                }
                catch (Exception ex)
                {
                    // For new users or when no history exists, use current cycle + 28 days default
                    var cycleEndDate = tracking.CycleEndDate ?? tracking.CycleStartDate.AddDays(5); // Default 5-day period
                    nextPeriodStart = cycleEndDate.AddDays(28); // Standard 28-day cycle
                    ovulationDate = nextPeriodStart.AddDays(-14); // Standard ovulation timing
                    
                    _logger.LogInformation("No cycle history found for customer {CustomerId}, using default 28-day cycle prediction. Next period: {NextPeriod}", tracking.CustomerId, nextPeriodStart);
                }

                // Schedule period reminder
                if (tracking.NotifyBeforeDays.HasValue && tracking.NotifyBeforeDays.Value > 0)
                {
                    var reminderDate = nextPeriodStart.AddDays(-tracking.NotifyBeforeDays.Value);
                    var created = await CreateNotificationAsync(
                        trackingId,
                        MenstrualCyclePhase.Menstrual,
                        reminderDate,
                        $"Your period is expected to start in {tracking.NotifyBeforeDays} days on {nextPeriodStart:MMM dd, yyyy}");
                    _logger.LogInformation("Period reminder created: {Created} for date {ReminderDate}", created, reminderDate);
                }

                // Schedule ovulation reminder (2 days before ovulation)
                var ovulationReminderDate = ovulationDate.AddDays(-2);
                var ovulationCreated = await CreateNotificationAsync(
                    trackingId,
                    MenstrualCyclePhase.Ovulation,
                    ovulationReminderDate,
                    $"Your ovulation is expected on {ovulationDate:MMM dd, yyyy}. Your fertile window starts soon!");
                _logger.LogInformation("Ovulation reminder created: {Created} for date {ReminderDate}", ovulationCreated, ovulationReminderDate);

                // Schedule fertility window reminder (1 day before fertile window)
                var fertilityStart = ovulationDate.AddDays(-5);
                var fertilityReminderDate = fertilityStart.AddDays(-1);
                var fertilityCreated = await CreateNotificationAsync(
                    trackingId,
                    MenstrualCyclePhase.Follicular,
                    fertilityReminderDate,
                    $"Your fertile window starts tomorrow ({fertilityStart:MMM dd, yyyy})");
                _logger.LogInformation("Fertility reminder created: {Created} for date {ReminderDate}", fertilityCreated, fertilityReminderDate);

                // Don't save changes here - let the parent transaction handle it
                // await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully scheduled notifications for tracking {TrackingId}", trackingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notifications for tracking {TrackingId}", trackingId);
                throw;
            }
        }

        private async Task<DateTime> CalculateNextCyclePrediction(Guid customerId)
        {
            // Get last 6 cycles for prediction
            var startDate = DateTime.UtcNow.AddMonths(-6);
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    predicate: m => m.CustomerId == customerId && m.CycleStartDate >= startDate,
                    orderBy: q => q.OrderByDescending(x => x.CycleStartDate));

            if (!trackings.Any())
                return DateTime.UtcNow.AddDays(28); // Default 28-day cycle

            var cycleLengths = trackings
                .Where(h => h.CycleEndDate.HasValue)
                .Select(h => (h.CycleEndDate!.Value - h.CycleStartDate).TotalDays)
                .ToList();

            var averageCycleLength = cycleLengths.Any() ? cycleLengths.Average() : 28;
            var lastCycle = trackings.First();
            
            return lastCycle.CycleEndDate?.AddDays(averageCycleLength) ?? DateTime.UtcNow.AddDays(28);
        }

        private async Task<(DateTime FertileWindowStart, DateTime FertileWindowEnd, DateTime OvulationDate)> CalculateFertilityWindow(Guid customerId)
        {
            var nextPeriod = await CalculateNextCyclePrediction(customerId);
            var ovulationDate = nextPeriod.AddDays(-14);
            
            return (
                FertileWindowStart: ovulationDate.AddDays(-5),
                FertileWindowEnd: ovulationDate.AddDays(1),
                OvulationDate: ovulationDate
            );
        }

        public async Task<bool> CreateNotificationAsync(Guid trackingId, MenstrualCyclePhase phase, DateTime scheduledDate, string message)
        {
            try
            {
                // Validate input parameters
                if (trackingId == Guid.Empty)
                {
                    _logger.LogError("TrackingId is empty");
                    return false;
                }

                if (string.IsNullOrEmpty(message))
                {
                    _logger.LogError("Message is null or empty");
                    return false;
                }

                // Check if tracking exists
                var trackingExists = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: t => t.TrackingId == trackingId);
                
                if (trackingExists == null)
                {
                    _logger.LogError("Tracking with ID {TrackingId} does not exist", trackingId);
                    return false;
                }

                var notification = new MenstrualCycleNotification
                {
                    NotificationId = Guid.NewGuid(),
                    TrackingId = trackingId,
                    Phase = phase,
                    SentAt = scheduledDate,
                    Message = message,
                    IsSent = false
                };

                _logger.LogInformation("Creating notification: TrackingId={TrackingId}, Phase={Phase}, ScheduledDate={ScheduledDate}, Message={Message}", 
                    trackingId, phase, scheduledDate, message);

                // Check the context state before adding
                var contextState = _unitOfWork.Context.ChangeTracker.Entries().Count();
                _logger.LogInformation("Context has {Count} tracked entities before adding notification", contextState);

                await _unitOfWork.GetRepository<MenstrualCycleNotification>().InsertAsync(notification);
                
                // Check the context state after adding
                var contextStateAfter = _unitOfWork.Context.ChangeTracker.Entries().Count();
                _logger.LogInformation("Context has {Count} tracked entities after adding notification", contextStateAfter);

                // Check if the entity is actually tracked
                var entry = _unitOfWork.Context.Entry(notification);
                _logger.LogInformation("Notification entity state: {State}", entry.State);
                
                _logger.LogInformation("Notification created successfully with ID: {NotificationId}", notification.NotificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for trackingId: {TrackingId}, phase: {Phase}", trackingId, phase);
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

        public async Task<bool> TestCreateNotificationDirectlyAsync(Guid trackingId)
        {
            try
            {
                _logger.LogInformation("=== TESTING DIRECT NOTIFICATION CREATION ===");
                
                // Check if tracking exists
                var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: t => t.TrackingId == trackingId);
                
                if (tracking == null)
                {
                    _logger.LogError("Tracking not found for ID: {TrackingId}", trackingId);
                    return false;
                }
                
                _logger.LogInformation("Found tracking: {TrackingId}, Customer: {CustomerId}, NotificationEnabled: {NotificationEnabled}", 
                    tracking.TrackingId, tracking.CustomerId, tracking.NotificationEnabled);

                // Create a simple test notification
                var testNotification = new MenstrualCycleNotification
                {
                    NotificationId = Guid.NewGuid(),
                    TrackingId = trackingId,
                    Phase = MenstrualCyclePhase.Menstrual,
                    SentAt = DateTime.UtcNow.AddDays(1),
                    Message = "TEST NOTIFICATION - Debug test",
                    IsSent = false
                };

                _logger.LogInformation("Creating test notification with ID: {NotificationId}", testNotification.NotificationId);

                // Add to context
                await _unitOfWork.GetRepository<MenstrualCycleNotification>().InsertAsync(testNotification);
                
                // Check entity state
                var entry = _unitOfWork.Context.Entry(testNotification);
                _logger.LogInformation("Test notification entity state: {State}", entry.State);

                // Save changes
                var saveResult = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("SaveChanges result: {SaveResult} entities saved", saveResult);

                // Verify if it was saved
                var savedNotification = await _unitOfWork.GetRepository<MenstrualCycleNotification>()
                    .FirstOrDefaultAsync(predicate: n => n.NotificationId == testNotification.NotificationId);

                if (savedNotification != null)
                {
                    _logger.LogInformation("✅ Test notification was successfully saved to database!");
                    return true;
                }
                else
                {
                    _logger.LogError("❌ Test notification was NOT saved to database!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test notification creation");
                return false;
            }
        }
    }
}
