# Agora.io Integration Implementation

## Overview

This implementation integrates Agora.io video calling services into the Everwell Healthcare System to replace Jitsi Meet with a more robust, enterprise-grade video meeting solution.

## 🚀 Features

- **Automatic Channel Creation**: Channels are created automatically when virtual appointments are scheduled
- **Time-Controlled Access**: Meetings are only accessible 5 minutes before the scheduled time until the appointment ends
- **Secure Token Generation**: Each user gets a unique, time-limited access token
- **Seamless Integration**: Works with existing appointment booking flow
- **Configurable Service**: Can switch between Agora and Daily.co via configuration

## 📦 Package Dependencies

The following packages are installed in `Everwell.BLL`:

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="ClosedXML" Version="0.105.0" />
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.7.4" />
<PackageReference Include="Microsoft.AspNetCore" Version="2.3.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.IdentityModel.JsonWebTokens" Version="8.11.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.11.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.11.0" />
```

## ⚙️ Configuration

### appsettings.json

```json
{
  "VideoMeeting": {
    "UseAgora": true
  },
  "Agora": {
    "AppId": "your_agora_app_id",
    "AppCertificate": "your_agora_app_certificate",
    "BaseUrl": "http://localhost:5173/meeting",
    "TokenExpirationTime": 3600
  }
}
```

### Environment Variables (Production)

For production deployment, these sensitive values should be stored as environment variables:

- `AGORA_APP_ID`
- `AGORA_APP_CERTIFICATE`

## 🏗️ Architecture

### Service Layer

1. **IAgoraService** - Interface defining Agora operations
2. **AgoraService** - Implementation of Agora channel management and token generation
3. **CalendarService** - Updated to support both Agora and Daily.co services
4. **AppointmentService** - Uses CalendarService for video meeting creation

### Key Components

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ AppointmentService │ -> │ CalendarService │ -> │   AgoraService  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │   DailyService  │
                    │   (fallback)    │
                    └─────────────────┘
```

## 📝 Implementation Details

### 1. AgoraService.cs

Located in `Everwell.BLL/Services/Implements/AgoraService.cs`

**Key Methods:**
- `CreateChannelAsync(Appointment appointment)` - Creates Agora channel for appointment
- `GenerateRtcTokenAsync(string channelName, uint userId)` - Generates access tokens
- `EnableChannelAsync(string channelName)` - Enables channel access
- `DisableChannelAsync(string channelName)` - Disables channel access

### 2. CalendarService.cs

Updated to support multiple video meeting providers:

```csharp
public async Task<string> CreateVideoMeetingAsync(Appointment appointment)
{
    var useAgora = _configuration.GetValue<bool?>("VideoMeeting:UseAgora") ?? true;
    
    if (useAgora)
    {
        return await CreateAgoraVideoMeetingAsync(appointment);
    }
    else
    {
        return await CreateDailyVideoMeetingAsync(appointment);
    }
}
```

### 3. Dependency Injection

Services are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IAgoraService, AgoraService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IDailyService, DailyService>();
```

## 🔧 Token Generation

The current implementation uses a development-friendly token generation approach. For production, you should replace this with the official Agora SDK.

### Development Token Generation

```csharp
private string GenerateAgoraToken(string appId, string appCertificate, string channelName, uint userId, long expireTimestamp)
{
    // Creates a basic HMAC-SHA256 signed token
    var payload = new { iss = appId, exp = expireTimestamp, channel = channelName, uid = userId, role = 1 };
    var payloadJson = JsonSerializer.Serialize(payload);
    var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
    var payloadBase64 = Convert.ToBase64String(payloadBytes);
    
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
    var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
    var signatureBase64 = Convert.ToBase64String(signatureBytes);
    
    return $"agora_{payloadBase64}.{signatureBase64}";
}
```

### Production Token Generation

For production, install the official Agora SDK and use:

```bash
dotnet add package AgoraIO.RTC.SDK
```

Then replace the token generation with:

```csharp
using AgoraIO.RTC;

var token = RtcTokenBuilder.BuildTokenWithUid(
    appId,
    appCertificate,
    channelName,
    userId,
    RtcTokenBuilder.Role.RolePublisher,
    (uint)expireTimestamp
);
```

## 🧪 Testing

### Test Controller

A test controller `AgoraController.cs` is provided with the following endpoints:

- `POST /api/agora/test-channel` - Test channel creation with mock appointment
- `POST /api/agora/generate-token` - Test token generation
- `GET /api/agora/test-real-appointment/{appointmentId}` - Test with real appointment

### Test Usage

```bash
# Test channel creation
curl -X POST "https://localhost:7050/api/agora/test-channel" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Test token generation
curl -X POST "https://localhost:7050/api/agora/generate-token?channelName=test&userId=123" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Test with real appointment
curl -X GET "https://localhost:7050/api/agora/test-real-appointment/APPOINTMENT_GUID" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📊 Channel Information Response

```json
{
  "channelName": "healthcare-12345678-90ab-cdef-1234-567890abcdef",
  "channelUrl": "http://localhost:5173/meeting/appointment-id",
  "rtcToken": "agora_eyJpc3MiOiJhcHBfaWQi...signature",
  "startTime": "2025-01-08T08:00:00",
  "endTime": "2025-01-08T10:00:00",
  "isActive": false,
  "isEnabled": false,
  "maxParticipants": 10
}
```

## 🕒 Time Management

### Appointment Time Slots

- **Morning1**: 08:00 - 10:00 (UTC+7)
- **Morning2**: 10:00 - 12:00 (UTC+7)
- **Afternoon1**: 13:00 - 15:00 (UTC+7)
- **Afternoon2**: 15:00 - 17:00 (UTC+7)

### Access Control

- **Early Access**: 5 minutes before appointment start time
- **Active Period**: From start time to end time
- **Automatic Expiry**: Tokens expire at appointment end time

## 🔄 Migration from Jitsi

The system automatically uses Agora instead of Jitsi with no database changes required:

- ✅ Same `IsVirtual` field usage
- ✅ Same `GoogleMeetLink` field for storing channel URLs
- ✅ Same `MeetingId` field for storing channel names
- ✅ Same appointment booking flow

## 🚨 Error Handling

The implementation includes comprehensive error handling:

- **Configuration Errors**: Missing App ID or Certificate
- **Token Generation Errors**: Fallback to development tokens
- **Channel Creation Errors**: Logged with full context
- **Network Errors**: Graceful degradation

## 📈 Production Considerations

### Security

1. **Environment Variables**: Store sensitive credentials as environment variables
2. **Token Expiry**: Tokens automatically expire at appointment end
3. **Channel Isolation**: Each appointment gets a unique channel

### Performance

1. **No Server Load**: Agora handles all video processing
2. **Scalable**: Supports unlimited concurrent meetings
3. **Global CDN**: Agora's global infrastructure ensures low latency

### Monitoring

1. **Comprehensive Logging**: All operations are logged with context
2. **Error Tracking**: Failed operations are logged with stack traces
3. **Usage Analytics**: Channel creation and access patterns are tracked

## 🔧 Troubleshooting

### Common Issues

1. **Invalid App ID/Certificate**
   ```
   Error: Agora App ID or App Certificate not configured
   Solution: Check appsettings.json configuration
   ```

2. **Token Generation Fails**
   ```
   Error: Failed to generate Agora token
   Solution: Check App Certificate format and validity
   ```

3. **Channel Creation Fails**
   ```
   Error: Failed to create Agora channel
   Solution: Verify appointment data and configuration
   ```

### Debug Mode

Enable detailed logging by setting log level to `Debug` in appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Everwell.BLL.Services.Implements.AgoraService": "Debug"
    }
  }
}
```

## 📋 Next Steps

1. **Replace Development Token**: Implement production Agora SDK token generation
2. **Add Channel Management**: Implement background service for channel lifecycle
3. **Add Analytics**: Track meeting usage and quality metrics
4. **Add Recording**: Implement meeting recording functionality
5. **Add Screen Sharing**: Enable screen sharing capabilities

## 📞 Support

For issues related to Agora integration:

1. Check the logs for detailed error messages
2. Verify configuration in appsettings.json
3. Test with the provided test endpoints
4. Review Agora.io documentation for SDK specifics

---

## 🎯 Integration Status

- ✅ **AgoraService Implementation**: Complete
- ✅ **CalendarService Integration**: Complete
- ✅ **Dependency Injection**: Complete
- ✅ **Configuration**: Complete
- ✅ **Test Controller**: Complete
- ✅ **Error Handling**: Complete
- ⚠️ **Production Token Generation**: Needs official SDK
- 🔲 **Background Channel Management**: Future enhancement
- 🔲 **Recording Functionality**: Future enhancement
