using Everwell.DAL.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Everwell.API.Models.Requests;

/// <summary>
/// Request model for updating a service
/// </summary>
public class UpdateServiceRequest
{
    /// <summary>
    /// Service name
    /// </summary>
    [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Service description
    /// </summary>
    [StringLength(256, ErrorMessage = "Description cannot exceed 256 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Service price
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Convert request to Service entity using existing service data
    /// </summary>
    public Service ToEntity(Service existingService)
    {
        return new Service
        {
            Id = existingService.Id,
            Name = Name?.Trim() ?? existingService.Name,
            Description = Description?.Trim() ?? existingService.Description,
            Price = Price ?? existingService.Price,
            IsActive = IsActive ?? existingService.IsActive,
            CreatedAt = existingService.CreatedAt,
            UpdatedAt = DateOnly.FromDateTime(DateTime.Now)
        };
    }
} 