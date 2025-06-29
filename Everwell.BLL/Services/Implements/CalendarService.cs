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
    private readonly IDailyService _dailyService;

    public CalendarService(
        IUnitOfWork<EverwellDbContext> unitOfWork,
        ILogger<CalendarService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IDailyService dailyService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _configuration = configuration;
        _dailyService = dailyService;
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

            _logger.LogInformation("🔍 DEBUG - About to call _dailyService.EnsureRoomAsync");
            var meetingUrl = await _dailyService.EnsureRoomAsync(appointment);
            
            _logger.LogInformation("✅ Created Daily room for appointment {AppointmentId}: {Url}", appointment.Id, meetingUrl);
            
            return meetingUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create Daily video meeting for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<bool> UpdateCalendarEventAsync(string eventId, Appointment appointment)
    {
        try
        {
            // For Daily we simply ensure a room exists (idempotent) and ignore eventId
            await _dailyService.EnsureRoomAsync(appointment);
            _logger.LogInformation("Updated Daily room for appointment {AppointmentId}", appointment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Daily room for appointment {AppointmentId}", appointment.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCalendarEventAsync(string eventId)
    {
        try
        {
            // Daily rooms auto-expire; nothing to delete.
            _logger.LogInformation("DeleteCalendarEventAsync called for {EventId} – no action needed for Daily", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Daily room {EventId}", eventId);
            return false;
        }
    }

    public async Task<string> GenerateMeetingLinkAsync(string eventId)
    {
        try
        {
            _logger.LogInformation("Generating meeting link for Daily room {EventId}", eventId);
            return $"https://{_configuration["Daily:Domain"]}/{eventId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate meeting link for Daily room {EventId}", eventId);
            return string.Empty;
        }
    }

    public async Task<bool> CreateSimpleCalendarEventAsync(Appointment appointment)
    {
        try
        {
            await _dailyService.EnsureRoomAsync(appointment);
            _logger.LogInformation("Created Daily room for appointment {AppointmentId}", appointment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Daily room for appointment {AppointmentId}", appointment.Id);
            return false;
        }
    }
} 