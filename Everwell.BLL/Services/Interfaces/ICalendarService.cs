using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface ICalendarService
{
    Task<string> CreateVideoMeetingAsync(Appointment appointment);
    Task<bool> UpdateCalendarEventAsync(string eventId, Appointment appointment);
    Task<bool> DeleteCalendarEventAsync(string eventId);
    Task<string> GenerateMeetingLinkAsync(string eventId);
    Task<bool> CreateSimpleCalendarEventAsync(Appointment appointment);
} 