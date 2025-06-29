namespace Everwell.BLL.Services.Interfaces;

using Everwell.DAL.Data.Entities;

public interface IDailyService
{
    /// <summary>
    /// Ensures a Daily room exists for the given appointment and returns the join URL.
    /// If the appointment already contains a MeetingId / GoogleMeetLink it is returned.
    /// Otherwise a new room is created via Daily REST API and the appointment is updated.
    /// </summary>
    Task<string> EnsureRoomAsync(Appointment appointment);

    // Optional helpers to match existing controller endpoints
    Task<bool> IsChannelActiveAsync(string roomIdentifier, DateTime nowUtc);

    Task<string> GenerateRtcTokenAsync(string meetingUrl, uint uid, string role, DateTime expireAtUtc);
} 