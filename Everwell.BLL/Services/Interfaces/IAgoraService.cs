using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IAgoraService
{
    Task<AgoraChannelInfo> CreateChannelAsync(Appointment appointment);
    Task<string> GenerateRtcTokenAsync(string channelName, uint userId, string role = "publisher");
    Task<bool> EnableChannelAsync(string channelName);
    Task<bool> DisableChannelAsync(string channelName);
    Task<AgoraChannelInfo> GetChannelInfoAsync(string channelName);
    Task<bool> IsChannelActiveAsync(string channelName);
    Task<bool> DeleteChannelAsync(string channelName);
}

public class AgoraChannelInfo
{
    public string ChannelName { get; set; }
    public string ChannelUrl { get; set; }
    public string RtcToken { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsEnabled { get; set; }
    public uint MaxParticipants { get; set; } = 10;
    public Dictionary<string, object> Config { get; set; } = new();
}