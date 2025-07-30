// ============================================================================
// FEEDBACKS CONTROLLER
// ============================================================================
// This controller manages customer feedback, reviews, and service ratings
// It handles feedback collection, moderation, and quality improvement insights
// 
// FEEDBACK SYSTEM FLOW:
// 1. SERVICE COMPLETION: Customer completes appointment or service
// 2. FEEDBACK REQUEST: System prompts for feedback via notification
// 3. FEEDBACK SUBMISSION: Customer provides rating and comments
// 4. MODERATION: Staff reviews feedback for appropriateness
// 5. PUBLICATION: Approved feedback displayed publicly
// 6. ANALYTICS: Feedback data used for service improvement
// 
// FEEDBACK TYPES:
// - Appointment feedback: Rating consultant and consultation quality
// - Service feedback: STI testing, cycle tracking, general app experience
// - Feature feedback: Suggestions for app improvements
// - Support feedback: Customer service experience ratings
// 
// RATING SYSTEM:
// - 5-star rating scale for overall satisfaction
// - Category-specific ratings (communication, expertise, timeliness)
// - Written comments for detailed feedback
// - Anonymous feedback option for sensitive topics
// 
// QUALITY ASSURANCE:
// - Feedback moderation to filter inappropriate content
// - Sentiment analysis for automated quality monitoring
// - Trend analysis for service improvement insights
// - Response system for addressing customer concerns
// 
// PRIVACY & MODERATION:
// - Customer identity protection in public reviews
// - Content filtering for inappropriate language
// - Healthcare provider response capabilities
// - Feedback analytics for business intelligence

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

    /// <summary>
    /// GET ALL FEEDBACKS
    /// =================
    /// Retrieves all feedback records with role-based filtering and access control
    /// 
    /// ROLE-BASED ACCESS:
    /// - Admin: View all feedbacks for quality monitoring and analytics
    /// - Consultant: View feedbacks about their services for improvement
    /// - Customer: View public feedbacks to make informed decisions
    /// 
    /// FEEDBACK ANALYTICS:
    /// - Overall service quality trends
    /// - Individual consultant performance metrics
    /// - Service improvement opportunities
    /// - Customer satisfaction patterns
    /// 
    /// DATA FILTERING:
    /// - Service applies role-based data filtering
    /// - Sensitive information protected based on user role
    /// - Public vs private feedback distinction
    /// - Moderation status consideration
    /// 
    /// USE CASES:
    /// - Admin dashboard: Service quality overview
    /// - Consultant profile: Performance insights
    /// - Customer decision: Service provider selection
    /// - Quality assurance: Trend monitoring
    /// </summary>
    [HttpGet(ApiEndpointConstants.Feedback.GetAllFeedbacksEndpoint)]
    [Authorize(Roles = "Admin,Consultant,Customer")] // Role-based feedback access
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        try
        {
            // Service applies role-based filtering and privacy controls
            // Returns appropriate feedback data based on user's role and permissions
            var feedbacks = await _feedbackService.GetAllFeedbackResponsesAsync();
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Feedbacks retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks // Filtered feedback data based on user role
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

    /// <summary>
    /// GET FEEDBACKS BY CUSTOMER
    /// =========================
    /// Retrieves feedback records for a specific customer
    /// 
    /// CUSTOMER FEEDBACK TRACKING:
    /// - Personal feedback history and submissions
    /// - Service experience timeline
    /// - Rating patterns and preferences
    /// - Follow-up feedback requests
    /// 
    /// ACCESS CONTROL:
    /// - Customers can view their own feedback history
    /// - Admins can view any customer's feedback for support
    /// - Privacy protection for sensitive feedback content
    /// 
    /// FEEDBACK INSIGHTS:
    /// - Customer satisfaction trends over time
    /// - Service improvement based on individual feedback
    /// - Personalized service recommendations
    /// - Quality assurance follow-up opportunities
    /// 
    /// USE CASES:
    /// - Customer profile: "My Feedback History"
    /// - Admin support: Customer service issue resolution
    /// - Quality improvement: Individual customer experience analysis
    /// - Service personalization: Tailored recommendations
    /// </summary>
    // User-specific endpoints
    [HttpGet("/api/v2.5/feedback/customer")]
    [Authorize(Roles = "Customer,Admin")] // Customer own data + Admin support access
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeedbacksByCustomer(Guid? customerId = null)
    {
        try
        {
            // Service handles authorization:
            // - Customers can only access their own feedback
            // - Admins can access any customer's feedback for support
            var feedbacks = await _feedbackService.GetFeedbacksByCustomerAsync(customerId);
            var response = new ApiResponse<IEnumerable<FeedbackResponse>>
            {
                Message = "Customer feedbacks retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = feedbacks // Customer's complete feedback history
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