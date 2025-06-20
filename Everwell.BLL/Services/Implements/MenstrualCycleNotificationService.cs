using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
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
            IHttpContextAccessor httpContextAccessor,
            AutoMapper.IMapper mapper,
            IEmailService emailService)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
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
                // First try to find in context (for newly created entities)
                var tracking = _unitOfWork.Context.ChangeTracker.Entries<MenstrualCycleTracking>()
                    .FirstOrDefault(e => e.Entity.TrackingId == trackingId)?.Entity;

                // If not found in context, query database
                if (tracking == null)
                {
                    tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                        .FirstOrDefaultAsync(
                            predicate: t => t.TrackingId == trackingId,
                            include: q => q.Include(t => t.Customer));
                }
                else
                {
                    // Load customer if needed
                    if (tracking.Customer == null)
                    {
                        tracking.Customer = await _unitOfWork.GetRepository<User>()
                            .FirstOrDefaultAsync(predicate: u => u.Id == tracking.CustomerId);
                    }
                }

                if (tracking == null || !tracking.NotificationEnabled)
                {
                    _logger.LogWarning("Tracking not found or notifications disabled for trackingId: {TrackingId}", trackingId);
                    return;
                }

                await ScheduleNotificationsForTrackingAsync(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notifications for tracking {TrackingId}", trackingId);
                throw;
            }
        }

        public async Task ScheduleNotificationsForTrackingAsync(MenstrualCycleTracking tracking)
        {
            try
            {
                if (tracking == null || !tracking.NotificationEnabled)
                {
                    _logger.LogWarning("Tracking is null or notifications disabled for trackingId: {TrackingId}", tracking?.TrackingId);
                    return;
                }

                // Load customer if needed
                if (tracking.Customer == null)
                {
                    tracking.Customer = await _unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(predicate: u => u.Id == tracking.CustomerId);
                }

                _logger.LogInformation("Scheduling notifications for tracking {TrackingId}, Customer {CustomerId}", tracking.TrackingId, tracking.CustomerId);

                // Calculate next cycle prediction with correct logic
                var (nextPeriodStart, cycleLength, confidence) = await CalculateNextCyclePredictionWithConfidence(tracking.CustomerId);
                var ovulationDate = CalculateOvulationDate(nextPeriodStart, cycleLength);
                var (fertilityStart, fertilityEnd) = CalculateFertilityWindow(ovulationDate);

                _logger.LogInformation("Predictions - Next period: {NextPeriod}, Ovulation: {Ovulation}, Cycle length: {CycleLength}, Confidence: {Confidence}",
                    nextPeriodStart, ovulationDate, cycleLength, confidence);

                // Schedule period reminder
                if (tracking.NotifyBeforeDays.HasValue && tracking.NotifyBeforeDays.Value > 0)
                {
                    var reminderDate = nextPeriodStart.AddDays(-tracking.NotifyBeforeDays.Value);
                    var created = await CreateNotificationAsync(
                        tracking.TrackingId,
                        MenstrualCyclePhase.Menstrual,
                        reminderDate,
                        $"Your period is expected to start in {tracking.NotifyBeforeDays} days on {nextPeriodStart:MMM dd, yyyy}. Confidence: {confidence}%");
                    _logger.LogInformation("Period reminder created: {Created} for date {ReminderDate}", created, reminderDate);
                }

                // Schedule ovulation reminder (2 days before ovulation)
                var ovulationReminderDate = ovulationDate.AddDays(-2);
                var ovulationCreated = await CreateNotificationAsync(
                    tracking.TrackingId,
                    MenstrualCyclePhase.Ovulation,
                    ovulationReminderDate,
                    $"Your ovulation is expected on {ovulationDate:MMM dd, yyyy}. Your fertile window starts soon!");
                _logger.LogInformation("Ovulation reminder created: {Created} for date {ReminderDate}", ovulationCreated, ovulationReminderDate);

                // Schedule fertility window reminder (1 day before fertile window starts)
                var fertilityReminderDate = fertilityStart.AddDays(-1);
                var fertilityCreated = await CreateNotificationAsync(
                    tracking.TrackingId,
                    MenstrualCyclePhase.Follicular,
                    fertilityReminderDate,
                    $"Your fertile window starts tomorrow ({fertilityStart:MMM dd, yyyy}) and lasts until {fertilityEnd:MMM dd, yyyy}");
                _logger.LogInformation("Fertility reminder created: {Created} for date {ReminderDate}", fertilityCreated, fertilityReminderDate);

                _logger.LogInformation("Successfully scheduled notifications for tracking {TrackingId}", tracking.TrackingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notifications for tracking {TrackingId}", tracking?.TrackingId);
                throw;
            }
        }

        private async Task<(DateTime nextPeriodStart, double cycleLength, int confidence)> CalculateNextCyclePredictionWithConfidence(Guid customerId)
        {
            // Get last 6 cycles for prediction (more data = better accuracy)
            var startDate = DateTime.UtcNow.AddMonths(-6);
            var trackings = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                .GetListAsync(
                    predicate: m => m.CustomerId == customerId && m.CycleStartDate >= startDate,
                    orderBy: q => q.OrderByDescending(x => x.CycleStartDate));

            // Convert to list to enable indexing
            var trackingsList = trackings.ToList();

            if (trackingsList.Count < 2)
            {
                // Default for new users - use standard 28-day cycle
                return (DateTime.UtcNow.AddDays(28), 28, 30); // Low confidence for new users
            }

            // Calculate cycle lengths correctly (start of one period to start of next)
            var cycleLengths = new List<double>();
            for (int i = 0; i < trackingsList.Count - 1; i++)
            {
                var currentStart = trackingsList[i].CycleStartDate;
                var nextStart = trackingsList[i + 1].CycleStartDate;
                var cycleLength = (currentStart - nextStart).TotalDays;
                
                // Validate reasonable cycle length (21-45 days)
                if (cycleLength >= 21 && cycleLength <= 45)
                    cycleLengths.Add(cycleLength);
            }

            if (!cycleLengths.Any())
            {
                return (DateTime.UtcNow.AddDays(28), 28, 30);
            }

            var averageCycleLength = cycleLengths.Average();
            var lastCycleStart = trackingsList.First().CycleStartDate; // First is most recent due to OrderByDescending
            var nextPeriodStart = lastCycleStart.AddDays(averageCycleLength);
            
            // Calculate confidence based on cycle regularity and data points
            var confidence = CalculateConfidenceScore(cycleLengths);
            
            return (nextPeriodStart, averageCycleLength, confidence);
        }

        private DateTime CalculateOvulationDate(DateTime nextPeriodStart, double cycleLength)
        {
            // Luteal phase is typically 12-16 days (average 14)
            // For shorter cycles, luteal phase might be shorter
            double lutealPhaseLength = 14;
            
            if (cycleLength < 28)
                lutealPhaseLength = Math.Max(10, cycleLength * 0.5);
            else if (cycleLength > 35)
                lutealPhaseLength = Math.Min(16, cycleLength * 0.45);
            
            return nextPeriodStart.AddDays(-lutealPhaseLength);
        }

        private (DateTime start, DateTime end) CalculateFertilityWindow(DateTime ovulationDate)
        {
            return (
                start: ovulationDate.AddDays(-5), // 5 days before ovulation (sperm survival)
                end: ovulationDate                // Ovulation day (egg survives ~24h)
            );
        }

        private int CalculateConfidenceScore(List<double> cycleLengths)
        {
            if (cycleLengths.Count < 2) return 30; // Low confidence for insufficient data
            
            // Calculate standard deviation
            var mean = cycleLengths.Average();
            var standardDeviation = Math.Sqrt(cycleLengths.Select(x => Math.Pow(x - mean, 2)).Average());
            
            // Base confidence on regularity and data points
            var regularityScore = Math.Max(0, 100 - (standardDeviation * 10)); // Lower deviation = higher score
            var dataPointBonus = Math.Min(20, cycleLengths.Count * 4); // More data = higher confidence
            
            var confidence = (int)(regularityScore * 0.8 + dataPointBonus);
            return Math.Max(30, Math.Min(95, confidence)); // Clamp between 30-95%
        }

        private bool IsRegularCycle(List<double> cycleLengths)
        {
            if (cycleLengths.Count < 3) return false;
            
            var mean = cycleLengths.Average();
            var standardDeviation = Math.Sqrt(cycleLengths.Select(x => Math.Pow(x - mean, 2)).Average());
            
            return standardDeviation <= 6; // Cycles within 6 days variation = regular
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

                // Don't schedule notifications in the past
                if (scheduledDate < DateTime.UtcNow.Date)
                {
                    _logger.LogWarning("Skipping notification scheduled for past date: {ScheduledDate}", scheduledDate);
                    return true; // Return true to not break the flow
                }
                
                scheduledDate = DateTime.SpecifyKind(scheduledDate, DateTimeKind.Utc);

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

                await _unitOfWork.GetRepository<MenstrualCycleNotification>().InsertAsync(notification);
                
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
                var subject = "Fertility Window Reminder - Everwell Healthcare";
                var body = GenerateFertilityWindowEmail(user.Name, windowStart, windowEnd);
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        private async Task SendNotificationEmail(MenstrualCycleNotification notification)
        {
            var subject = GetEmailSubject(notification.Phase);
            var body = GenerateEmailBody(notification.Tracking.Customer?.Name ?? "User", notification.Message, notification.Phase);
            
            await _emailService.SendEmailAsync(notification.Tracking.Customer?.Email, subject, body);
        }

        private string GetEmailSubject(MenstrualCyclePhase phase)
        {
            return phase switch
            {
                MenstrualCyclePhase.Menstrual => "Period Reminder - Everwell Healthcare",
                MenstrualCyclePhase.Ovulation => "Ovulation Reminder - Everwell Healthcare",
                MenstrualCyclePhase.Follicular => "Fertility Window Reminder - Everwell Healthcare",
                MenstrualCyclePhase.Luteal => "Cycle Update - Everwell Healthcare",
                _ => "Menstrual Cycle Reminder - Everwell Healthcare"
            };
        }

        private string GenerateEmailBody(string userName, string message, MenstrualCyclePhase phase)
        {
            var phaseDescription = phase switch
            {
                MenstrualCyclePhase.Menstrual => "Period",
                MenstrualCyclePhase.Follicular => "Fertility Window",
                MenstrualCyclePhase.Ovulation => "Ovulation",
                MenstrualCyclePhase.Luteal => "Luteal Phase",
                _ => "Cycle Update"
            };

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>Hello {userName},</h2>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #2c3e50; margin-top: 0;'>{phaseDescription} Reminder</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>{message}</p>
                        </div>
                        
                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='font-size: 14px; color: #666;'>
                                Take care of yourself and track your symptoms in the Everwell app.
                            </p>
                            <p style='font-size: 12px; color: #999;'>
                                This is an automated reminder from Everwell Healthcare. 
                                If you wish to modify your notification preferences, please log into your account.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePeriodReminderEmail(string userName, DateTime predictedDate)
        {
            return $"Hello {userName}, your period is expected to start on {predictedDate:MMMM dd, yyyy}. Make sure you're prepared!";
        }

        private string GenerateOvulationReminderEmail(string userName, DateTime ovulationDate)
        {
            return $"Hello {userName}, your ovulation is expected on {ovulationDate:MMMM dd, yyyy}. Your fertility window is active!";
        }

        private string GenerateFertilityWindowEmail(string userName, DateTime windowStart, DateTime windowEnd)
        {
            return $"Hello {userName}, your fertility window is from {windowStart:MMMM dd} to {windowEnd:MMMM dd, yyyy}.";
        }

        public async Task<bool> TestCreateNotificationDirectlyAsync(Guid trackingId)
        {
            try
            {
                _logger.LogInformation("Testing direct notification creation for tracking {TrackingId}", trackingId);
                
                // Get tracking to verify it exists
                var tracking = await _unitOfWork.GetRepository<MenstrualCycleTracking>()
                    .FirstOrDefaultAsync(predicate: t => t.TrackingId == trackingId,
                                       include: q => q.Include(t => t.Customer));
                
                if (tracking == null)
                {
                    _logger.LogError("Tracking not found for testing: {TrackingId}", trackingId);
                    return false;
                }
                
                _logger.LogInformation("Found tracking: {TrackingId}, Customer: {CustomerId}, NotificationEnabled: {NotificationEnabled}",
                    tracking.TrackingId, tracking.CustomerId, tracking.NotificationEnabled);
                
                var testNotification = new MenstrualCycleNotification
                {
                    NotificationId = Guid.NewGuid(),
                    TrackingId = trackingId,
                    Phase = MenstrualCyclePhase.Menstrual,
                    SentAt = DateTime.UtcNow.AddDays(1),
                    Message = "Test notification - direct creation",
                    IsSent = false
                };
                
                await _unitOfWork.GetRepository<MenstrualCycleNotification>().InsertAsync(testNotification);
                await _unitOfWork.SaveChangesAsync();
                
                // Verify it was saved
                var savedNotification = await _unitOfWork.GetRepository<MenstrualCycleNotification>()
                    .FirstOrDefaultAsync(predicate: n => n.NotificationId == testNotification.NotificationId);
                
                var success = savedNotification != null;
                _logger.LogInformation("Test notification creation result: {Success}", success);
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test notification creation");
                return false;
            }
        }
    }
}
