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

            _logger.LogInformation("🔍 DEBUG - About to call _dailyService.CreatePreScheduledRoomAsync");
            // Create Daily.co room with pre-scheduling
            var roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
            
            if (roomInfo == null)
            {
                throw new Exception("DailyService returned null room info");
            }

            if (string.IsNullOrEmpty(roomInfo.RoomUrl))
            {
                throw new Exception("DailyService returned empty room URL");
            }

            _logger.LogInformation("🔍 DEBUG - _dailyService.CreatePreScheduledRoomAsync succeeded, roomInfo.RoomUrl: {RoomUrl}", roomInfo.RoomUrl);
            
            _logger.LogInformation("✅ Created Daily.co room for appointment {AppointmentId}: {RoomName} from {StartTime} to {EndTime}", 
                appointment.Id, roomInfo.RoomName, roomInfo.StartTime, roomInfo.EndTime);
            
            return roomInfo.RoomUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create Daily.co video meeting for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<bool> UpdateCalendarEventAsync(string eventId, Appointment appointment)
    {
        try
        {
            // For Daily.co, we'll delete the old room and create a new one
            var oldRoomName = $"appointment-{appointment.Id:N}".ToLower();
            await _dailyService.DeleteRoomAsync(oldRoomName);
            var roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
            
            _logger.LogInformation("Updated Daily.co room for appointment {AppointmentId}", appointment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Daily.co room for appointment {AppointmentId}", appointment.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCalendarEventAsync(string eventId)
    {
        try
        {
            // Delete the Daily.co room
            var result = await _dailyService.DeleteRoomAsync(eventId);
            
            _logger.LogInformation("Deleted Daily.co room {RoomName}", eventId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Daily.co room {RoomName}", eventId);
            return false;
        }
    }

    public async Task<string> GenerateMeetingLinkAsync(string eventId)
    {
        try
        {
            // For Daily.co, get the room URL directly
            _logger.LogInformation("Generating meeting link for room {RoomName}", eventId);
            
            var roomUrl = await _dailyService.GetRoomUrlAsync(eventId);
            return roomUrl ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate meeting link for room {RoomName}", eventId);
            return string.Empty;
        }
    }

    public async Task<bool> CreateSimpleCalendarEventAsync(Appointment appointment)
    {
        try
        {
            // Create Daily.co room with pre-scheduling
            var roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
            
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