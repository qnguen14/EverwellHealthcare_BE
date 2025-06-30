using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.BackgroundServices;

public class AgoraChannelManagementService : BackgroundService
{
    private readonly ILogger<AgoraChannelManagementService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public AgoraChannelManagementService(
        ILogger<AgoraChannelManagementService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agora Channel Management Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ManageChannelsAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is being stopped
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Agora Channel Management Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait longer after error
            }
        }

        _logger.LogInformation("Agora Channel Management Service stopped");
    }

    private async Task ManageChannelsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<EverwellDbContext>>();
        var agoraService = scope.ServiceProvider.GetRequiredService<IAgoraService>();

        try
        {
            var currentTime = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(currentTime);

            // Get appointments for today that are virtual
            var todayAppointments = await unitOfWork.GetRepository<Appointment>()
                .GetListAsync(predicate: a => a.AppointmentDate == today && a.IsVirtual == true);

            foreach (var appointment in todayAppointments)
            {
                var startTime = GetAppointmentStartTime(appointment);
                var endTime = GetAppointmentEndTime(appointment);
                
                var channelName = GenerateChannelName(appointment);

                // Check if we need to enable the channel (5 minutes before start time)
                if (currentTime >= startTime.AddMinutes(-5) && currentTime < startTime.AddMinutes(5) && 
                    !await IsChannelEnabledAsync(appointment))
                {
                    await agoraService.EnableChannelAsync(channelName);
                    _logger.LogInformation("Enabled Agora channel for appointment {AppointmentId} at {Time}", 
                        appointment.Id, currentTime);
                }

                // Check if we need to disable the channel (after end time)
                if (currentTime >= endTime && await IsChannelEnabledAsync(appointment))
                {
                    await agoraService.DisableChannelAsync(channelName);
                    _logger.LogInformation("Disabled Agora channel for appointment {AppointmentId} at {Time}", 
                        appointment.Id, currentTime);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing Agora channels");
        }
    }

    private DateTime GetAppointmentStartTime(Appointment appointment)
    {
        var baseDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);
        
        return appointment.Slot switch
        {
            ShiftSlot.Morning1 => baseDate.AddHours(8),   // 8:00 AM
            ShiftSlot.Morning2 => baseDate.AddHours(10),  // 10:00 AM
            ShiftSlot.Afternoon1 => baseDate.AddHours(13), // 1:00 PM
            ShiftSlot.Afternoon2 => baseDate.AddHours(15), // 3:00 PM
            _ => baseDate.AddHours(8)
        };
    }

    private DateTime GetAppointmentEndTime(Appointment appointment)
    {
        var startTime = GetAppointmentStartTime(appointment);
        return startTime.AddHours(2); // Each slot is 2 hours
    }

    private string GenerateChannelName(Appointment appointment)
    {
        return $"everwell-{appointment.Id.ToString("N")[..12]}-{appointment.AppointmentDate:yyyyMMdd}-{appointment.Slot}";
    }

    private async Task<bool> IsChannelEnabledAsync(Appointment appointment)
    {
        // In a real implementation, this would check the channel status from database or Agora API
        // For now, assume we track this in the appointment status
        return await Task.FromResult(appointment.Status == AppointmentStatus.Scheduled);
    }
} 