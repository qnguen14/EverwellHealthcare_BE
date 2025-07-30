// ============================================================================
// APPOINTMENTS CONTROLLER
// ============================================================================
// This controller manages the complete appointment lifecycle in the healthcare system
// It handles appointment booking, scheduling, updates, cancellations, and meeting management
// 
// APPOINTMENT FLOW OVERVIEW:
// 1. BOOKING: Customer selects consultant and available time slot
// 2. SCHEDULING: System creates appointment with consultant's schedule
// 3. CONFIRMATION: Both parties receive notifications
// 4. MEETING SETUP: Daily.co room created for video consultation
// 5. CHECK-IN/OUT: Attendance tracking for billing and records
// 6. COMPLETION: Feedback collection and payment processing
// 
// KEY BUSINESS RULES:
// - Only authenticated users can book appointments
// - Consultants manage their own schedules
// - Admins have full appointment management access
// - Meeting links are generated automatically
// - Check-in required for session billing

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

    /// <summary>
    /// GET ALL APPOINTMENTS
    /// ====================
    /// Retrieves all appointments in the system with role-based filtering
    /// 
    /// ACCESS CONTROL:
    /// - Admin: Can view all appointments system-wide
    /// - Consultant: Can view their own appointments only
    /// - Customer: Can view their own appointments only
    /// 
    /// BUSINESS LOGIC:
    /// 1. Service layer applies role-based filtering
    /// 2. Returns appointments with related data (Customer, Consultant info)
    /// 3. Includes appointment status, meeting links, and scheduling details
    /// 
    /// RESPONSE DATA:
    /// - Appointment details (date, time, status)
    /// - Customer information (name, contact)
    /// - Consultant information (name, specialization)
    /// - Meeting link (if generated)
    /// - Check-in/out status
    /// </summary>
    [HttpGet(ApiEndpointConstants.Appointment.GetAllAppointmentsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateAppointmentsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")] // Role-based access control
    public async Task<IActionResult> GetAllAppointments()
    {
        try
        {
            // Service applies role-based filtering based on JWT claims
            // - Customers see only their appointments
            // - Consultants see only their appointments
            // - Admins see all appointments
            var response = await _appointmentService.GetAllAppointmentsAsync();
            if (response == null || !response.Any())
            {
                return NotFound(new { message = "Không tìm thấy cuộc hẹn nào" }); // "No appointments found"
            }
                
            var apiResponse = new ApiResponse<IEnumerable<CreateAppointmentsResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment retrieved successfully",
                IsSuccess = true,
                Data = response // Contains filtered appointment list with related entities
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// GET APPOINTMENT BY ID
    /// =====================
    /// Retrieves specific appointment details with authorization validation
    /// 
    /// AUTHORIZATION FLOW:
    /// 1. Validate user has access to this specific appointment
    /// 2. Customer can only access their own appointments
    /// 3. Consultant can only access appointments they're assigned to
    /// 4. Admin can access any appointment
    /// 
    /// USE CASES:
    /// - View appointment details before meeting
    /// - Access meeting link and room information
    /// - Check appointment status and timing
    /// - Review participant information
    /// 
    /// SECURITY:
    /// - ID-based access control prevents unauthorized viewing
    /// - Service layer validates user relationship to appointment
    /// </summary>
    [HttpGet(ApiEndpointConstants.Appointment.GetAppointmentEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        try
        {
            // Service validates user has access to this specific appointment
            // Uses JWT claims to check user ID against appointment participants
            var response = await _appointmentService.GetAppointmentByIdAsync(id);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại" }); // "Appointment does not exist"

            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Appointment retrieved successfully",
                IsSuccess = true,
                Data = response // Contains complete appointment details with participants
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
                return NotFound(new { message = "Cuộc hẹn này đã được đặt hoặc ngày chọn không hợp lệ." });
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
    
    [HttpPut(ApiEndpointConstants.Appointment.UpdateMeetingLinkEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles =  "Admin,Consultant")]
    public async Task<IActionResult> UpdateMeetingLink(Guid id, string meetingLink)
    {
        try
        {
            var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (existingAppointment == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại." });
            
            var response = await _appointmentService.UpdateMeetingLinkAsync(id, meetingLink);
            if (response == null)
                return NotFound(new { message = $"Đường dẫn cuộc hợp không hợp lệ: {meetingLink}" });

            var apiResponse = new ApiResponse<CreateAppointmentsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Cuộc họp được cập nhật đường dẫn thành công.",
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
    
    // ---------------- Check-in / Check-out ----------------

    [HttpPut(ApiEndpointConstants.Appointment.MarkCheckInEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin, Customer, Consultant")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        try
        {
            var response = await _appointmentService.MarkCheckInAsync(id);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại." });

            var apiResponse = new ApiResponse<CheckInResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Check-in thành công",
                IsSuccess = true,
                Data = response
            };

            return Ok(apiResponse);
            
        } catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Appointment.MarkCheckOutEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CheckOutResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin, Customer, Consultant")]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        try
        {
            var response = await _appointmentService.MarkCheckOutAsync(id);
            if (response == null)
                return NotFound(new { message = "Cuộc hẹn không tồn tại." });
            var apiResponse = new ApiResponse<CheckOutResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Check-out thành công",
                IsSuccess = true,
                Data = response
            };
            
            return Ok(apiResponse);

        } catch (Exception ex)
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
    [Authorize(Roles = "Admin, Staff, Consultant, Customer")]
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