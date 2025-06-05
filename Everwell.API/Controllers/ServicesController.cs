using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet(ApiEndpointConstants.Service.GetAllServicesEndpoint)]
    public async Task<ActionResult<IEnumerable<Service>>> GetAllServices()
    {
        try
        {
            var services = await _serviceService.GetAllServicesAsync();
            return Ok(services);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Service.GetServiceEndpoint)]
    public async Task<ActionResult<Service>> GetServiceById(Guid id)
    {
        try
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
                return NotFound(new { message = "Service not found" });
            
            return Ok(service);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
} 