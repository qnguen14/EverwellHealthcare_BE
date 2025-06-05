using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
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
    public async Task<ActionResult<IEnumerable<MenstrualCycleTracking>>> GetAllMenstrualCycleTrackings()
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
    public async Task<ActionResult<MenstrualCycleTracking>> GetMenstrualCycleTrackingById(Guid id)
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
} 