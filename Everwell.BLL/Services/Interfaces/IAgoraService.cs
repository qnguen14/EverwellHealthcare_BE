using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IAgoraService
{
    Task<AgoraChannelInfo> CreateChannelAsync(Appointment appointment);
    Task<string> GenerateRtcTokenAsync(string channelName, uint uid, string role, DateTime validUntil);
    Task<bool> EnableChannelAsync(string channelName);
    Task<bool> DisableChannelAsync(string channelName);
    Task<bool> ScheduleChannelAsync(string channelName, DateTime startTime, DateTime endTime);
    Task<bool> IsChannelActiveAsync(string channelName, DateTime currentTime);
}

public class AgoraChannelInfo
{
    public string ChannelName { get; set; }
    public string AppId { get; set; }
    public string RtcToken { get; set; }
    public uint Uid { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string MeetingUrl { get; set; }
    public bool IsActive { get; set; }
}