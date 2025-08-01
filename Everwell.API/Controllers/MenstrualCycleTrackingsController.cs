using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.MenstrualCycle;
using Everwell.DAL.Data.Responses.MenstrualCycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class MenstrualCycleTrackingsController : ControllerBase
{
    private readonly IMenstrualCycleTrackingService _menstrualCycleTrackingService;

    public MenstrualCycleTrackingsController(IMenstrualCycleTrackingService menstrualCycleTrackingService)
    {
        _menstrualCycleTrackingService = menstrualCycleTrackingService;
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetAllMenstrualCycleTrackingsEndpoint)]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GetMenstrualCycleResponse>>> GetAllMenstrualCycleTrackings()
    {
        try
        {
            var trackings = await _menstrualCycleTrackingService.GetAllMenstrualCycleTrackingsAsync();
            return Ok(trackings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetMenstrualCycleTrackingEndpoint)]
    [Authorize]
    public async Task<ActionResult<GetMenstrualCycleResponse>> GetMenstrualCycleTrackingById(Guid id)
    {
        try
        {
            var tracking = await _menstrualCycleTrackingService.GetMenstrualCycleTrackingByIdAsync(id);
            if (tracking == null)
                return NotFound(new { message = "Menstrual Cycle Tracking not found" });
            
            return Ok(tracking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost(ApiEndpointConstants.MenstrualCycleTracking.CreateMenstrualCycleTrackingEndpoint)]
    [Authorize(Roles = "Customer")] // Only customers can track their own cycles
    public async Task<ActionResult<CreateMenstrualCycleResponse>> CreateMenstrualCycleTracking([FromBody] CreateMenstrualCycleRequest request)
    {
        try
        {
            var userId = GetCurrentUserId(); // Extract user ID from JWT token claims
            
            // Service handles cycle validation, overlap detection, and notification scheduling
            var tracking = await _menstrualCycleTrackingService.CreateMenstrualCycleTrackingAsync(request, userId);
            return CreatedAtAction(nameof(GetMenstrualCycleTrackingById), new { id = tracking.TrackingId }, tracking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.MenstrualCycleTracking.UpdateMenstrualCycleTrackingEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CreateMenstrualCycleResponse>> UpdateMenstrualCycleTracking(Guid id, [FromBody] UpdateMenstrualCycleRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Critical: Verify ownership to prevent unauthorized access to sensitive health data
            var existingTracking = await _menstrualCycleTrackingService.GetMenstrualCycleTrackingByIdAsync(id);
            if (existingTracking == null)
                return NotFound(new { message = "Menstrual Cycle Tracking not found" });
                
            // HIPAA compliance: Users can only modify their own reproductive health data
            if (existingTracking.CustomerId != userId)
                return Forbid("You can only update your own cycle tracking data");
            
            var tracking = await _menstrualCycleTrackingService.UpdateMenstrualCycleTrackingAsync(id, request);
            return Ok(tracking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.MenstrualCycleTracking.DeleteMenstrualCycleTrackingEndpoint)]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> DeleteMenstrualCycleTracking(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            // Admin bypass: Admins can delete any cycle data for moderation/support purposes
            if (userRole != "Admin")
            {
                var existingTracking = await _menstrualCycleTrackingService.GetMenstrualCycleTrackingByIdAsync(id);
                if (existingTracking == null)
                    return NotFound(new { message = "Menstrual Cycle Tracking not found" });
                    
                // Privacy protection: Non-admin users can only delete their own sensitive health data
                if (existingTracking.CustomerId != userId)
                    return Forbid("You can only delete your own cycle tracking data");
            }
            
            var result = await _menstrualCycleTrackingService.DeleteMenstrualCycleTrackingAsync(id);
            if (!result)
                return NotFound(new { message = "Menstrual Cycle Tracking not found" });
                
            return Ok(new { message = "Menstrual Cycle Tracking deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetCycleHistoryEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<GetMenstrualCycleResponse>>> GetCycleHistory([FromQuery] int months = 12)
    {
        try
        {
            var userId = GetCurrentUserId();
            var history = await _menstrualCycleTrackingService.GetCycleHistoryAsync(userId, months);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.PredictNextCycleEndpoint)]
    [Authorize(Roles = "Customer")] // Predictions are personal and user-specific
    public async Task<ActionResult<CyclePredictionResponse>> PredictNextCycle()
    {
        try
        {
            var userId = GetCurrentUserId();
            // Complex algorithm: Uses historical data, cycle patterns, and ML to predict next period
            var prediction = await _menstrualCycleTrackingService.PredictNextCycleAsync(userId);
            return Ok(prediction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetFertilityWindowEndpoint)]
    [Authorize(Roles = "Customer")] // Fertility data is highly sensitive reproductive information
    public async Task<ActionResult<FertilityWindowResponse>> GetFertilityWindow()
    {
        try
        {
            var userId = GetCurrentUserId();
            // Critical calculation: Determines ovulation window for family planning/contraception
            var fertilityWindow = await _menstrualCycleTrackingService.GetFertilityWindowAsync(userId);
            return Ok(fertilityWindow);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetCycleAnalyticsEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CycleAnalyticsResponse>> GetCycleAnalytics()
    {
        try
        {
            var userId = GetCurrentUserId();
            var analytics = await _menstrualCycleTrackingService.GetCycleAnalyticsAsync(userId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetCycleInsightsEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CycleInsightsResponse>> GetCycleInsights()
    {
        try
        {
            var userId = GetCurrentUserId();
            var insights = await _menstrualCycleTrackingService.GetCycleInsightsAsync(userId);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetNotificationsEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUpcomingNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _menstrualCycleTrackingService.GetUpcomingNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost(ApiEndpointConstants.MenstrualCycleTracking.UpdateNotificationPreferencesEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> UpdateNotificationPreferences([FromBody] NotificationPreferencesRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _menstrualCycleTrackingService.UpdateNotificationPreferencesAsync(userId, request);
            
            if (result)
                return Ok(new { message = "Notification preferences updated successfully" });
            else
                return BadRequest(new { message = "Failed to update notification preferences" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.MenstrualCycleTracking.GetCycleTrendsEndpoint)]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<CycleTrendData>>> GetCycleTrends([FromQuery] int months = 6)
    {
        try
        {
            var userId = GetCurrentUserId();
            var trends = await _menstrualCycleTrackingService.GetCycleTrendsAsync(userId, months);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        // Extract user ID from JWT token - critical for data isolation and security
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        // Fail fast if token is invalid or missing user ID to prevent unauthorized access
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }

    private string GetCurrentUserRole()
    {
        // Role-based authorization for admin functions and data access control
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException("User role not found in token");
    }
}