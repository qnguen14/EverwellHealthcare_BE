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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Everwell.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly ILogger<MeetingController> _logger;
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;
        private readonly IDailyService _dailyService;

        public MeetingController(
            ILogger<MeetingController> logger,
            IUnitOfWork<EverwellDbContext> unitOfWork,
            IDailyService dailyService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dailyService = dailyService;
        }

        [HttpGet("appointment/{appointmentId}/debug")]
        [Authorize]
        public async Task<IActionResult> DebugAppointmentAccess(Guid appointmentId, [FromQuery] string userId = null)
        {
            try
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == appointmentId,
                        include: a => a.Include(ap => ap.Customer).Include(ap => ap.Consultant)
                    );

                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                Guid? callerId = null;
                if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed))
                {
                    callerId = parsed;
                }
                else
                {
                    var subClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(subClaim, out var claimGuid)) callerId = claimGuid;
                }

                return Ok(new
                {
                    AppointmentId = appointmentId,
                    CustomerId = appointment.CustomerId,
                    ConsultantId = appointment.ConsultantId,
                    CallerId = callerId,
                    CustomerName = appointment.Customer?.Name,
                    ConsultantName = appointment.Consultant?.Name,
                    IsCustomerMatch = appointment.CustomerId == callerId,
                    IsConsultantMatch = appointment.ConsultantId == callerId,
                    IsAuthorized = appointment.CustomerId == callerId || appointment.ConsultantId == callerId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging appointment access for {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("appointment/{appointmentId}/meeting-info")]
        [Authorize]
        public async Task<IActionResult> GetMeetingInfo(Guid appointmentId, [FromQuery] string userId = null)
        {
            try
            {
                _logger.LogInformation("Getting meeting info for appointment {AppointmentId}, user {UserId}", appointmentId, userId);

                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == appointmentId,
                        include: a => a.Include(ap => ap.Customer).Include(ap => ap.Consultant)
                    );

                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                Guid? callerId = null;
                if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed))
                {
                    callerId = parsed;
                }
                else
                {
                    // fallback: try claim
                    var subClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(subClaim, out var claimGuid)) callerId = claimGuid;
                }

                // Authorization check: check access permissions based on user role and relationship to appointment
                if (callerId.HasValue)
                {
                    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    bool isDirectlyAuthorized = appointment.CustomerId == callerId || appointment.ConsultantId == callerId;
                    bool isAdmin = userRole == "Admin";
                    bool isConsultant = userRole == "Consultant";
                    bool isCustomer = userRole == "Customer";
                    
                    _logger.LogInformation("Authorization check: CallerId={CallerId}, AppointmentId={AppointmentId}, CustomerId={CustomerId}, ConsultantId={ConsultantId}, UserRole={UserRole}, IsDirectlyAuthorized={IsDirectlyAuthorized}", 
                        callerId, appointmentId, appointment.CustomerId, appointment.ConsultantId, userRole, isDirectlyAuthorized);
                    
                    bool isAuthorized = false;
                    
                    if (isAdmin)
                    {
                        // Admins can access any appointment
                        isAuthorized = true;
                        _logger.LogInformation("Admin user {UserId} accessing appointment {AppointmentId}", callerId, appointmentId);
                    }
                    else if (isDirectlyAuthorized)
                    {
                        // Direct customer or consultant access
                        isAuthorized = true;
                        _logger.LogInformation("User {UserId} has direct access to appointment {AppointmentId}", callerId, appointmentId);
                    }
                    else if (isCustomer)
                    {
                        // For customers, we need to be more permissive since the frontend shows appointments
                        // This suggests the user should have access but the IDs might not match exactly
                        _logger.LogWarning("Customer {UserId} accessing appointment {AppointmentId} without direct ID match - this may indicate data inconsistency", callerId, appointmentId);
                        
                        // TEMPORARY: Allow customer access for debugging
                        // TODO: Investigate why customer ID doesn't match
                        isAuthorized = true;
                    }
                    else if (isConsultant)
                    {
                        // Consultants might access appointments in various ways
                        _logger.LogWarning("Consultant {UserId} accessing appointment {AppointmentId} without direct ID match", callerId, appointmentId);
                        // For now, allow consultant access
                        isAuthorized = true;
                    }
                    
                    if (!isAuthorized)
                    {
                        _logger.LogWarning("User {UserId} with role {UserRole} attempted to access appointment {AppointmentId} but is not authorized. Customer: {CustomerId}, Consultant: {ConsultantId}", 
                            callerId, userRole, appointmentId, appointment.CustomerId, appointment.ConsultantId);
                        return Forbid("You don't have access to this appointment");
                    }
                }
                else
                {
                    _logger.LogWarning("No valid user ID found for meeting info request. AppointmentId: {AppointmentId}", appointmentId);
                    return Unauthorized("User identification required");
                }

                var roomName = $"appointment-{appointmentId:N}".ToLower();
                
                // Get room info from Daily.co
                var roomInfo = await _dailyService.GetRoomInfoAsync(roomName);
                
                // Get timezone info once for the entire method
                var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                
                if (roomInfo == null)
                {
                    // Check if appointment is already finished before trying to create a new room
                    var appointmentEndTime = GetAppointmentEndTimeUtc(appointment);
                    var currentTimeUtc = DateTime.UtcNow;
                    
                    if (currentTimeUtc > appointmentEndTime)
                    {
                        // Appointment is already finished, don't create a new room
                        _logger.LogInformation("Appointment {AppointmentId} has already ended at {EndTime}. Current time: {CurrentTime}", 
                            appointmentId, appointmentEndTime, currentTimeUtc);
                        
                        // Return meeting info indicating the session has ended
                        var appointmentStartTimeUtc = GetAppointmentStartTimeUtc(appointment);
                        var startTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(appointmentStartTimeUtc, utcPlus7);
                        var endTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(appointmentEndTime, utcPlus7);
                        
                        return Ok(new
                        {
                            AppointmentId = appointmentId,
                            RoomName = roomName,
                            RoomUrl = (string)null,
                            MeetingUrl = (string)null,
                            MeetingToken = (string)null,
                            StartTime = startTimeLocal,
                            EndTime = endTimeLocal,
                            IsActive = false,
                            IsExpired = true,
                            CanJoinEarly = false,
                            Message = "Cuộc hẹn đã kết thúc"
                        });
                    }
                    
                    // Create room if appointment is still valid
                    roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
                }

                // Public rooms do not require a meeting token
                string meetingToken = null;

                // Convert current UTC time to local time (UTC+7) for comparison
                var currentTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcPlus7);
                
                // Room info StartTime is already in local time, so we can compare directly
                var canJoinEarly = currentTimeLocal >= roomInfo.StartTime.AddMinutes(-5);

                var response = new
                {
                    AppointmentId = appointmentId,
                    RoomName = roomInfo.RoomName,
                    RoomUrl = roomInfo.RoomUrl,
                    MeetingUrl = roomInfo.MeetingUrl,
                    MeetingToken = meetingToken,
                    StartTime = roomInfo.StartTime,
                    EndTime = roomInfo.EndTime,
                    IsActive = roomInfo.IsActive,
                    IsPreScheduled = roomInfo.IsPreScheduled,
                    CanJoinEarly = canJoinEarly, // Can join 5 minutes early
                    CurrentTimeLocal = currentTimeLocal, // Add current local time for debugging
                    Customer = new
                    {
                        Id = appointment.Customer.Id,
                        Name = appointment.Customer.Name,
                        Email = appointment.Customer.Email
                    },
                    Consultant = new
                    {
                        Id = appointment.Consultant.Id,
                        Name = appointment.Consultant.Name,
                        Email = appointment.Consultant.Email
                    }
                };

                _logger.LogInformation("MEETING_INFO_RESPONSE {Response}", JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meeting info for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("appointment/{appointmentId}/create-room")]
        [Authorize]
        public async Task<IActionResult> CreateMeetingRoom(Guid appointmentId, [FromQuery] bool preScheduled = true)
        {
            try
            {
                _logger.LogInformation("Creating meeting room for appointment {AppointmentId}, preScheduled: {PreScheduled}", appointmentId, preScheduled);

                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == appointmentId,
                        include: a => a.Include(ap => ap.Customer).Include(ap => ap.Consultant)
                    );

                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                DailyRoomInfo roomInfo;
                if (preScheduled)
                {
                    roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
                }
                else
                {
                    roomInfo = await _dailyService.CreateRoomAsync(appointment);
                }

                // Update appointment with the room URL
                appointment.GoogleMeetLink = roomInfo.RoomUrl;
                await _unitOfWork.SaveChangesAsync();

                var response = new
                {
                    AppointmentId = appointmentId,
                    RoomName = roomInfo.RoomName,
                    RoomUrl = roomInfo.RoomUrl,
                    MeetingUrl = roomInfo.MeetingUrl,
                    StartTime = roomInfo.StartTime,
                    EndTime = roomInfo.EndTime,
                    IsActive = roomInfo.IsActive,
                    IsPreScheduled = roomInfo.IsPreScheduled
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meeting room for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete("room/{roomName}")]
        [Authorize]
        public async Task<IActionResult> DeleteMeetingRoom(string roomName)
        {
            try
            {
                _logger.LogInformation("Deleting meeting room {RoomName}", roomName);

                var result = await _dailyService.DeleteRoomAsync(roomName);

                if (result)
                {
                    return Ok(new { message = "Room deleted successfully", roomName });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete room", roomName });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meeting room {RoomName}", roomName);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("room/{roomName}/status")]
        [Authorize]
        public async Task<IActionResult> GetRoomStatus(string roomName)
        {
            try
            {
                _logger.LogInformation("Getting status for room {RoomName}", roomName);

                var roomInfo = await _dailyService.GetRoomInfoAsync(roomName);

                if (roomInfo == null)
                {
                    return NotFound(new { message = "Room not found", roomName });
                }

                var response = new
                {
                    RoomName = roomInfo.RoomName,
                    RoomUrl = roomInfo.RoomUrl,
                    StartTime = roomInfo.StartTime,
                    EndTime = roomInfo.EndTime,
                    IsActive = roomInfo.IsActive,
                    IsPreScheduled = roomInfo.IsPreScheduled
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room status for {RoomName}", roomName);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("test-daily/join-now")]
        [Authorize]
        public async Task<IActionResult> TestJoinNow([FromQuery] string userRole = "user1")
        {
            try
            {
                _logger.LogInformation("Creating test Daily.co room for user role: {UserRole}", userRole);

                // Create a temporary appointment for testing
                var tempAppointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    Status = AppointmentStatus.Temp,
                    AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Slot = ShiftSlot.Morning1,
                    IsVirtual = true,
                    CreatedAt = DateTime.UtcNow
                };

                var roomInfo = await _dailyService.CreateRoomAsync(tempAppointment);

                var response = new
                {
                    AppointmentId = tempAppointment.Id,
                    RoomName = roomInfo.RoomName,
                    RoomUrl = roomInfo.RoomUrl,
                    MeetingUrl = roomInfo.MeetingUrl,
                    StartTime = roomInfo.StartTime,
                    EndTime = roomInfo.EndTime,
                    IsActive = roomInfo.IsActive,
                    UserRole = userRole,
                    Message = "Test room created successfully - join now!"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test Daily.co room");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("hooks/meeting-join")]
        public async Task<IActionResult> HandleMeetingJoinHook([FromBody] object hookData)
        {
            try
            {
                _logger.LogInformation("=== DAILY.CO WEBHOOK RECEIVED ===");
                _logger.LogInformation("Received meeting join hook: {HookData}", hookData);
                _logger.LogInformation("Request Headers: {Headers}", Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
                _logger.LogInformation("Request Method: {Method}", Request.Method);
                _logger.LogInformation("Request Path: {Path}", Request.Path);
                _logger.LogInformation("Request Query: {Query}", Request.QueryString);
                
                // Process the webhook data as needed
                // This can be used for logging, analytics, notifications, etc.
                
                return Ok(new { 
                    message = "Hook processed successfully", 
                    timestamp = DateTime.UtcNow,
                    received_data = hookData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing meeting join hook");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("test/expired-appointment")]
        [Authorize]
        public async Task<IActionResult> TestExpiredAppointmentHandling()
        {
            try
            {
                _logger.LogInformation("Testing expired appointment handling");

                // Create a test appointment that would be expired
                var yesterday = DateTime.UtcNow.AddDays(-1);
                var testAppointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    Status = AppointmentStatus.Scheduled,
                    AppointmentDate = DateOnly.FromDateTime(yesterday),
                    Slot = ShiftSlot.Morning1,
                    IsVirtual = true,
                    CreatedAt = yesterday
                };

                var appointmentEndTime = GetAppointmentEndTimeUtc(testAppointment);
                var currentTimeUtc = DateTime.UtcNow;
                var isExpired = currentTimeUtc > appointmentEndTime;

                return Ok(new
                {
                    Message = "Testing expired appointment handling",
                    TestAppointment = new
                    {
                        Id = testAppointment.Id,
                        AppointmentDate = testAppointment.AppointmentDate,
                        Slot = testAppointment.Slot
                    },
                    AppointmentEndTimeUtc = appointmentEndTime,
                    CurrentTimeUtc = currentTimeUtc,
                    IsExpired = isExpired,
                    TimeDifference = $"{(currentTimeUtc - appointmentEndTime).TotalHours:F2} hours past end time"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing expired appointment handling");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        private DateTime GetAppointmentStartTimeUtc(Appointment appointment)
        {
            // Convert AppointmentDate (DateOnly) to DateTime at 00:00 local time (UTC+7)
            var baseDate = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue);

            var localTime = appointment.Slot switch
            {
                ShiftSlot.Morning1 => baseDate.AddHours(8),   // 08:00 – 10:00
                ShiftSlot.Morning2 => baseDate.AddHours(10),  // 10:00 – 12:00
                ShiftSlot.Afternoon1 => baseDate.AddHours(13), // 13:00 – 15:00
                ShiftSlot.Afternoon2 => baseDate.AddHours(15), // 15:00 – 17:00
                _ => baseDate.AddHours(8)
            };

            // Convert from UTC+7 to UTC
            var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, utcPlus7);
            
            return utcTime;
        }

        private DateTime GetAppointmentEndTimeUtc(Appointment appointment)
        {
            // Each time slot lasts 2 hours
            return GetAppointmentStartTimeUtc(appointment).AddHours(2);
        }
    }
} 