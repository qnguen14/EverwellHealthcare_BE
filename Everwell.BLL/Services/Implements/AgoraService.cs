using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Everwell.BLL.Services.Implements;

public class AgoraService : IAgoraService
{
    private readonly ILogger<AgoraService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string? _appId;
    private readonly string? _appCertificate;
    private readonly string? _baseUrl;
    private readonly int _tokenExpirationTime;

    public AgoraService(ILogger<AgoraService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _appId = configuration["Agora:AppId"];
        _appCertificate = configuration["Agora:AppCertificate"];
        _baseUrl = configuration["Agora:BaseUrl"];
        _tokenExpirationTime = int.Parse(configuration["Agora:TokenExpirationTime"] ?? "3600");
    }

    public async Task<AgoraChannelInfo> CreateChannelAsync(Appointment appointment)
    {
        try
        {
            var channelName = GenerateChannelName(appointment);
            var startTime = GetAppointmentStartTime(appointment);
            var endTime = GetAppointmentEndTime(appointment);
            
            // Generate RTC token for the channel
            var rtcToken = await GenerateRtcTokenAsync(
                channelName,
                (uint)appointment.CustomerId.GetHashCode()
            );
            
            // Convert to local time for display
            var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var startTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, utcPlus7);
            var endTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, utcPlus7);

            var channelInfo = new AgoraChannelInfo
            {
                ChannelName = channelName,
                ChannelUrl = $"{_baseUrl}/{appointment.Id}",
                RtcToken = rtcToken,
                StartTime = startTimeLocal,
                EndTime = endTimeLocal,
                IsActive =
                    DateTime.UtcNow >= startTime.AddMinutes(-5) && DateTime.UtcNow <= endTime,
                IsEnabled = false, // Will be enabled by background service
            };

            _logger.LogInformation(
                "Created Agora channel for appointment {AppointmentId}: {ChannelName}",
                appointment.Id,
                channelName
            );

            return channelInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create Agora channel for appointment {AppointmentId}",
                appointment.Id
            );
            throw;
        }
    }

    public Task<string> GenerateRtcTokenAsync(
        string channelName,
        uint userId,
        string role = "publisher"
    )
    {
        try
        {
            if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_appCertificate))
            {
                throw new InvalidOperationException(
                    "Agora App ID or App Certificate not configured"
                );
            }

            // Calculate expiration time (current time + configured expiration)
            var expireTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _tokenExpirationTime;
            
            // Use Agora's token generation logic
            var token = GenerateAgoraToken(
                _appId,
                _appCertificate,
                channelName,
                userId,
                expireTimestamp
            );
            
            _logger.LogInformation(
                "Generated RTC token for channel {ChannelName}, user {UserId}",
                channelName,
                userId
            );
            
            return Task.FromResult(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to generate RTC token for channel {ChannelName}",
                channelName
            );
            throw;
        }
    }

    public Task<bool> EnableChannelAsync(string channelName)
    {
        try
        {
            // In Agora, channels are created on-demand when users join
            // This method can be used to mark the channel as "enabled" in your system
            _logger.LogInformation("Enabled Agora channel: {ChannelName}", channelName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable channel {ChannelName}", channelName);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisableChannelAsync(string channelName)
    {
        try
        {
            // Mark channel as disabled - implement your logic here
            _logger.LogInformation("Disabled Agora channel: {ChannelName}", channelName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable channel {ChannelName}", channelName);
            return Task.FromResult(false);
        }
    }

    public Task<AgoraChannelInfo> GetChannelInfoAsync(string channelName)
    {
        // Implement channel info retrieval logic
        throw new NotImplementedException();
    }

    public Task<bool> IsChannelActiveAsync(string channelName)
    {
        // Implement channel activity check
        throw new NotImplementedException();
    }

    public Task<bool> DeleteChannelAsync(string channelName)
    {
        try
        {
            // Agora channels don't need explicit deletion
            // Implement any cleanup logic here
            _logger.LogInformation("Deleted Agora channel: {ChannelName}", channelName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete channel {ChannelName}", channelName);
            return Task.FromResult(false);
        }
    }

    private string GenerateChannelName(Appointment appointment)
    {
        return $"healthcare-{appointment.Id:N}";
    }

    private DateTime GetAppointmentStartTime(Appointment appointment)
    {
        var baseDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);
        var localTime = appointment.Slot switch
        {
            ShiftSlot.Morning1 => baseDate.AddHours(8), // 08:00 – 10:00
            ShiftSlot.Morning2 => baseDate.AddHours(10), // 10:00 – 12:00
            ShiftSlot.Afternoon1 => baseDate.AddHours(13), // 13:00 – 15:00
            ShiftSlot.Afternoon2 => baseDate.AddHours(15), // 15:00 – 17:00
            _ => baseDate.AddHours(8),
        };

        // Convert from UTC+7 to UTC
        var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeToUtc(localTime, utcPlus7);
    }

    private DateTime GetAppointmentEndTime(Appointment appointment)
    {
        return GetAppointmentStartTime(appointment).AddHours(2);
    }

    private string GenerateAgoraToken(
        string appId,
        string appCertificate,
        string channelName,
        uint userId,
        long expireTimestamp
    )
    {
        try
        {
            // Create a basic Agora-compatible token for development
            // This is a simplified version - for production, use the official Agora SDK
            
            // Create token payload
            var payload = new
            {
                iss = appId,
                exp = expireTimestamp,
                channel = channelName,
                uid = userId,
                role = 1 // Publisher role
            };
            
            // Convert to JSON
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            var payloadBase64 = Convert.ToBase64String(payloadBytes);
            
            // Create signature using HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
            var signatureBase64 = Convert.ToBase64String(signatureBytes);
            
            // Combine to create token
            var token = $"agora_{payloadBase64}.{signatureBase64}";
            
            _logger.LogInformation("Generated development Agora token for channel {ChannelName}, user {UserId}", 
                channelName, userId);
                
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Agora token");
            
            // Fallback to simple token for development
            var fallbackToken = $"dev_token_{appId}_{channelName}_{userId}_{expireTimestamp}";
            
            _logger.LogWarning(
                "Using fallback development token. Replace with proper Agora token generation in production."
            );
            
            return fallbackToken;
        }
    }
}
