using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Responses.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet(ApiEndpointConstants.Appointment.GetAllAppointmentsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateAppointmentsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetAllAppointments()
    {
        try
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Appointment.GetAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateAppointmentsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });
            
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpGet(ApiEndpointConstants.Appointment.GetAppointmentsByConsultantEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetAppointmentConsultantResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetAppointmentsByConsultant(Guid id)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByConsultant(id);
            if (appointments == null || !appointments.Any())
                return NotFound(new { message = "No appointments found for this consultant" });
            
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPost(ApiEndpointConstants.Appointment.CreateAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateAppointmentsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentRequest request)
    {
        try
        {
            var appointment = await _appointmentService.CreateAppointmentAsync(request);
            if (appointment != null)
            {
                return NotFound(new { message = "Appointment is already booked" });
            }
            
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPut(ApiEndpointConstants.Appointment.UpdateAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateAppointmentsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Consultant")]
    public async Task<IActionResult> UpdateAppointment(Guid id, Appointment appointment)
    {
        try
        {
            var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, appointment);
            if (updatedAppointment == null)
                return NotFound(new { message = "Appointment not found" });
            
            return Ok(updatedAppointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.Appointment.DeleteAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DeleteAppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Consultant")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var response = await _appointmentService.DeleteAppointmentAsync(id);

        var apiResponse = new ApiResponse<DeleteAppointmentResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Project deleted successfully",
            IsSuccess = true,
            Data = response
        };

        return Ok(apiResponse);
    }
} 