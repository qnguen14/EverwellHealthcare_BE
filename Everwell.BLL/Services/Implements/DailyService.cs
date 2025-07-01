using System.Text;
using System.Net.Http.Headers;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Everwell.DAL.Repositories.Interfaces;

namespace Everwell.BLL.Services.Implements;

public class DailyService : IDailyService
{
    private readonly ILogger<DailyService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _dailyApiKey;
    private readonly string _dailyDomainName;
    private readonly string _baseUrl;
    private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;

    public DailyService(
        ILogger<DailyService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork<EverwellDbContext> unitOfWork)
    {
        _logger = logger;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _httpClient = httpClientFactory.CreateClient();
        
        _dailyApiKey = configuration["Daily:ApiKey"];
        _dailyDomainName = configuration["Daily:DomainName"];
        _baseUrl = configuration["Daily:BaseUrl"] ?? "https://yourdomain.com/meeting";

        // Configure HTTP client for Daily.co API
        _httpClient.BaseAddress = new Uri("https://api.daily.co/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _dailyApiKey);
    }

    public async Task<DailyRoomInfo> CreateRoomAsync(Appointment appointment)
    {
        try
        {
            var roomName = GenerateRoomName(appointment);
            
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

            var roomConfig = new
            {
                name = roomName,
                privacy = "public",
                properties = new
                {
                    nbf = ((DateTimeOffset)startTime).ToUnixTimeSeconds(),
                    exp = ((DateTimeOffset)endTime).ToUnixTimeSeconds(),
                    enable_chat = true,
                    enable_knocking = false,
                    enable_screenshare = true,
                    enable_recording = false,
                    max_participants = 10
                }
            };

            // Try to create the room – but if it already exists, fall back to returning the existing one
            var createResponse = await _httpClient.PostAsync("rooms", new StringContent(JsonConvert.SerializeObject(roomConfig), Encoding.UTF8, "application/json"));

            HttpResponseMessage response;

            if (createResponse.IsSuccessStatusCode)
            {
                response = createResponse;
            }
            else if (createResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Check if room already exists – Daily returns 400 for duplicate names
                var existing = await _httpClient.GetAsync($"rooms/{roomName}");
                if (!existing.IsSuccessStatusCode)
                {
                    var err = await createResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create Daily.co room. Status: {Status}, Error: {Error}", createResponse.StatusCode, err);
                    throw new Exception($"Failed to create Daily.co room: {createResponse.StatusCode}");
                }
                response = existing;
            }
            else
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create Daily.co room. Status: {Status}, Error: {Error}", createResponse.StatusCode, errorContent);
                throw new Exception($"Failed to create Daily.co room: {createResponse.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var roomData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            var roomUrl = BuildRoomUrl(roomName);
            
            var roomInfo = new DailyRoomInfo
            {
                RoomName = roomName,
                RoomUrl = roomUrl,
                MeetingUrl = $"{_baseUrl}/{appointment.Id}",
                StartTime = startTime,
                EndTime = endTime,
                IsActive = DateTime.UtcNow >= startTime && DateTime.UtcNow <= endTime,
                IsPreScheduled = false
            };

            _logger.LogInformation("Created Daily.co room for appointment {AppointmentId}: {RoomName}", 
                appointment.Id, roomName);

            return roomInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Daily.co room for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<DailyRoomInfo> CreatePreScheduledRoomAsync(Appointment appointment)
    {
        try
        {
            var roomName = GenerateRoomName(appointment);
            var startTime = GetAppointmentStartTime(appointment);
            var endTime = GetAppointmentEndTime(appointment);

            // Allow immediate access: always set nbf to "now - 30s" so room opens instantly
            var nowUtc = DateTime.UtcNow;
            var roomStartTime = nowUtc.AddSeconds(-30);

            var roomConfig = new
            {
                name = roomName,
                privacy = "public",
                properties = new
                {
                    nbf = ((DateTimeOffset)roomStartTime).ToUnixTimeSeconds(),
                    exp = ((DateTimeOffset)endTime).ToUnixTimeSeconds(),
                    enable_chat = true,
                    enable_knocking = false,
                    enable_screenshare = true,
                    enable_recording = false,
                    max_participants = 10,
                    // Pre-scheduled room config
                    enable_prejoin_ui = true,
                    meeting_join_hook = $"{_baseUrl}/hooks/meeting-join"
                }
            };

            // Try to create the room – but if it already exists, fall back to returning the existing one
            var createResponse = await _httpClient.PostAsync("rooms", new StringContent(JsonConvert.SerializeObject(roomConfig), Encoding.UTF8, "application/json"));

            HttpResponseMessage response;

            if (createResponse.IsSuccessStatusCode)
            {
                response = createResponse;
            }
            else if (createResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Check if room already exists – Daily returns 400 for duplicate names
                var existing = await _httpClient.GetAsync($"rooms/{roomName}");
                if (!existing.IsSuccessStatusCode)
                {
                    var err = await createResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create Daily.co room. Status: {Status}, Error: {Error}", createResponse.StatusCode, err);
                    throw new Exception($"Failed to create Daily.co room: {createResponse.StatusCode}");
                }
                response = existing;
            }
            else
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create Daily.co room. Status: {Status}, Error: {Error}", createResponse.StatusCode, errorContent);
                throw new Exception($"Failed to create Daily.co room: {createResponse.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var roomData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            var roomUrl = BuildRoomUrl(roomName);
            
            var roomInfo = new DailyRoomInfo
            {
                RoomName = roomName,
                RoomUrl = roomUrl,
                MeetingUrl = $"{_baseUrl}/{appointment.Id}",
                StartTime = startTime,
                EndTime = endTime,
                IsActive = DateTime.UtcNow >= roomStartTime && DateTime.UtcNow <= endTime,
                IsPreScheduled = true
            };

            _logger.LogInformation("Created pre-scheduled Daily.co room for appointment {AppointmentId}: {RoomName}, available from {StartTime}", 
                appointment.Id, roomName, roomStartTime);

            return roomInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create pre-scheduled Daily.co room for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task<string> GetRoomUrlAsync(string roomName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"rooms/{roomName}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var roomData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                
                return BuildRoomUrl(roomName);
            }
            else
            {
                _logger.LogWarning("Room {RoomName} not found", roomName);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get room URL for {RoomName}", roomName);
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(string roomName)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"rooms/{roomName}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted Daily.co room: {RoomName}", roomName);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to delete Daily.co room {RoomName}. Status: {Status}", 
                    roomName, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Daily.co room {RoomName}", roomName);
            return false;
        }
    }

    public async Task<DailyRoomInfo> GetRoomInfoAsync(string roomName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"rooms/{roomName}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var roomData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                
                var roomUrl = BuildRoomUrl(roomName);
                var startTime = DateTimeOffset.FromUnixTimeSeconds((long)roomData.config.nbf).DateTime;
                var endTime = DateTimeOffset.FromUnixTimeSeconds((long)roomData.config.exp).DateTime;
                
                return new DailyRoomInfo
                {
                    RoomName = roomName,
                    RoomUrl = roomUrl,
                    MeetingUrl = roomUrl, // For Daily.co, these are the same
                    StartTime = startTime,
                    EndTime = endTime,
                    IsActive = DateTime.UtcNow >= startTime && DateTime.UtcNow <= endTime,
                    IsPreScheduled = roomData.config.enable_prejoin_ui == true
                };
            }
            else
            {
                _logger.LogWarning("Room {RoomName} not found", roomName);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get room info for {RoomName}", roomName);
            throw;
        }
    }

    public async Task<bool> IsRoomActiveAsync(string roomName)
    {
        try
        {
            var roomInfo = await GetRoomInfoAsync(roomName);
            return roomInfo?.IsActive ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if room {RoomName} is active", roomName);
            return false;
        }
    }

    private string GenerateRoomName(Appointment appointment)
    {
        // Generate a unique room name based on appointment
        var baseRoomName = $"appointment-{appointment.Id:N}";
        return baseRoomName.ToLower();
    }

    private DateTime GetAppointmentStartTime(Appointment appointment)
    {
        // Convert AppointmentDate (DateOnly) to DateTime at 00:00 local time
        var baseDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);

        return appointment.Slot switch
        {
            ShiftSlot.Morning1 => baseDate.AddHours(8),   // 08:00 – 10:00
            ShiftSlot.Morning2 => baseDate.AddHours(10),  // 10:00 – 12:00
            ShiftSlot.Afternoon1 => baseDate.AddHours(13), // 13:00 – 15:00
            ShiftSlot.Afternoon2 => baseDate.AddHours(15), // 15:00 – 17:00
            _ => baseDate.AddHours(8)
        };
    }

    private DateTime GetAppointmentEndTime(Appointment appointment)
    {
        // Each time slot lasts 2 hours
        return GetAppointmentStartTime(appointment).AddHours(2);
    }

    private string BuildRoomUrl(string roomName)
    {
        if (string.IsNullOrWhiteSpace(_dailyDomainName))
        {
            return $"https://{roomName}"; // fallback – should not really happen
        }

        var domain = _dailyDomainName.Trim();

        // Remove protocol if user accidentally included it in settings
        if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            domain = domain[7..];
        else if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            domain = domain[8..];

        // If config already contains .daily.co, don't append suffix again
        if (!domain.EndsWith(".daily.co", StringComparison.OrdinalIgnoreCase))
        {
            domain = $"{domain}.daily.co";
        }

        return $"https://{domain}/{roomName}";
    }

    /// <summary>
    /// Generate a Daily meeting token. Consultants are marked as owners so they can admit participants from the lobby.
    /// </summary>
    public async Task<string> GenerateMeetingTokenAsync(string roomName, Guid userId, string userName, bool isOwner = false)
    {
        try
        {
            var payload = new
            {
                properties = new
                {
                    room_name = roomName,
                    is_owner = isOwner,
                    user_id = userId.ToString(),
                    user_name = userName ?? "Guest"
                }
            };

            var resp = await _httpClient.PostAsync("meeting-tokens", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create meeting token. Status: {Status}, Error: {Error}", resp.StatusCode, error);
                return null;
            }

            var body = await resp.Content.ReadAsStringAsync();
            dynamic tokenObj = JsonConvert.DeserializeObject<dynamic>(body);
            return (string)tokenObj.token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate meeting token for {RoomName}", roomName);
            return null;
        }
    }
} 