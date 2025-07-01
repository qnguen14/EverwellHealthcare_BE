using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IDailyService
{
    Task<DailyRoomInfo> CreateRoomAsync(Appointment appointment);
    Task<DailyRoomInfo> CreatePreScheduledRoomAsync(Appointment appointment);
    Task<string> GetRoomUrlAsync(string roomName);
    Task<bool> DeleteRoomAsync(string roomName);
    Task<DailyRoomInfo> GetRoomInfoAsync(string roomName);
    Task<bool> IsRoomActiveAsync(string roomName);
    Task<string> GenerateMeetingTokenAsync(string roomName, Guid userId, string userName, bool isOwner = false);
}

public class DailyRoomInfo
{
    public string RoomName { get; set; }
    public string RoomUrl { get; set; }
    public string MeetingUrl { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsPreScheduled { get; set; }
    public Dictionary<string, object> Config { get; set; } = new();
} 