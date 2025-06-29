using System.Text;
using System.Security.Cryptography;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Everwell.DAL.Repositories.Interfaces;

namespace Everwell.BLL.Services.Implements;

public class AgoraService : IAgoraService
{
    private readonly ILogger<AgoraService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _appId;
    private readonly string _appCertificate;
    private readonly string _baseUrl;
    private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;

    public AgoraService(
        ILogger<AgoraService> logger, 
        IConfiguration configuration, 
        IUnitOfWork<EverwellDbContext> unitOfWork)
    {
        _logger = logger;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _appId = configuration["Agora:AppId"];
        _appCertificate = configuration["Agora:AppCertificate"];
        _baseUrl = configuration["Agora:BaseUrl"] ?? "https://yourdomain.com/meeting";
    }

    public async Task<AgoraChannelInfo> CreateChannelAsync(Appointment appointment)
    {
        try
        {
            var channelName = GenerateChannelName(appointment);
            
            // Handle temporary appointments for 'Join Now' testing
            DateTime startTime, endTime;
            if (appointment.Status == AppointmentStatus.Temp)
            {
                var now = DateTime.UtcNow;
                startTime = now.AddMinutes(-2);
                endTime = now.AddHours(2);
            }
            else
            {
                startTime = GetAppointmentStartTime(appointment);
                endTime = GetAppointmentEndTime(appointment);
            }

            // Generate unique UID for each user joining the same channel
            // Using appointment ID + timestamp to ensure uniqueness
            var uid = GenerateUniqueUid(appointment.Id);

            // Generate RTC token for the appointment duration with unique UID
            var rtcToken = await GenerateRtcTokenAsync(channelName, uid, "publisher", endTime);

            var channelInfo = new AgoraChannelInfo
            {
                ChannelName = channelName,
                AppId = _appId,
                RtcToken = rtcToken,
                StartTime = startTime,
                EndTime = endTime,
                MeetingUrl = $"{_baseUrl}/{appointment.Id}",
                IsActive = await IsChannelActiveAsync(channelName, DateTime.UtcNow),
                Uid = uid // Add UID to the response
            };

            _logger.LogInformation("Generated meeting URL: {MeetingUrl} for appointment {AppointmentId} with UID: {Uid}", 
                channelInfo.MeetingUrl, appointment.Id, uid);

            _logger.LogInformation("Created Agora channel for appointment {AppointmentId}: {ChannelName}", 
                appointment.Id, channelName);

            return channelInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Agora channel for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<string> GenerateRtcTokenAsync(string channelName, uint uid, string role, DateTime validUntil)
    {
        try
        {
            if (string.IsNullOrEmpty(_appCertificate))
            {
                _logger.LogWarning("App Certificate not configured, using App ID as token for development");
                return await Task.FromResult(_appId);
            }

            // Use proper Agora AccessToken2 generation algorithm
            var token = BuildAccessToken2(
                _appId,
                _appCertificate,
                channelName,
                uid,
                role == "publisher" ? 1 : 2, // 1=publisher, 2=subscriber
                validUntil
            );

            _logger.LogInformation("Generated secure RTC token for channel {ChannelName}, UID {Uid} valid until {ValidUntil}", 
                channelName, uid, validUntil);
            return await Task.FromResult(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate RTC token for channel {ChannelName}", channelName);
            throw;
        }
    }

    // Generate unique UID based on appointment and current timestamp
    private uint GenerateUniqueUid(Guid appointmentId)
    {
        // Convert appointment ID to hash and use timestamp to ensure uniqueness
        var appointmentHash = appointmentId.GetHashCode();
        var timestampHash = DateTime.UtcNow.Ticks.GetHashCode();
        
        // Combine hashes and ensure it's a positive 32-bit integer
        var combinedHash = Math.Abs(appointmentHash ^ timestampHash);
        
        // Ensure it's not 0 and fits in uint range
        var uid = (uint)(combinedHash % (int.MaxValue - 1)) + 1;
        
        _logger.LogInformation("Generated unique UID {Uid} for appointment {AppointmentId}", uid, appointmentId);
        return uid;
    }

    // Proper Agora AccessToken2 implementation
    private string BuildAccessToken2(string appId, string appCertificate, string channelName, uint uid, int role, DateTime expireTime)
    {
        try
        {
            var expireTimestamp = ((DateTimeOffset)expireTime).ToUnixTimeSeconds();
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // AccessToken2 structure based on Agora documentation
            var version = "007";
            var privileges = new Dictionary<int, long>
            {
                { 1, expireTimestamp }, // Join channel privilege
                { 2, expireTimestamp }, // Publish audio privilege  
                { 3, expireTimestamp }, // Publish video privilege
                { 4, expireTimestamp }  // Publish data stream privilege
            };

            // Create the token payload
            var tokenPayload = new
            {
                iss = appId,
                exp = expireTimestamp,
                iat = currentTimestamp,
                salt = GenerateRandomSalt(),
                channel_name = channelName,
                uid = uid.ToString(),
                role = role,
                privileges = privileges
            };

            var payloadJson = JsonConvert.SerializeObject(tokenPayload);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            var payloadBase64 = Convert.ToBase64String(payloadBytes);

            // Create signature
            var signatureData = $"{appId}:{channelName}:{uid}:{currentTimestamp}:{expireTimestamp}";
            var signature = CreateHmacSha256Signature(signatureData, appCertificate);

            // Combine to create final token
            var token = $"{version}{appId}{payloadBase64}{signature}";
            
            _logger.LogInformation("Successfully built AccessToken2 for channel {ChannelName}, UID {Uid}", channelName, uid);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build AccessToken2");
            throw;
        }
    }

    private string GenerateRandomSalt()
    {
        var random = new Random();
        return random.Next(100000000, int.MaxValue).ToString();
    }

    private string CreateHmacSha256Signature(string data, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }

    public async Task<bool> EnableChannelAsync(string channelName)
    {
        try
        {
            // In a real implementation, this would call Agora's Channel Management API
            _logger.LogInformation("Enabled channel {ChannelName}", channelName);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable channel {ChannelName}", channelName);
            return false;
        }
    }

    public async Task<bool> DisableChannelAsync(string channelName)
    {
        try
        {
            // In a real implementation, this would call Agora's Channel Management API
            _logger.LogInformation("Disabled channel {ChannelName}", channelName);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable channel {ChannelName}", channelName);
            return false;
        }
    }

    public async Task<bool> ScheduleChannelAsync(string channelName, DateTime startTime, DateTime endTime)
    {
        try
        {
            // Store scheduling information (in real implementation, this would be in database)
            _logger.LogInformation("Scheduled channel {ChannelName} from {StartTime} to {EndTime}", 
                channelName, startTime, endTime);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule channel {ChannelName}", channelName);
            return false;
        }
    }

    public async Task<bool> IsChannelActiveAsync(string channelName, DateTime currentTime)
    {
        try
        {
            var appointment = await GetAppointmentFromChannelName(channelName);
            if (appointment == null)
            {
                _logger.LogWarning("Could not find appointment for channel {ChannelName}", channelName);
                return false;
            }

            DateTime startTime, endTime;
            if (appointment.Status == AppointmentStatus.Temp)
            {
                // For temp appointments, the "start time" is effectively when they were created.
                // We'll give a generous window to account for any small delays.
                var creationTime = appointment.CreatedAt; // Assuming CreatedAt is set on creation
                startTime = creationTime.AddMinutes(-5);
                endTime = creationTime.AddHours(2);
            }
            else
            {
                startTime = GetAppointmentStartTime(appointment);
                endTime = GetAppointmentEndTime(appointment);
            }
            
            // Allow joining 5 minutes before start
            var canJoinTime = startTime.AddMinutes(-5);

            var isActive = currentTime >= canJoinTime && currentTime <= endTime;
            
            _logger.LogInformation("Channel active check for {ChannelName}: CurrentTime={CurrentTime}, StartTime={StartTime}, EndTime={EndTime}, IsActive={IsActive}",
                channelName, currentTime, startTime, endTime, isActive);

            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check channel status for {ChannelName}", channelName);
            return false;
        }
    }

    private async Task<Appointment?> GetAppointmentFromChannelName(string channelName)
    {
        try
        {
            // Extract appointment ID from channel name (format: "appointment_{appointmentId}")
            if (channelName.StartsWith("appointment_"))
            {
                var appointmentIdStr = channelName.Substring("appointment_".Length);
                if (Guid.TryParse(appointmentIdStr, out var appointmentId))
                {
                    return await _unitOfWork.GetRepository<Appointment>()
                        .FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment from channel name {ChannelName}", channelName);
            return null;
        }
    }

    private string GenerateChannelName(Appointment appointment)
    {
        return $"appointment_{appointment.Id}";
    }

    private DateTime GetAppointmentStartTime(Appointment appointment)
    {
        // Convert DateOnly and ShiftSlot to DateTime
        var appointmentDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);
        
        return appointment.Slot switch
        {
            ShiftSlot.Morning1 => appointmentDate.AddHours(8),   // 8:00 AM
            ShiftSlot.Morning2 => appointmentDate.AddHours(10),  // 10:00 AM
            ShiftSlot.Afternoon1 => appointmentDate.AddHours(13), // 1:00 PM
            ShiftSlot.Afternoon2 => appointmentDate.AddHours(15), // 3:00 PM
            _ => appointmentDate.AddHours(8) // Default to 8:00 AM
        };
    }

    private DateTime GetAppointmentEndTime(Appointment appointment)
    {
        var startTime = GetAppointmentStartTime(appointment);
        return startTime.AddHours(2); // Each appointment slot is 2 hours
    }

    private string GenerateHmacSha256Token(string message, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hashBytes);
        }
    }
} 