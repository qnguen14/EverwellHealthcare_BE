using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Exceptions;
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
            var response = await _appointmentService.GetAllAppointmentsAsync();
            if (response == null || !response.Any())
            {
                return NotFound(new { message = "Không tìm thấy cuộc hẹn nào" });
            }
                

            var apiResponse = new ApiResponse<IEnumerable<CreateAppointmentsResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment retrieved successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Appointment.GetAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        try
        {
            var response = await _appointmentService.GetAppointmentByIdAsync(id);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại" });

            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment retrieved successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
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
            var response = await _appointmentService.GetAppointmentsByConsultant(id);
            if (response == null || !response.Any())
                return NotFound(new { message = "Không cuộc hẹn nào đã được đặt với tư vấn viên này." });

            var apiResponse = new ApiResponse<IEnumerable<GetAppointmentConsultantResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment retrieved successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPost(ApiEndpointConstants.Appointment.CreateAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentRequest request)
    {
        try
        {
            var response = await _appointmentService.CreateAppointmentAsync(request);
            if (response == null)
            {
                return NotFound(new { message = "Cuộc hẹn này đã được đặt với ngày và giờ này." });
            }

            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment created successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPut(ApiEndpointConstants.Appointment.UpdateAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Consultant")]
    public async Task<IActionResult> UpdateAppointment(Guid id, UpdateAppointmentRequest request)
    {
        try
        {
            var response = await _appointmentService.UpdateAppointmentAsync(id, request);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại." });

            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment updated successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Appointment.CancelAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Consultant,Customer")]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        try
        {
            var response = await _appointmentService.CancelAppoinemntAsync(id);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại." });
            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment cancelled successfully",
                IsSuccess = true,
                Data = response
            };
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.Appointment.DeleteAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<DeleteAppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Consultant")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var response = await _appointmentService.DeleteAppointmentAsync(id);

        var apiResponse = new ApiResponse<DeleteAppointmentResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Appointment deleted successfully",
            IsSuccess = true,
            Data = response
        };

        return Ok(apiResponse);
    }

    [HttpGet(ApiEndpointConstants.Appointment.GetConsultantSchedulesEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetScheduleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin, Staff, Consultant")]
    public async Task<IActionResult> GetConsultantSchedules()
    {
        try
        {
            var response = await _appointmentService.GetConsultantSchedules();
            if (response == null || !response.Any())
                return NotFound(new { message = "Không lịch nào tồn tại trong hệ thống." });

            var apiResponse = new ApiResponse<IEnumerable<GetScheduleResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment deleted successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Appointment.GetConsultantSchedulesByIdEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetScheduleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin, Staff, Consultant")]
    public async Task<IActionResult> GetConsultantSchedulesById(Guid id)
    {
        try
        {
            var response = await _appointmentService.GetConsultantSchedulesById(id);
            if (response == null || !response.Any())
                return NotFound(new { message = "Không lịch nào tồn tại trong hệ thống." });

            var apiResponse = new ApiResponse<IEnumerable<GetScheduleResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment deleted successfully",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost(ApiEndpointConstants.Appointment.CreateConsultantScheduleEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<GetScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin, Staff, Consultant")]
    public async Task<IActionResult> CreateConsultantSchedule(CreateScheduleRequest request)
    {
        try
        {
            var response = await _appointmentService.CreateConsultantSchedule(request);

            var apiResponse = new ApiResponse<GetScheduleResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Cuộc hẹn được tạo thành công",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = ex.Message,
                IsSuccess = false
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

}