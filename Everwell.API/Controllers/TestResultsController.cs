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
    public async Task<ActionResult<IEnumerable<TestResult>>> GetAllTestResults()
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
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<TestResult>> GetTestResultById(Guid id)
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

    [HttpPost(ApiEndpointConstants.TestResult.CreateTestResultEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<TestResult>> CreateTestResult(CreateTestResultRequest request)
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
    [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<TestResult>> UpdateTestResult(Guid id, CreateTestResultRequest request)
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

        [HttpDelete(ApiEndpointConstants.TestResult.DeleteTestResultEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateAppointmentsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<ActionResult<TestResult>> DeleteTestResult(Guid id)
        {
            try
            {
                var testResult = await _testResultService.DeleteTestResultAsync(id);
                if (testResult == null)
                    return NotFound(new { message = "Test Result not found" });

                var apiResponse = new ApiResponse<CreateTestResultResponse>
                {
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
