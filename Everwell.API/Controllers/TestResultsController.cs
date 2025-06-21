using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.TestResult;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Data.Responses.TestResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class TestResultsController : ControllerBase
{
    private readonly ITestResultService _testResultService;

    public TestResultsController(ITestResultService testResultService)
    {
        _testResultService = testResultService;
    }

    [HttpGet(ApiEndpointConstants.TestResult.GetAllTestResultsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateTestResultResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    [Authorize]
    public async Task<IActionResult> GetAllTestResults()
    {
        try
        {
            var testResults = await _testResultService.GetAllTestResultsAsync();

            if (testResults == null || !testResults.Any())
                return NotFound(new { message = "No test results found" });

            var apiResponse = new ApiResponse<IEnumerable<CreateTestResultResponse>>
            {
                Data = testResults,
                Message = "Test results retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.TestResult.GetTestResultEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateTestResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetTestResultById(Guid id)
    {
        try
        {
            var testResult = await _testResultService.GetTestResultByIdAsync(id);
            if (testResult == null)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<CreateTestResultResponse>
            {
                Data = testResult,
                Message = "Test result retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };


            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.TestResult.GetTestResultsBySTITestsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateTestResultResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetTestResultBySTITest(Guid id)
    {
        try
        {
            var testResults = await _testResultService.GetTestResultsBySTITestingIdAsync(id);
            if (testResults == null)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<IEnumerable<CreateTestResultResponse>>
            {
                Data = testResults,
                Message = "Test result retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };


            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.TestResult.GetTestResultByCustomerEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateTestResultResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetTestResultsByCustomer(Guid id)
    {
        try
        {
            var testResults = await _testResultService.GetTestResultByCustomerAsync(id);
            if (testResults == null)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<IEnumerable<CreateTestResultResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Test result retrieved successfully",
                IsSuccess = true,
                Data = testResults,
            };


            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost(ApiEndpointConstants.TestResult.CreateTestResultEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> CreateTestResult(CreateTestResultRequest request)
    {
        try
        {
            var testResult = await _testResultService.CreateTestResultAsync(request);
            if (testResult == null)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<CreateTestResultResponse>
            {
                Data = testResult,
                Message = "Test result created successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };


            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.TestResult.UpdateTestResultEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateTestResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> UpdateTestResult(Guid id, UpdateTestResultRequest request)
    {
        try
        {
            var testResult = await _testResultService.UpdateTestResultAsync(id, request);
            if (testResult == null)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<CreateTestResultResponse>
            {
                Data = testResult,
                Message = "Test result updated successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };

            return Ok(apiResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.TestResult.DeleteTestResultEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> DeleteTestResult(Guid id)
    {
        try
        {
            var result = await _testResultService.DeleteTestResultAsync(id);
            if (!result)
                return NotFound(new { message = "Test Result not found" });

            var apiResponse = new ApiResponse<bool>
            {
                Data = true,
                Message = "Test result deleted successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
