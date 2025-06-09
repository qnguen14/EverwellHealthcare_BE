using Everwell.API.Constants;
using Everwell.API.Models.Requests;
using Everwell.API.Models.Responses;
using Everwell.API.Helpers;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
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

    /// <summary>
    /// Get all services
    /// GET: /api/v1/service/getall
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.GetAllServicesEndpoint)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Service>>>> GetAllServices()
    {
        try
        {
            var services = await _serviceService.GetAllServicesAsync();
            return Ok(new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 200,
                Message = "Services retrieved successfully",
                IsSuccess = true,
                Data = services
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Get service by ID
    /// GET: /api/v1/service/{id}
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.GetServiceEndpoint)]
    public async Task<ActionResult<ApiResponse<Service>>> GetServiceById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<Service>
                {
                    StatusCode = 400,
                    Message = "Invalid service ID",
                    IsSuccess = false
                });
            }

            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(new ApiResponse<Service>
                {
                    StatusCode = 404,
                    Message = "Service not found",
                    IsSuccess = false
                });
            }
            
            return Ok(new ApiResponse<Service>
            {
                StatusCode = 200,
                Message = "Service retrieved successfully",
                IsSuccess = true,
                Data = service
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Service>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new service
    /// POST: /api/v1/service/create
    /// </summary>
    [HttpPost(ApiEndpointConstants.Service.CreateServiceEndpoint)]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Service>>> CreateService([FromBody] CreateServiceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new ApiResponse<Service>
                {
                    StatusCode = 400,
                    Message = "Invalid input data",
                    IsSuccess = false,
                    Reason = errors
                });
            }

            var service = request.ToEntity();
            var createdService = await _serviceService.CreateServiceAsync(service);
            
            return CreatedAtAction(
                nameof(GetServiceById), 
                new { id = createdService.Id }, 
                new ApiResponse<Service>
                {
                    StatusCode = 201,
                    Message = "Service created successfully",
                    IsSuccess = true,
                    Data = createdService
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Service>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing service
    /// PUT: /api/v1/service/update/{id}
    /// </summary>
    [HttpPut(ApiEndpointConstants.Service.UpdateServiceEndpoint)]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Service>>> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<Service>
                {
                    StatusCode = 400,
                    Message = "Invalid service ID",
                    IsSuccess = false
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new ApiResponse<Service>
                {
                    StatusCode = 400,
                    Message = "Invalid input data",
                    IsSuccess = false,
                    Reason = errors
                });
            }

            var existingService = await _serviceService.GetServiceByIdAsync(id);
            if (existingService == null)
            {
                return NotFound(new ApiResponse<Service>
                {
                    StatusCode = 404,
                    Message = "Service not found",
                    IsSuccess = false
                });
            }

            var updatedService = request.ToEntity(existingService);
            var result = await _serviceService.UpdateServiceAsync(id, updatedService);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<Service>
                {
                    StatusCode = 404,
                    Message = "Unable to update service",
                    IsSuccess = false
                });
            }

            return Ok(new ApiResponse<Service>
            {
                StatusCode = 200,
                Message = "Service updated successfully",
                IsSuccess = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Service>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete a service
    /// DELETE: /api/v1/service/delete/{id}
    /// </summary>
    [HttpDelete(ApiEndpointConstants.Service.DeleteServiceEndpoint)]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteService(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = 400,
                    Message = "Invalid service ID",
                    IsSuccess = false
                });
            }

            var existingService = await _serviceService.GetServiceByIdAsync(id);
            if (existingService == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = 404,
                    Message = "Service not found",
                    IsSuccess = false
                });
            }

            var result = await _serviceService.DeleteServiceAsync(id);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = 400,
                    Message = "Unable to delete service",
                    IsSuccess = false
                });
            }

            return Ok(new ApiResponse<bool>
            {
                StatusCode = 200,
                Message = "Service deleted successfully",
                IsSuccess = true,
                Data = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Get services by category (based on diag.vn services)
    /// GET: /api/v1/service/category/{category}
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.ServiceEndpoint + "/category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Service>>>> GetServicesByCategory(string category)
    {
        try
        {
            var allServices = await _serviceService.GetAllServicesAsync();
            var filteredServices = ServiceCategoryHelper.FilterByCategory(allServices, category);
            
            return Ok(new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 200,
                Message = $"Services for category '{category}' retrieved successfully",
                IsSuccess = true,
                Data = filteredServices
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Search services with filters
    /// GET: /api/v1/service/search
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.ServiceEndpoint + "/search")]
    public async Task<ActionResult<SearchServiceResponse>> SearchServices([FromQuery] SearchServiceRequest request)
    {
        try
        {
            var allServices = await _serviceService.GetAllServicesAsync();
            var response = ServiceSearchHelper.ApplyFiltersWithPagination(allServices, request);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SearchServiceResponse
            {
                Success = false,
                Message = "Internal server error: " + ex.Message,
                Data = new List<Service>(),
                Count = 0
            });
        }
    }

    /// <summary>
    /// Get active services only
    /// GET: /api/v1/service/active
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.ServiceEndpoint + "/active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Service>>>> GetActiveServices()
    {
        try
        {
            var allServices = await _serviceService.GetAllServicesAsync();
            var activeServices = allServices.Where(s => s.IsActive).ToList();
            
            return Ok(new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 200,
                Message = "Active services retrieved successfully",
                IsSuccess = true,
                Data = activeServices
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Get services by price range
    /// GET: /api/v1/service/price-range?min={min}&max={max}
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.ServiceEndpoint + "/price-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Service>>>> GetServicesByPriceRange(
        [FromQuery] decimal min = 0,
        [FromQuery] decimal max = decimal.MaxValue)
    {
        try
        {
            var allServices = await _serviceService.GetAllServicesAsync();
            var filteredServices = allServices.Where(s => s.Price >= min && s.Price <= max).ToList();
            
            return Ok(new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 200,
                Message = $"Services in price range {min:C} - {max:C} retrieved successfully",
                IsSuccess = true,
                Data = filteredServices
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Service>>
            {
                StatusCode = 500,
                Message = "Internal server error",
                IsSuccess = false,
                Reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Get service statistics
    /// GET: /api/v1/service/statistics
    /// </summary>
    [HttpGet(ApiEndpointConstants.Service.ServiceEndpoint + "/statistics")]
    public async Task<ActionResult<ServiceStatisticsResponse>> GetServiceStatistics()
    {
        try
        {
            var allServices = await _serviceService.GetAllServicesAsync();
            var statistics = ServiceStatsHelper.GetServiceStatistics(allServices);
            
            return Ok(new ServiceStatisticsResponse
            {
                Success = true,
                Message = "Service statistics retrieved successfully",
                Data = statistics
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ServiceStatisticsResponse
            {
                Success = false,
                Message = "Internal server error: " + ex.Message
            });
        }
    }
} 