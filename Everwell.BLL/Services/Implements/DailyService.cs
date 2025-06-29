using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Linq;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class DailyService : IDailyService
{
    private readonly IConfiguration _config;
    private readonly ILogger<DailyService> _logger;
    private readonly IUnitOfWork<Everwell.DAL.Data.Entities.EverwellDbContext> _uow;
    private readonly IHttpClientFactory _httpClientFactory;

    public DailyService(IConfiguration config, ILogger<DailyService> logger, IUnitOfWork<Everwell.DAL.Data.Entities.EverwellDbContext> uow, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _uow = uow;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> EnsureRoomAsync(Appointment appointment)
    {
        if (!string.IsNullOrWhiteSpace(appointment.GoogleMeetLink)) // reuse field for meeting url
        {
            return appointment.GoogleMeetLink;
        }

        // Build Daily request
        var apiKey = _config["Daily:ApiKey"];
        var domain = _config["Daily:Domain"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(domain))
            throw new InvalidOperationException("Daily configuration is missing (Daily:ApiKey, Daily:Domain)");

        // Create a valid room name (Daily.co allows alphanumeric, dash, underscore)
        var roomName = $"appointment-{appointment.Id.ToString().Replace("-", "")}";

        var start = GetStartDateTime(appointment).AddMinutes(-5); // allow early entry 5 min before
        var end = start.AddHours(2); // assume 2h duration

        // Build payload with correct structure for Daily.co API
        var payload = new
        {
            name = roomName,
            properties = new
            {
                nbf = new DateTimeOffset(start).ToUnixTimeSeconds(),
                exp = new DateTimeOffset(end).ToUnixTimeSeconds(),
                max_participants = 2
            }
        };

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.daily.co/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var jsonPayload = JsonSerializer.Serialize(payload);
        _logger.LogInformation("Creating Daily.co room with payload: {Payload}", jsonPayload);
        
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("rooms", content);
        
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            _logger.LogError("Daily.co API error: {StatusCode} - {ErrorContent}", resp.StatusCode, errorContent);
            throw new InvalidOperationException($"Daily.co API error: {resp.StatusCode} - {errorContent}");
        }
        
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var url = doc.RootElement.GetProperty("url").GetString();

        // Only persist to database if this is not a temporary appointment
        if (appointment.Status != AppointmentStatus.Temp)
        {
            // persist
            appointment.MeetingId = roomName;
            appointment.GoogleMeetLink = url;
            _uow.GetRepository<Appointment>().UpdateAsync(appointment);
            await _uow.SaveChangesAsync();
        }

        _logger.LogInformation("Created Daily room {Room} for appointment {Id} with URL: {Url}", roomName, appointment.Id, url);
        return url;
    }

    private static DateTime GetStartDateTime(Appointment a)
    {
        // Handle temporary appointments (for testing)
        if (a.Status == AppointmentStatus.Temp)
        {
            return DateTime.UtcNow; // Return current UTC time for immediate availability
        }

        var date = a.AppointmentDate.ToDateTime(TimeOnly.MinValue).Date;
        var time = a.Slot switch
        {
            ShiftSlot.Morning1 => new TimeSpan(8, 0, 0),
            ShiftSlot.Morning2 => new TimeSpan(10, 0, 0),
            ShiftSlot.Afternoon1 => new TimeSpan(13, 0, 0),
            _ => new TimeSpan(15, 0, 0)
        };
        
        var localDateTime = date + time;
        
        try
        {
            // Convert local time to UTC for proper scheduling
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, targetTimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback: assume the time is already in UTC
            return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
        }
    }

    // ------------------------------------------------------------
    // Optional helpers to keep existing controller endpoints happy
    // ------------------------------------------------------------

    /// <summary>
    /// Very lightweight check — considers a room active if the current time
    /// is between its scheduled start and exp timestamps.
    /// Since we name the room <c>appointment_{id}</c>, we can look up the
    /// appointment to get those slots.
    /// </summary>
    public async Task<bool> IsChannelActiveAsync(string roomIdentifier, DateTime nowUtc)
    {
        // roomIdentifier could be a full URL or just the room name
        var name = roomIdentifier.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? new Uri(roomIdentifier).Segments.Last().Trim('/')
            : roomIdentifier;

        if (!name.StartsWith("appointment_", StringComparison.OrdinalIgnoreCase))
            return true; // fallback, assume active

        if (!Guid.TryParse(name.Replace("appointment_", ""), out var appointmentId))
            return true;

        var appointment = await _uow.GetRepository<Appointment>().FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);
        if (appointment == null) return false;

        var start = GetStartDateTime(appointment).AddMinutes(-5);
        var end = start.AddHours(2);
        return nowUtc >= start && nowUtc <= end;
    }

    /// <summary>
    /// Daily rooms can be joined by URL alone, so we just return an empty string.
    /// </summary>
    public Task<string> GenerateRtcTokenAsync(string meetingUrl, uint uid, string role, DateTime expireAtUtc)
        => Task.FromResult(string.Empty);
} 