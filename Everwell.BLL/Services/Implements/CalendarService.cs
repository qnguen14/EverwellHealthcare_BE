using AutoMapper;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class CalendarService : BaseService<CalendarService>, ICalendarService
{
    private readonly IConfiguration _configuration;
    private readonly IAgoraService _agoraService;

    public CalendarService(
        IUnitOfWork<EverwellDbContext> unitOfWork,
        ILogger<CalendarService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IAgoraService agoraService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _configuration = configuration;
        _agoraService = agoraService;
    }

    public async Task<string> CreateVideoMeetingAsync(Appointment appointment)
    {
        _logger.LogInformation("🔍 DEBUG - CalendarService.CreateVideoMeetingAsync called for appointment {AppointmentId}", appointment.Id);
        
        try
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null");
            }

            _logger.LogInformation("🔍 DEBUG - About to call _agoraService.CreateChannelAsync");
            // Create Agora channel with time-based access
            var channelInfo = await _agoraService.CreateChannelAsync(appointment);
            
            if (channelInfo == null)
            {
                throw new Exception("AgoraService returned null channel info");
            }

            if (string.IsNullOrEmpty(channelInfo.MeetingUrl))
            {
                throw new Exception("AgoraService returned empty meeting URL");
            }

            _logger.LogInformation("🔍 DEBUG - _agoraService.CreateChannelAsync succeeded, channelInfo.MeetingUrl: {MeetingUrl}", channelInfo.MeetingUrl);
            
            _logger.LogInformation("✅ Created Agora channel for appointment {AppointmentId}: {ChannelName} from {StartTime} to {EndTime}", 
                appointment.Id, channelInfo.ChannelName, channelInfo.StartTime, channelInfo.EndTime);
            
            return channelInfo.MeetingUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create Agora video meeting for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<bool> UpdateCalendarEventAsync(string eventId, Appointment appointment)
    {
        try
        {
            // Disable old channel and create new one for updated appointment
            await _agoraService.DisableChannelAsync(eventId);
            var channelInfo = await _agoraService.CreateChannelAsync(appointment);
            
            _logger.LogInformation("Updated Agora channel for appointment {AppointmentId}", appointment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Agora channel for appointment {AppointmentId}", appointment.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCalendarEventAsync(string eventId)
    {
        try
        {
            // Disable the Agora channel
            var result = await _agoraService.DisableChannelAsync(eventId);
            
            _logger.LogInformation("Disabled Agora channel {ChannelName}", eventId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable Agora channel {ChannelName}", eventId);
            return false;
        }
    }

    public async Task<string> GenerateMeetingLinkAsync(string eventId)
    {
        try
        {
            // For Agora, we need the full appointment info to generate proper channel data
            _logger.LogInformation("Generating new meeting link for channel {ChannelName}", eventId);
            
            // This is a simplified approach - in production you might want to store more info
            // or reconstruct the appointment from the eventId
            return $"{_configuration["Agora:BaseUrl"]}/{eventId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate meeting link for channel {ChannelName}", eventId);
            return string.Empty;
        }
    }

    public async Task<bool> CreateSimpleCalendarEventAsync(Appointment appointment)
    {
        try
        {
            // Create Agora channel with time-based access
            var channelInfo = await _agoraService.CreateChannelAsync(appointment);
            
            _logger.LogInformation("Created simple calendar event for appointment {AppointmentId}", appointment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create simple calendar event for appointment {AppointmentId}", appointment.Id);
            return false;
        }
    }
} 