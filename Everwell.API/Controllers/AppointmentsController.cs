using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Appointments;
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
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAllAppointments()
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
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<ActionResult<Appointment>> GetAppointmentById(Guid id)
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
    
    [HttpPost(ApiEndpointConstants.Appointment.CreateAppointmentEndpoint)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<ActionResult<Appointment>> CreateAppointment(CreateAppointmentRequest request)
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
} 