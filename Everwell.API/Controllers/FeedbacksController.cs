using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Feedback;
using Everwell.DAL.Data.Responses.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

/// <summary>
/// Controller responsible for managing feedback operations in the Everwell Healthcare system.
/// Handles CRUD operations for feedback entities and provides specialized endpoints for different user roles.
/// </summary>
[ApiController]
public class FeedbacksController : ControllerBase
{
    #region Fields
    
    /// <summary>
    /// Service for handling feedback business logic operations
    /// </summary>
    private readonly IFeedbackService _feedbackService;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes a new instance of the FeedbacksController
    /// </summary>
    /// <param name="feedbackService">The feedback service for business logic operations</param>
    public FeedbacksController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }
    
    #endregion
    
    #region CRUD Operations
    
    /// <summary>
    /// Retrieves all feedbacks in the system
    /// </summary>
    /// <returns>A list of all feedback responses</returns>
    /// <response code="200">Returns all feedbacks successfully</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet(ApiEndpointConstants.Feedback.GetAllFeedbacksEndpoint)]
    [Authorize(Roles = "Admin,Consultant,Customer")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        try
        {
            var feedbacks = await _feedbackService.GetAllFeedbackResponsesAsync();
            return Ok(CreateSuccessResponse(feedbacks, "Feedbacks retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve feedbacks");
        }
    }

    /// <summary>
    /// Retrieves a specific feedback by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the feedback</param>
    /// <returns>The feedback details if found</returns>
    /// <response code="200">Returns the feedback successfully</response>
    /// <response code="404">Feedback not found</response>
    /// <response code="500">Internal server error occurred</response>
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
                return NotFound(CreateErrorResponse("Feedback not found", StatusCodes.Status404NotFound));
            }
            
            return Ok(CreateSuccessResponse(feedback, "Feedback retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve feedback");
        }
    }

    /// <summary>
    /// Creates a new feedback entry in the system
    /// </summary>
    /// <param name="request">The feedback creation request containing all necessary details</param>
    /// <returns>The created feedback details</returns>
    /// <response code="201">Feedback created successfully</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost(ApiEndpointConstants.Feedback.CreateFeedbackEndpoint)]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<CreateFeedbackResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        try
        {
            // Validate the incoming request model
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Validation failed", StatusCodes.Status400BadRequest, ModelState));
            }

            var result = await _feedbackService.CreateFeedbackAsync(request);
            var response = CreateSuccessResponse(result, "Feedback created successfully", StatusCodes.Status201Created);
            
            return CreatedAtAction(nameof(GetFeedbackById), new { id = result.FeedbackId }, response);
        }
        catch (InvalidOperationException ex)
        {
            // Handle business logic violations
            return BadRequest(CreateErrorResponse(ex.Message, StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to create feedback");
        }
    }

    /// <summary>
    /// Updates an existing feedback entry
    /// </summary>
    /// <param name="id">The unique identifier of the feedback to update</param>
    /// <param name="request">The feedback update request containing modified details</param>
    /// <returns>The updated feedback details</returns>
    /// <response code="200">Feedback updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Feedback not found or unauthorized</response>
    /// <response code="500">Internal server error occurred</response>
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
            // Validate the incoming request model
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Validation failed", StatusCodes.Status400BadRequest, ModelState));
            }

            var result = await _feedbackService.UpdateFeedbackAsync(id, request);
            if (result == null)
            {
                return NotFound(CreateErrorResponse("Feedback not found or you are not authorized to update it", StatusCodes.Status404NotFound));
            }

            return Ok(CreateSuccessResponse(result, "Feedback updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to update feedback");
        }
    }

    /// <summary>
    /// Deletes a feedback entry from the system
    /// </summary>
    /// <param name="id">The unique identifier of the feedback to delete</param>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">Feedback deleted successfully</response>
    /// <response code="404">Feedback not found</response>
    /// <response code="500">Internal server error occurred</response>
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
                return NotFound(CreateErrorResponse("Feedback not found", StatusCodes.Status404NotFound));
            }

            return Ok(CreateSuccessResponse<object>(null, "Feedback deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to delete feedback");
        }
    }
    
    #endregion
    
    #region Specialized Endpoints

    /// <summary>
    /// Retrieves all feedbacks associated with a specific customer
    /// </summary>
    /// <param name="customerId">Optional customer ID. If not provided, uses the current user's ID</param>
    /// <returns>A list of feedbacks for the specified customer</returns>
    /// <response code="200">Customer feedbacks retrieved successfully</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("/api/v2.5/feedback/customer")]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbacksByCustomer(Guid? customerId = null)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByCustomerAsync(customerId);
            return Ok(CreateSuccessResponse(feedbacks, "Customer feedbacks retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve customer feedbacks");
        }
    }

    /// <summary>
    /// Retrieves all feedbacks associated with a specific consultant
    /// </summary>
    /// <param name="consultantId">Optional consultant ID. If not provided, uses the current user's ID</param>
    /// <returns>A list of feedbacks for the specified consultant</returns>
    /// <response code="200">Consultant feedbacks retrieved successfully</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("/api/v2.5/feedback/consultant")]
    [Authorize(Roles = "Consultant,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbacksByConsultant(Guid? consultantId = null)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByConsultantAsync(consultantId);
            return Ok(CreateSuccessResponse(feedbacks, "Consultant feedbacks retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve consultant feedbacks");
        }
    }

    /// <summary>
    /// Retrieves feedback associated with a specific appointment
    /// </summary>
    /// <param name="appointmentId">The unique identifier of the appointment</param>
    /// <returns>The feedback for the specified appointment</returns>
    /// <response code="200">Appointment feedback retrieved successfully</response>
    /// <response code="404">No feedback found for this appointment</response>
    /// <response code="500">Internal server error occurred</response>
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
                return NotFound(CreateErrorResponse("No feedback found for this appointment", StatusCodes.Status404NotFound));
            }

            return Ok(CreateSuccessResponse(feedback, "Appointment feedback retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve appointment feedback");
        }
    }

    /// <summary>
    /// Validates whether a customer can provide feedback for a specific appointment
    /// </summary>
    /// <param name="appointmentId">The unique identifier of the appointment</param>
    /// <returns>Boolean indicating if feedback can be provided</returns>
    /// <response code="200">Validation result returned successfully</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("/api/v2.5/feedback/can-provide/{appointmentId}")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanProvideFeedback(Guid appointmentId)
    {
        try
        {
            var canProvide = await _feedbackService.CanCustomerProvideFeedbackAsync(appointmentId);
            var message = canProvide ? "You can provide feedback for this appointment" : "You cannot provide feedback for this appointment";
            return Ok(CreateSuccessResponse(canProvide, message));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to validate feedback eligibility");
        }
    }

    /// <summary>
    /// Retrieves public reviews for a specific consultant (visible to customers)
    /// </summary>
    /// <param name="consultantId">The unique identifier of the consultant</param>
    /// <returns>A list of public reviews for the consultant</returns>
    /// <response code="200">Public consultant reviews retrieved successfully</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet(ApiEndpointConstants.Feedback.GetPublicConsultantReviewsEndpoint)]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublicConsultantReviews(Guid consultantId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByConsultantAsync(consultantId);
            return Ok(CreateSuccessResponse(feedbacks, "Public consultant reviews retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to retrieve public consultant reviews");
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    /// <param name="data">The data to include in the response</param>
    /// <param name="message">Success message</param>
    /// <param name="statusCode">HTTP status code (defaults to 200)</param>
    /// <returns>Standardized API response</returns>
    private static ApiResponse<T> CreateSuccessResponse<T>(T data, string message, int statusCode = StatusCodes.Status200OK)
    {
        return new ApiResponse<T>
        {
            Message = message,
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data
        };
    }
    
    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="data">Optional additional error data</param>
    /// <returns>Standardized API error response</returns>
    private static ApiResponse<object> CreateErrorResponse(string message, int statusCode, object? data = null)
    {
        return new ApiResponse<object>
        {
            Message = message,
            IsSuccess = false,
            StatusCode = statusCode,
            Data = data
        };
    }
    
    /// <summary>
    /// Handles exceptions and returns a standardized error response
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="customMessage">Custom error message for the user</param>
    /// <returns>Standardized error response</returns>
    private ObjectResult HandleException(Exception ex, string customMessage)
    {
        var response = new ApiResponse<object>
        {
            Message = customMessage,
            IsSuccess = false,
            StatusCode = StatusCodes.Status500InternalServerError,
            Data = new { details = ex.Message }
        };
        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }
    
    #endregion
}