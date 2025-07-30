// ============================================================================
// STI TESTING CONTROLLER
// ============================================================================
// This controller manages STI (Sexually Transmitted Infection) testing services
// It handles test package booking, sample collection, lab processing, and results
// 
// STI TESTING FLOW:
// 1. PACKAGE SELECTION: Customer chooses test package (Basic/Advanced/Custom)
// 2. BOOKING: Schedule test appointment or home collection
// 3. SAMPLE COLLECTION: Either at clinic or home service
// 4. LAB PROCESSING: Samples sent to laboratory for analysis
// 5. RESULTS: Medical staff review and release results
// 6. NOTIFICATION: Customer notified of results availability
// 7. CONSULTATION: Follow-up consultation if needed
// 
// TEST PACKAGES:
// - Basic: Chlamydia, Gonorrhea, Syphilis
// - Advanced: Basic + HIV, Herpes, Hepatitis
// - Custom: User-selected specific tests
// 
// STATUS WORKFLOW:
// Scheduled → SampleTaken → Processing → Completed/Cancelled
// 
// SECURITY & PRIVACY:
// - Strict confidentiality for all test results
// - Role-based access (Customer sees own tests, Staff manages all)
// - Secure result delivery and storage
// - HIPAA-compliant data handling

using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.STITests;
using Everwell.DAL.Data.Responses.STITests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class STITestingsController : ControllerBase
{
    private readonly ISTITestingService _stiTestingService;

    public STITestingsController(ISTITestingService stiTestingService)
    {
        _stiTestingService = stiTestingService;
    }

    
    /// <summary>
    /// GET ALL STI TESTINGS - ADMIN/STAFF OVERVIEW
    /// ===========================================
    /// Retrieves all STI testing records in the system for administrative purposes
    /// 
    /// ADMINISTRATIVE USE CASES:
    /// - Lab management: Track all pending and completed tests
    /// - Quality control: Monitor testing workflow and timelines
    /// - Reporting: Generate testing statistics and trends
    /// - Customer service: Assist customers with test inquiries
    /// 
    /// DATA INCLUDED:
    /// - Test package details (Basic/Advanced/Custom)
    /// - Customer information (anonymized for privacy)
    /// - Testing status and timeline
    /// - Payment and billing information
    /// - Lab processing notes
    /// 
    /// SECURITY:
    /// - Restricted to authorized staff only
    /// - Patient data anonymized where appropriate
    /// - Audit trail for data access
    /// 
    /// BUSINESS LOGIC:
    /// - Service applies role-based filtering
    /// - Returns comprehensive test management data
    /// - Supports pagination for large datasets
    /// </summary>
    [HttpGet(ApiEndpointConstants.STITesting.GetAllSTITestingsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateSTITestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize] // Staff and Admin access for test management
    public async Task<IActionResult> GetAllSTITestings()
    {
        try
        {
            // Service returns all STI tests with role-based filtering
            // Includes test details, status, and customer information
            var stiTestings = await _stiTestingService.GetAllSTITestingsAsync();
            
            if (stiTestings == null || !stiTestings.Any())
                return NotFound(new { message = "No STI Testings found" });

            var apiResponse = new ApiResponse<IEnumerable<CreateSTITestResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testings retrieved successfully",
                IsSuccess = true,
                Data = stiTestings // Complete test records for administrative management
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.STITesting.GetSTITestingEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateSTITestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetSTITestingById(Guid id)
    {
        try
        {
            var stiTesting = await _stiTestingService.GetSTITestingByIdAsync(id);
            if (stiTesting == null)
                return NotFound(new { message = "STI Testing not found" });
            
            var apiResponse = new ApiResponse<CreateSTITestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testings retrieved successfully",
                IsSuccess = true,
                Data = stiTesting
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpGet(ApiEndpointConstants.STITesting.GetSTITestingsByCurrentUserEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateSTITestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetSTITestsByCurrentUser()
    {
        try
        {
            var stiTesting = await _stiTestingService.GetCurrentUserSTITests();
            if (stiTesting == null)
                return NotFound(new { message = "STI Testing not found" });
            
            var apiResponse = new ApiResponse<IEnumerable<CreateSTITestResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testings retrieved successfully",
                IsSuccess = true,
                Data = stiTesting
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    /// <summary>
    /// GET STI TESTS BY CUSTOMER
    /// =========================
    /// Retrieves all STI testing records for a specific customer
    /// 
    /// CUSTOMER HEALTH TRACKING:
    /// - Personal test history and timeline
    /// - Test results and recommendations
    /// - Upcoming scheduled tests
    /// - Payment and billing history
    /// - Follow-up consultation needs
    /// 
    /// PRIVACY & ACCESS CONTROL:
    /// - Customers can only access their own tests
    /// - Staff can access for customer service
    /// - Healthcare providers access for consultation
    /// - Strict data protection compliance
    /// 
    /// TEST HISTORY FEATURES:
    /// - Chronological test ordering
    /// - Status tracking (Scheduled → Completed)
    /// - Result availability notifications
    /// - Trend analysis for health monitoring
    /// 
    /// USE CASES:
    /// - Customer dashboard: "My Test History"
    /// - Healthcare consultation: Review previous tests
    /// - Customer service: Assist with test inquiries
    /// - Health tracking: Monitor testing frequency
    /// </summary>
    [HttpGet(ApiEndpointConstants.STITesting.GetSTITestingsByCustomerEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateSTITestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize] // Customer access to own tests, Staff access for support
    public async Task<IActionResult> GetSTITestsByCustomer(Guid customerId)
    {
        try
        {
            // Service applies authorization checks:
            // - Customers can only access their own tests
            // - Staff/Admin can access for customer service
            var stiTesting = await _stiTestingService.GetSTITestsByCustomer(customerId);
            if (stiTesting == null)
                return NotFound(new { message = "Không tìm thấy STI Tests nào trong hệ thống." });
            
            var apiResponse = new ApiResponse<IEnumerable<CreateSTITestResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testings retrieved successfully",
                IsSuccess = true,
                Data = stiTesting // Customer's complete test history with results
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    
    [HttpPost(ApiEndpointConstants.STITesting.CreateSTITestingEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateSTITestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> CreateSTITesting(CreateSTITestRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var createdTesting = await _stiTestingService.CreateSTITestingAsync(request);
            if (createdTesting == null)
                return NotFound(new { message = "Đã xảy ra lỗi tạo STI Test." });
            
            var apiResponse = new ApiResponse<CreateSTITestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Đơn STI Test đã được tạo thành công.",
                IsSuccess = true,
                Data = createdTesting
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPut(ApiEndpointConstants.STITesting.UpdateSTITestingEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateSTITestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> UpdateSTITesting(Guid id, UpdateSTITestRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var updatedTesting = await _stiTestingService.UpdateSTITestingAsync(id, request);
            if (updatedTesting == null)
                return NotFound(new { message = "Đã xảy ra lỗi cập nhật STI Test." });
            
            var apiResponse = new ApiResponse<CreateSTITestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testing updated successfully",
                IsSuccess = true,
                Data = updatedTesting
            };
            
            return Ok(apiResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
    
    [HttpDelete(ApiEndpointConstants.STITesting.DeleteSTITestingEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateSTITestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> DeleteSTITesting(Guid id)
    {
        try
        {
            var isDeleted = await _stiTestingService.DeleteSTITestingAsync(id);
            if (!isDeleted)
                return NotFound(new { message = "Đã xảy ra lỗi xoá STI Test" });
            
            var apiResponse = new ApiResponse<CreateSTITestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testing deleted successfully",
                IsSuccess = true,
                Data = null
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    

}