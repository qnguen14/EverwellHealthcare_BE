using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Everwell.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgoraController : ControllerBase
{
    private readonly ILogger<AgoraController> _logger;
    private readonly IAgoraService _agoraService;
    private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;

    public AgoraController(
        ILogger<AgoraController> logger,
        IAgoraService agoraService,
        IUnitOfWork<EverwellDbContext> unitOfWork)
    {
        _logger = logger;
        _agoraService = agoraService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("test-channel")]
    [Authorize]
    public async Task<IActionResult> TestCreateChannel([FromQuery] string appointmentId = null)
    {
        try
        {
            // Create a test appointment if none provided
            var appointment = new Appointment
            {
                Id = !string.IsNullOrEmpty(appointmentId) && Guid.TryParse(appointmentId, out var parsedId) 
                    ? parsedId 
                    : Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                ConsultantId = Guid.NewGuid(),
                AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Slot = ShiftSlot.Morning1,
                Status = AppointmentStatus.Scheduled,
                IsVirtual = true,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Testing Agora channel creation for appointment {AppointmentId}", appointment.Id);

            var channelInfo = await _agoraService.CreateChannelAsync(appointment);

            var response = new
            {
                Success = true,
                Message = "Agora channel created successfully",
                AppointmentId = appointment.Id,
                ChannelInfo = new
                {
                    channelInfo.ChannelName,
                    channelInfo.ChannelUrl,
                    channelInfo.RtcToken,
                    channelInfo.StartTime,
                    channelInfo.EndTime,
                    channelInfo.IsActive,
                    channelInfo.IsEnabled,
                    channelInfo.MaxParticipants
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Agora channel creation");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Failed to create Agora channel", 
                Error = ex.Message 
            });
        }
    }

    [HttpPost("generate-token")]
    [Authorize]
    public async Task<IActionResult> TestGenerateToken(
        [FromQuery] string channelName = "test-channel",
        [FromQuery] uint userId = 12345)
    {
        try
        {
            _logger.LogInformation("Testing Agora token generation for channel {ChannelName}, user {UserId}", 
                channelName, userId);

            var token = await _agoraService.GenerateRtcTokenAsync(channelName, userId);

            var response = new
            {
                Success = true,
                Message = "Agora token generated successfully",
                ChannelName = channelName,
                UserId = userId,
                Token = token,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Agora token generation");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Failed to generate Agora token", 
                Error = ex.Message 
            });
        }
    }

    [HttpGet("test-real-appointment/{appointmentId}")]
    [Authorize]
    public async Task<IActionResult> TestRealAppointment(Guid appointmentId)
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
                return NotFound(new { Message = "Appointment not found" });
            }

            _logger.LogInformation("Testing Agora channel creation for real appointment {AppointmentId}", appointmentId);

            var channelInfo = await _agoraService.CreateChannelAsync(appointment);

            var response = new
            {
                Success = true,
                Message = "Agora channel created successfully for real appointment",
                AppointmentId = appointmentId,
                AppointmentDetails = new
                {
                    appointment.AppointmentDate,
                    appointment.Slot,
                    appointment.Status,
                    appointment.IsVirtual,
                    CustomerName = appointment.Customer?.Name,
                    ConsultantName = appointment.Consultant?.Name
                },
                ChannelInfo = new
                {
                    channelInfo.ChannelName,
                    channelInfo.ChannelUrl,
                    channelInfo.RtcToken,
                    channelInfo.StartTime,
                    channelInfo.EndTime,
                    channelInfo.IsActive,
                    channelInfo.IsEnabled,
                    channelInfo.MaxParticipants
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Agora channel creation for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { 
                Success = false, 
                Message = "Failed to create Agora channel for appointment", 
                Error = ex.Message 
            });
        }
    }
}
