using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using System.Security.Claims;

namespace Everwell.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IAgoraService _agoraService;
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(
            IAgoraService agoraService,
            IUnitOfWork<EverwellDbContext> unitOfWork,
            ILogger<MeetingController> logger)
        {
            _agoraService = agoraService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("channel/{channelName}/status")]
        [Authorize]
        public async Task<IActionResult> GetChannelStatus(string channelName)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var isActive = await _agoraService.IsChannelActiveAsync(channelName, currentTime);
                return Ok(new
                {
                    ChannelName = channelName,
                    IsActive = isActive,
                    CurrentTime = currentTime,
                    Message = isActive ? "Channel is available for joining" : "Channel is not available at this time"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking channel status for {ChannelName}", channelName);
                return StatusCode(500, "Error checking channel status");
            }
        }

        [HttpPost("join/{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> JoinMeeting(Guid appointmentId, [FromQuery] bool bypass = false, [FromQuery] Guid? userId = null)
        {
            try
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>().FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);
                if (appointment == null)
                {
                    return NotFound("Appointment not found");
                }

                if (!appointment.IsVirtual)
                {
                    return BadRequest(new { Message = "This is an in-person appointment." });
                }

                var currentTime = DateTime.UtcNow;
                var startTime = GetAppointmentStartTime(appointment);
                var endTime = GetAppointmentEndTime(appointment);

                if (!bypass)
                {
                    var availableFrom = startTime.AddMinutes(-5);
                    if (currentTime < availableFrom)
                    {
                        return BadRequest(new { Message = $"Meeting room will be available in {Math.Ceiling((availableFrom - currentTime).TotalMinutes)} minutes." });
                    }
                    if (currentTime > endTime)
                    {
                        return BadRequest(new { Message = "Meeting has ended." });
                    }
                }

                // Generate channel info for this specific appointment
                var channelInfo = await _agoraService.CreateChannelAsync(appointment);
                
                // If a specific user ID is provided, generate a consistent UID for that user
                uint userUid;
                if (userId.HasValue)
                {
                    userUid = GenerateConsistentUidForUser(userId.Value, appointmentId);
                }
                else
                {
                    // Generate a random unique UID for this session
                    userUid = GenerateRandomUid();
                }

                // Generate a new token for this specific user
                var userToken = await _agoraService.GenerateRtcTokenAsync(channelInfo.ChannelName, userUid, "publisher", endTime);

                return Ok(new
                {
                    channelInfo.ChannelName,
                    channelInfo.AppId,
                    RtcToken = userToken,  // User-specific token
                    Uid = userUid,         // User-specific UID
                    channelInfo.MeetingUrl,
                    channelInfo.StartTime,
                    channelInfo.EndTime,
                    Message = "You can now join the meeting"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining meeting for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, "Error joining meeting");
            }
        }

        // Generate a consistent UID for a specific user in a specific appointment
        private uint GenerateConsistentUidForUser(Guid userId, Guid appointmentId)
        {
            // Combine user ID and appointment ID to create a consistent UID
            var combinedString = $"{userId}-{appointmentId}";
            var hash = combinedString.GetHashCode();
            var uid = (uint)(Math.Abs(hash) % (int.MaxValue - 1)) + 1;
            
            _logger.LogInformation("Generated consistent UID {Uid} for user {UserId} in appointment {AppointmentId}", 
                uid, userId, appointmentId);
            return uid;
        }

        // Generate a random UID for anonymous users
        private uint GenerateRandomUid()
        {
            var random = new Random();
            var uid = (uint)random.Next(1, int.MaxValue);
            
            _logger.LogInformation("Generated random UID {Uid} for anonymous user", uid);
            return uid;
        }

        [HttpGet("appointment/{appointmentId}/meeting-info")]
        [Authorize]
        public async Task<IActionResult> GetMeetingInfo(Guid appointmentId, [FromQuery] Guid? userId = null)
        {
            try
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>().FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);
                if (appointment == null)
                {
                    return NotFound("Appointment not found");
                }
                if (!appointment.IsVirtual)
                {
                    return BadRequest(new { Message = "This is an in-person appointment." });
                }

                var currentTime = DateTime.UtcNow;
                var startTime = GetAppointmentStartTime(appointment);
                var endTime = GetAppointmentEndTime(appointment);
                var channelInfo = await _agoraService.CreateChannelAsync(appointment);
                // Determine UID for this user
                uint userUid;
                if (userId.HasValue)
                {
                    userUid = GenerateConsistentUidForUser(userId.Value, appointmentId);
                }
                else
                {
                    var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(claimUserId, out var parsedId))
                    {
                        userUid = GenerateConsistentUidForUser(parsedId, appointmentId);
                    }
                    else
                    {
                        userUid = GenerateRandomUid();
                    }
                }

                // Generate a user-specific RTC token
                var userToken = await _agoraService.GenerateRtcTokenAsync(channelInfo.ChannelName, userUid, "publisher", endTime);

                var isActive = await _agoraService.IsChannelActiveAsync(channelInfo.ChannelName, currentTime);
                var canJoin = currentTime >= startTime.AddMinutes(-5) && currentTime <= endTime;

                return Ok(new
                {
                    AppointmentId = appointmentId,
                    channelInfo.ChannelName,
                    channelInfo.AppId,
                    RtcToken = userToken, // user-specific token
                    Uid = userUid,        // user-specific UID
                    channelInfo.MeetingUrl,
                    StartTime = startTime,
                    EndTime = endTime,
                    CurrentTime = currentTime,
                    IsActive = isActive,
                    CanJoin = canJoin
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meeting info for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, "Error getting meeting information");
            }
        }

        [HttpGet("test-agora/join-now")]
        [AllowAnonymous]
        public async Task<IActionResult> TestAgoraJoinNow([FromQuery] string? userRole = "user1")
        {
            try
            {
                _logger.LogInformation("Testing Agora 'Join Now' service for user role: {UserRole}", userRole);
                var now = DateTime.UtcNow;

                // Create a temporary appointment that is active immediately
                var tempAppointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    AppointmentDate = DateOnly.FromDateTime(now),
                    Slot = ShiftSlot.Morning1, // This will be used to calculate a base time
                    IsVirtual = true,
                    // Mark this as a temporary appointment for the service to handle
                    Status = AppointmentStatus.Temp
                };

                // Generate channel info
                var channelInfo = await _agoraService.CreateChannelAsync(tempAppointment);

                // Generate a unique UID based on the user role (for testing multiple users)
                var userUid = GenerateTestUid(userRole, tempAppointment.Id);
                
                // Generate user-specific token
                var userToken = await _agoraService.GenerateRtcTokenAsync(channelInfo.ChannelName, userUid, "publisher", channelInfo.EndTime);

                return Ok(new
                {
                    Success = true,
                    Message = $"Generated a temporary meeting room for {userRole}. Use different userRole values (user1, user2, etc.) to test multiple participants.",
                    UserRole = userRole,
                    ChannelInfo = new
                    {
                        channelInfo.ChannelName,
                        channelInfo.AppId,
                        RtcToken = userToken,    // User-specific token
                        Uid = userUid,          // User-specific UID
                        channelInfo.MeetingUrl,
                        channelInfo.StartTime,
                        channelInfo.EndTime,
                        IsActive = await _agoraService.IsChannelActiveAsync(channelInfo.ChannelName, DateTime.UtcNow)
                    },
                    TestingInfo = new
                    {
                        Instructions = "To test multiple users, call this endpoint with different userRole parameters (e.g., ?userRole=user1, ?userRole=user2)",
                        Note = "Each user will get a unique UID and token, allowing them to see each other in the video call"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agora 'Join Now' service test failed");
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        // Generate a test UID based on user role and appointment
        private uint GenerateTestUid(string userRole, Guid appointmentId)
        {
            var combinedString = $"{userRole}-{appointmentId}";
            var hash = combinedString.GetHashCode();
            var uid = (uint)(Math.Abs(hash) % (int.MaxValue - 1)) + 1;
            
            _logger.LogInformation("Generated test UID {Uid} for user role {UserRole} in appointment {AppointmentId}", 
                uid, userRole, appointmentId);
            return uid;
        }

        [HttpGet("test-agora/{appointmentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> TestAgoraService( Guid appointmentId)
        {
            try
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>().FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);
                if (appointment == null)
                {
                    return NotFound("Appointment not found");
                }

                var channelInfo = await _agoraService.CreateChannelAsync(appointment);
                return Ok(new
                {
                    Success = true,
                    AppointmentId = appointmentId,
                    ChannelInfo = new
                    {
                        channelInfo.ChannelName,
                        channelInfo.AppId,
                        channelInfo.RtcToken,
                        channelInfo.MeetingUrl,
                        channelInfo.StartTime,
                        channelInfo.EndTime,
                        channelInfo.IsActive
                    },
                    Message = "Agora service test completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agora service test failed for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        private DateTime GetAppointmentStartTime(Appointment appointment)
        {
            var baseDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);
            var localTime = appointment.Slot switch
            {
                ShiftSlot.Morning1 => baseDate.AddHours(8),
                ShiftSlot.Morning2 => baseDate.AddHours(10),
                ShiftSlot.Afternoon1 => baseDate.AddHours(13),
                ShiftSlot.Afternoon2 => baseDate.AddHours(15),
                _ => baseDate.AddHours(8)
            };

            try
            {
                // Note: Ensure the TimeZone ID is correct for your server environment.
                // "SE Asia Standard Time" is for Windows. For Linux/macOS, it might be "Asia/Bangkok".
                var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeToUtc(localTime, targetTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback for environments where the timezone ID might not be available
                return DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
            }
        }

        private DateTime GetAppointmentEndTime(Appointment appointment)
        {
            var startTime = GetAppointmentStartTime(appointment);
            return startTime.AddHours(2); // Each slot is 2 hours
        }
    }
} 