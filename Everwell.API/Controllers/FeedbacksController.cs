using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Feedback;
using Everwell.DAL.Data.Responses.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class FeedbacksController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbacksController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet(ApiEndpointConstants.Feedback.GetAllFeedbacksEndpoint)]
    [Authorize(Roles = "Admin,Consultant,Customer")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        try
        {
            var feedbacks = await _feedbackService.GetAllFeedbackResponsesAsync();
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Feedbacks retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpGet(ApiEndpointConstants.Feedback.GetFeedbackEndpoint)]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FeedbackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbackById(Guid id)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackResponseByIdAsync(id);
            if (feedback == null)
            {
                var notFoundResponse = new ApiResponse<object>
                {
                    Message = "Feedback not found",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                };
                return NotFound(notFoundResponse);
            }
            
            var response = new ApiResponse<FeedbackResponse>
            {
                Message = "Feedback retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedback
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpPost(ApiEndpointConstants.Feedback.CreateFeedbackEndpoint)]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<CreateFeedbackResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var validationResponse = new ApiResponse<object>
                {
                    Message = "Validation failed",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Data = ModelState
                };
                return BadRequest(validationResponse);
            }

            var result = await _feedbackService.CreateFeedbackAsync(request);
            var response = new ApiResponse<CreateFeedbackResponse>
            {
                Message = "Feedback created successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Data = result
            };
            return CreatedAtAction(nameof(GetFeedbackById), new { id = result.FeedbackId }, response);
        }
        catch (InvalidOperationException ex)
        {
            var response = new ApiResponse<object>
            {
                Message = ex.Message,
                IsSuccess = false,
                StatusCode = StatusCodes.Status400BadRequest
            };
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpPut(ApiEndpointConstants.Feedback.UpdateFeedbackEndpoint)]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<FeedbackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var validationResponse = new ApiResponse<object>
                {
                    Message = "Validation failed",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Data = ModelState
                };
                return BadRequest(validationResponse);
            }

            var result = await _feedbackService.UpdateFeedbackAsync(id, request);
            if (result == null)
            {
                var notFoundResponse = new ApiResponse<object>
                {
                    Message = "Feedback not found or you are not authorized to update it",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                };
                return NotFound(notFoundResponse);
            }

            var response = new ApiResponse<FeedbackResponse>
            {
                Message = "Feedback updated successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = result
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpDelete(ApiEndpointConstants.Feedback.DeleteFeedbackEndpoint)]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFeedback(Guid id)
    {
        try
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (!result)
            {
                var notFoundResponse = new ApiResponse<object>
                {
                    Message = "Feedback not found",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                };
                return NotFound(notFoundResponse);
            }

            var response = new ApiResponse<object>
            {
                Message = "Feedback deleted successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    // User-specific endpoints
    [HttpGet("/api/v2.5/feedback/customer")]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbacksByCustomer(Guid? customerId = null)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByCustomerAsync(customerId);
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Customer feedbacks retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpGet("/api/v2.5/feedback/consultant")]
    [Authorize(Roles = "Consultant,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbacksByConsultant(Guid? consultantId = null)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByConsultantAsync(consultantId);
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Consultant feedbacks retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    [HttpGet("/api/v2.5/feedback/appointment/{appointmentId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FeedbackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbackByAppointment(Guid appointmentId)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByAppointmentAsync(appointmentId);
            if (feedback == null)
            {
                var notFoundResponse = new ApiResponse<object>
                {
                    Message = "No feedback found for this appointment",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound
                };
                return NotFound(notFoundResponse);
            }

            var response = new ApiResponse<FeedbackResponse>
            {
                Message = "Appointment feedback retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedback
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    // Validation endpoints
    [HttpGet("/api/v2.5/feedback/can-provide/{appointmentId}")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanProvideFeedback(Guid appointmentId)
    {
        try
        {
            var canProvide = await _feedbackService.CanCustomerProvideFeedbackAsync(appointmentId);
            var response = new ApiResponse<bool>
            {
                Message = canProvide ? "You can provide feedback for this appointment" : "You cannot provide feedback for this appointment",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = canProvide
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }

    // Public consultant reviews endpoint for customers
    [HttpGet(ApiEndpointConstants.Feedback.GetPublicConsultantReviewsEndpoint)]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublicConsultantReviews(Guid consultantId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByConsultantAsync(consultantId);
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Public consultant reviews retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Message = "Internal server error",
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Data = new { details = ex.Message }
            };
            return StatusCode(500, response);
        }
    }
} 