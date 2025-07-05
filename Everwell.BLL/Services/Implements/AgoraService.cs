using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Everwell.BLL.Services.Implements;

public class AgoraService : IAgoraService
{
    private readonly IConfiguration _configuration;

    public AgoraService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string BuildChannelName(Appointment appointment)
    {
        return $"appointment-{appointment.Id:N}".ToLower();
    }

    public string GenerateRtcToken(string channelName, uint uid, bool isHost)
    {
        var appId = _configuration["Agora:AppId"] ?? throw new InvalidOperationException("Agora:AppId is not configured");
        var appCert = _configuration["Agora:AppCert"] ?? throw new InvalidOperationException("Agora:AppCert is not configured");
        var expireSecStr = _configuration["Agora:ExpireSec"];
        if (!int.TryParse(expireSecStr, out var expireSec)) expireSec = 3600;

        var role = isHost ? 1 : 2; // 1 = Publisher, 2 = Subscriber
        var expireTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expireSec;

        return BuildAgoraToken(appId, appCert, channelName, uid, role, (uint)expireTimestamp);
    }

    public string GetAppId()
    {
        return _configuration["Agora:AppId"] ?? string.Empty;
    }

    private static string BuildAgoraToken(string appId, string appCert, string channelName, uint uid, int role, uint expireTimestamp)
    {
        // Agora Token Algorithm implementation
        var version = "007";
        var randomInt = (uint)Random.Shared.Next();
        
        var message = $"{appId}{uid}{channelName}{role}{expireTimestamp}{randomInt}";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCert));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        var signatureHex = Convert.ToHexString(signature).ToLower();
        
        var content = $"{version}{appId}{randomInt:x8}{expireTimestamp:x8}{role:x8}{uid:x8}{signatureHex}";
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        
        return token;
    }
} 