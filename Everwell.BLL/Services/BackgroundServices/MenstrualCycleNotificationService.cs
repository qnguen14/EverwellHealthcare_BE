using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.BackgroundServices
{
    public class MenstrualCycleNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MenstrualCycleNotificationService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); // Check every 6 hours

        public MenstrualCycleNotificationService(
            IServiceProvider serviceProvider,
            ILogger<MenstrualCycleNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotifications();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing menstrual cycle notifications");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task ProcessNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<IMenstrualCycleNotificationService>();
            
            await notificationService.ProcessPendingNotificationsAsync();
        }
    }
}
