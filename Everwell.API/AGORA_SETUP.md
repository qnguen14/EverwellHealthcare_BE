# Daily.co Integration with Time Controls

## 📋 Overview

This implementation uses Daily.co to provide precise time-controlled video meetings for appointments. The system:

- ✅ **Auto-starts meetings** 5 minutes before appointment time
- ✅ **Auto-ends meetings** exactly when the slot ends  
- ✅ **Restricts access** outside of scheduled times
- ✅ **Direct meeting links** for immediate access to Daily.co rooms
- ✅ **Automatic room management** handles room lifecycle

## 🚀 Setup Instructions

### 1. Get Daily.co Credentials

1. **Sign up** at [Daily.co Dashboard](https://dashboard.daily.co/)
2. **Create a domain** in the dashboard
3. **Get your credentials**:
   - API Key (required)
   - Domain Name (required)

### 2. Update Configuration

Update your `appsettings.json`:

```json
{
  "Daily": {
    "ApiKey": "YOUR_DAILY_API_KEY",
    "DomainName": "yourdomain.daily.co",
    "BaseUrl": "http://localhost:5173/meeting"
  }
}
```

**Note:** With the latest update, `MeetingUrl` now returns direct Daily.co room links instead of frontend wrapper URLs for immediate access.

### 3. Direct Meeting Links

The system now provides two types of URLs:

- **RoomUrl**: Direct Daily.co room link (e.g., `https://yourdomain.daily.co/room-name`)
- **MeetingUrl**: Now returns the same direct Daily.co room link for immediate access

This eliminates the need to go through a frontend meeting room interface and provides instant access to video meetings.

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
1. **Appointment created** → Daily.co room scheduled
2. **Room management** handles appointment times
3. **5 minutes before** → Room becomes accessible
4. **After end time** → Room access expires
5. **API access control** → Validates time before providing room URL

### Frontend Flow
1. User clicks **meeting link** from appointment
2. **Real-time status** shows availability
3. **Time restrictions** prevent early/late access
4. **"Join Meeting"** button only works during valid time
5. **Automatic updates** every 30 seconds

## 🛠️ Customization

### Modify Time Windows

In `DailyService.cs`:

```csharp
// Change early access (currently 5 minutes before)
var roomStartTime = startTime.AddMinutes(-5);

// Room expiration time
exp = ((DateTimeOffset)endTime).ToUnixTimeSeconds()
```

### Modify Slot Duration

In `DailyService.cs`:

```csharp
private DateTime GetAppointmentEndTime(Appointment appointment)
{
    var startTime = GetAppointmentStartTime(appointment);
    return startTime.AddHours(2); // Change from 2 hours to your preferred duration
}
```

### Custom Meeting URL

Update the `BaseUrl` in configuration to point to your custom meeting interface.

## 🔄 Direct Meeting Links Update

The system now provides direct Daily.co room links instead of frontend wrapper URLs:

- **MeetingUrl** now returns direct Daily.co room links
- **Faster access** - no intermediate pages
- **Same API endpoints** - no breaking changes
- **Backward compatible** - existing integrations continue to work

## 🧪 Testing

1. **Create virtual appointment** for current time slot
2. **Get meeting info** via API endpoint
3. **Before 5 minutes** → MeetingUrl will be null or restricted
4. **At 5 minutes before** → MeetingUrl returns direct Daily.co link
5. **After end time** → Room access expires automatically

## 📱 Production Considerations

1. **Load balancing**: Daily.co handles this automatically
2. **Scaling**: No server-side video processing needed
3. **Security**: Rooms auto-expire at appointment end
4. **Monitoring**: Logs all room activities
5. **Cost**: Pay only for active meeting minutes

## 🎉 Benefits of Direct Meeting Links

- ✅ **Instant access** - no intermediate pages
- ✅ **Professional reliability** with Daily.co platform
- ✅ **Automatic room management** with time controls
- ✅ **Better user experience** - direct to video call
- ✅ **Simplified flow** - fewer clicks to join

Your appointments now have **direct access** with **exact time control** - users get immediate access to Daily.co rooms during their scheduled slot! 🎯