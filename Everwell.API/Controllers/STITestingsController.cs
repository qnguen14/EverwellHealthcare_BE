using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
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
    [Authorize]
    public async Task<ActionResult<IEnumerable<STITesting>>> GetAllSTITestings()
    {
        try
        {
            var stiTestings = await _stiTestingService.GetAllSTITestingsAsync();
            return Ok(stiTestings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.STITesting.GetSTITestingEndpoint)]
    [Authorize]
    public async Task<ActionResult<STITesting>> GetSTITestingById(Guid id)
    {
        try
        {
            var stiTesting = await _stiTestingService.GetSTITestingByIdAsync(id);
            if (stiTesting == null)
                return NotFound(new { message = "STI Testing not found" });
            
            return Ok(stiTesting);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
} 