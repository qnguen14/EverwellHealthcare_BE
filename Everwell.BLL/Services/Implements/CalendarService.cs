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

    public CalendarService(
        IUnitOfWork<EverwellDbContext> unitOfWork,
        ILogger<CalendarService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateVideoMeetingAsync(Appointment appointment)
    {
        try
        {
            // Generate Jitsi Meet link
            var jitsiMeetLink = GenerateJitsiMeetLink(appointment);
            
            _logger.LogInformation("Generated Jitsi Meet link for appointment {AppointmentId}: {MeetingLink}", 
                appointment.Id, jitsiMeetLink);
            
            return await Task.FromResult(jitsiMeetLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create video meeting for appointment {AppointmentId}", appointment.Id);
            
            // Even if there's an error, still return the Jitsi Meet link
            return await Task.FromResult(GenerateJitsiMeetLink(appointment));
        }
    }

    private string GenerateJitsiMeetLink(Appointment appointment)
    {
        // Generate a unique room name based on appointment details
        var roomName = $"everwell-{appointment.Id.ToString("N")[..12]}-{appointment.AppointmentDate:yyyyMMdd}";
        
        // Use custom Jitsi domain if configured, otherwise use meet.jit.si
        var jitsiDomain = _configuration["JitsiMeet:Domain"] ?? "meet.jit.si";
        var meetingLink = $"https://{jitsiDomain}/{roomName}";
        
        _logger.LogInformation("Generated Jitsi Meet link for appointment {AppointmentId}: {MeetingLink}", 
            appointment.Id, meetingLink);
        
        return meetingLink;
    }

    public async Task<bool> UpdateCalendarEventAsync(string eventId, Appointment appointment)
    {
        // For Jitsi Meet, we don't need to update external calendar events
        // The meeting link is regenerated based on appointment details
        _logger.LogInformation("Calendar event update requested for appointment {AppointmentId}, but not needed for Jitsi Meet", appointment.Id);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteCalendarEventAsync(string eventId)
    {
        // For Jitsi Meet, we don't need to delete external calendar events
        // The meeting room is automatically cleaned up by Jitsi
        _logger.LogInformation("Calendar event deletion requested for event {EventId}, but not needed for Jitsi Meet", eventId);
        return await Task.FromResult(true);
    }

    public async Task<string> GenerateMeetingLinkAsync(string eventId)
    {
        // For Jitsi Meet, we can't retrieve the link from external events
        // This would need to be stored in the database or regenerated
        _logger.LogWarning("Cannot retrieve Jitsi Meet link from external event {EventId}. Link should be stored in database.", eventId);
        return await Task.FromResult(string.Empty);
    }

    public async Task<bool> CreateSimpleCalendarEventAsync(Appointment appointment)
    {
        // This is a no-op for Jitsi Meet since we don't integrate with external calendars
        // Users can manually add the Jitsi Meet link to their own calendars
        _logger.LogInformation("Simple calendar event creation requested for appointment {AppointmentId}. With Jitsi Meet, users can manually add the meeting link to their calendars.", appointment.Id);
        return await Task.FromResult(true);
    }
} 