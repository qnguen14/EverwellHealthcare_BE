using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
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
    [Authorize]
    public async Task<ActionResult<IEnumerable<TestResult>>> GetAllTestResults()
    {
        try
        {
            var testResults = await _testResultService.GetAllTestResultsAsync();
            return Ok(testResults);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.TestResult.GetTestResultEndpoint)]
    [Authorize]
    public async Task<ActionResult<TestResult>> GetTestResultById(Guid id)
    {
        try
        {
            var testResult = await _testResultService.GetTestResultByIdAsync(id);
            if (testResult == null)
                return NotFound(new { message = "Test Result not found" });
            
            return Ok(testResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
} 