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

    
    [HttpGet(ApiEndpointConstants.STITesting.GetAllSTITestingsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateSTITestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetAllSTITestings()
    {
        try
        {
            var stiTestings = await _stiTestingService.GetAllSTITestingsAsync();
            
            if (stiTestings == null || !stiTestings.Any())
                return NotFound(new { message = "No STI Testings found" });

            var apiResponse = new ApiResponse<IEnumerable<CreateSTITestResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "STI Testings retrieved successfully",
                IsSuccess = true,
                Data = stiTestings
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
    
    [HttpGet(ApiEndpointConstants.STITesting.GetSTITestingsByCustomerEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateSTITestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetSTITestsByCustomer(Guid customerId)
    {
        try
        {
            var stiTesting = await _stiTestingService.GetSTITestsByCustomer(customerId);
            if (stiTesting == null)
                return NotFound(new { message = "Không tìm thấy STI Tests nào trong hệ thống." });
            
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