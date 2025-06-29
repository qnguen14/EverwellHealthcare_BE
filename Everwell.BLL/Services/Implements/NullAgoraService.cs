using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Implements;

/// <summary>
/// Placeholder implementation of IAgoraService after migrating to Daily.
/// All methods are no-ops that return default values so existing services depending on the interface continue to resolve.
/// Remove this class once all Agora references are cleaned up.
/// </summary>
public class NullAgoraService : IAgoraService
{
    public Task<AgoraChannelInfo> CreateChannelAsync(Appointment appointment)
        => Task.FromResult(new AgoraChannelInfo
        {
            ChannelName = $"deprecated_{appointment.Id}",
            MeetingUrl = string.Empty,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            IsActive = false,
            AppId = string.Empty,
            RtcToken = string.Empty,
            Uid = 0
        });

    public Task<string> GenerateRtcTokenAsync(string channelName, uint uid, string role, DateTime validUntil)
        => Task.FromResult(string.Empty);

    public Task<bool> EnableChannelAsync(string channelName) => Task.FromResult(false);
    public Task<bool> DisableChannelAsync(string channelName) => Task.FromResult(true);
    public Task<bool> ScheduleChannelAsync(string channelName, DateTime startTime, DateTime endTime) => Task.FromResult(true);
    public Task<bool> IsChannelActiveAsync(string channelName, DateTime currentTime) => Task.FromResult(false);
} 