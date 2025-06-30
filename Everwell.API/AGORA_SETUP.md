# Agora.io Integration with Time Controls

## 📋 Overview

This implementation replaces Jitsi Meet with Agora.io to provide precise time-controlled video meetings for appointments. The system:

- ✅ **Auto-starts meetings** 5 minutes before appointment time
- ✅ **Auto-ends meetings** exactly when the slot ends  
- ✅ **Restricts access** outside of scheduled times
- ✅ **Background service** manages channel lifecycle

## 🚀 Setup Instructions

### 1. Get Agora.io Credentials

1. **Sign up** at [Agora.io Console](https://console.agora.io/)
2. **Create a project** in the console
3. **Get your credentials**:
   - App ID (required)
   - App Certificate (required for production)

### 2. Update Configuration

Update your `appsettings.json`:

```json
{
  "Agora": {
    "AppId": "YOUR_ACTUAL_AGORA_APP_ID",
    "AppCertificate": "YOUR_ACTUAL_AGORA_APP_CERTIFICATE", 
    "BaseUrl": "https://yourdomain.com/meeting"
  }
}
```

### 3. Install Agora NuGet Package (Optional - For Production)

For production, consider using the official Agora RTC token library:

```bash
dotnet add package Agora.RTC.Token
```

Then update `AgoraService.cs` to use the official token generation:

```csharp
using AgoraToken;

public async Task<string> GenerateRtcTokenAsync(string channelName, uint uid, string role, DateTime validUntil)
{
    var timestamp = ((DateTimeOffset)validUntil).ToUnixTimeSeconds();
    var privilegeExpired = (uint)timestamp;
    
    return RtcTokenBuilder.BuildTokenWithUid(_appId, _appCertificate, channelName, uid, 
        RtcTokenBuilder.Role.RolePublisher, privilegeExpired);
}
```

### 4. Frontend Meeting Interface

The meeting page is available at: `/meeting/{appointmentId}`

**Features:**
- Real-time countdown to meeting start/end
- Visual status indicators (waiting/active/ended)
- Automatic access control
- Meeting instructions

### 5. Time Control Logic

**Meeting Availability:**
- **Available from:** 5 minutes before appointment start
- **Active until:** Exact appointment end time  
- **Auto-management:** Background service handles enable/disable

**Appointment Slots:**
- `Morning1`: 8:00 AM - 10:00 AM
- `Morning2`: 10:00 AM - 12:00 PM  
- `Afternoon1`: 1:00 PM - 3:00 PM
- `Afternoon2`: 3:00 PM - 5:00 PM

## 🔗 API Endpoints

### Check Meeting Status
```
GET /api/meeting/appointment/{appointmentId}/meeting-info
```

### Join Meeting (Time-Controlled)
```
POST /api/meeting/join/{appointmentId}
```

### Check Channel Status
```
GET /api/meeting/channel/{channelName}/status
```

## 🎯 How It Works

### Backend Flow
1. **Appointment created** → Agora channel scheduled
2. **Background service** monitors appointment times
3. **5 minutes before** → Channel enabled automatically
4. **After end time** → Channel disabled automatically
5. **API access control** → Validates time before allowing join

### Frontend Flow
1. User clicks **meeting link** from appointment
2. **Real-time status** shows availability
3. **Time restrictions** prevent early/late access
4. **"Join Meeting"** button only works during valid time
5. **Automatic updates** every 30 seconds

## 🛠️ Customization

### Modify Time Windows

In `AgoraChannelManagementService.cs`:

```csharp
// Change early access (currently 5 minutes before)
if (currentTime >= startTime.AddMinutes(-5) && currentTime < startTime.AddMinutes(5))

// Change late access (currently disabled at end time)
if (currentTime >= endTime)
```

### Modify Slot Duration

In `AgoraService.cs`:

```csharp
private DateTime GetAppointmentEndTime(Appointment appointment)
{
    var startTime = GetAppointmentStartTime(appointment);
    return startTime.AddHours(2); // Change from 2 hours to your preferred duration
}
```

### Custom Meeting URL

Update the `BaseUrl` in configuration to point to your custom meeting interface.

## 🔄 Migration from Jitsi

The system automatically uses Agora instead of Jitsi. No database changes required since:

- Same `IsVirtual` field usage
- Same `MeetingId` field for storing channel names
- Same appointment booking flow

## 🧪 Testing

1. **Create virtual appointment** for current time slot
2. **Wait 5 minutes before** start time
3. **Visit meeting page** → Should show "waiting" status  
4. **At 5 minutes before** → Should show "Join Meeting" button
5. **After end time** → Should show "Meeting ended"

## 📱 Production Considerations

1. **Load balancing**: Agora handles this automatically
2. **Scaling**: No server-side video processing needed
3. **Security**: Tokens auto-expire at appointment end
4. **Monitoring**: Logs all channel activities
5. **Cost**: Pay only for active meeting minutes

## 🎉 Benefits vs Jitsi

- ✅ **Precise timing control** vs manual access
- ✅ **Professional reliability** vs community platform  
- ✅ **Automatic management** vs manual room creation
- ✅ **Better mobile support** vs browser-only
- ✅ **Analytics & monitoring** vs limited insights

Your appointments now have **exact time control** - users can only join during their scheduled slot! 🎯 