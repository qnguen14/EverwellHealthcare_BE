namespace Everwell.BLL.Services.Interfaces;

using Everwell.DAL.Data.Entities;

public interface IAgoraService
{
    /// <summary>
    /// Xây dựng tên kênh (channel) từ cuộc hẹn.
    /// </summary>
    string BuildChannelName(Appointment appointment);

    /// <summary>
    /// Sinh token RTC cho channel.
    /// </summary>
    /// <param name="channelName">Tên kênh</param>
    /// <param name="uid">UID của user (số dương &lt; 2^32).</param>
    /// <param name="isHost">Có phải host/bác sĩ không</param>
    string GenerateRtcToken(string channelName, uint uid, bool isHost);

    /// <summary>
    /// Trả về AppId cấu hình.
    /// </summary>
    string GetAppId();
}

public class AgoraMeetingInfo
{
    public string AppId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string RtcToken { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsHost { get; set; }
} 