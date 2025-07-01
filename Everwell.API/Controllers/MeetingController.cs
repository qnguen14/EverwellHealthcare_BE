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

                if (callerId.HasValue)
                {
                    if (appointment.CustomerId != callerId && appointment.ConsultantId != callerId)
                    {
                        return Forbid("You don't have access to this appointment");
                    }
                }

                var currentTime = DateTime.UtcNow;
                var roomName = $"appointment-{appointmentId:N}".ToLower();
                
                // Get room info from Daily.co
                var roomInfo = await _dailyService.GetRoomInfoAsync(roomName);
                
                if (roomInfo == null)
                {
                    // Create room if it doesn't exist
                    roomInfo = await _dailyService.CreatePreScheduledRoomAsync(appointment);
                }

                // Public rooms do not require a meeting token
                string meetingToken = null;

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
                    CanJoinEarly = currentTime >= roomInfo.StartTime.AddMinutes(-5), // Can join 5 minutes early
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
                _logger.LogInformation("Received meeting join hook: {HookData}", hookData);
                
                // Process the webhook data as needed
                // This can be used for logging, analytics, notifications, etc.
                
                return Ok(new { message = "Hook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing meeting join hook");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
} 