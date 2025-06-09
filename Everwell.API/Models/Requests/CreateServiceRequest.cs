using Everwell.DAL.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Everwell.API.Models.Requests;

/// <summary>
/// Request model for creating a new service
/// </summary>
public class CreateServiceRequest
{
    /// <summary>
    /// Service name
    /// </summary>
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
    public required string Name { get; set; }

    /// <summary>
    /// Service description
    /// </summary>
    [Required(ErrorMessage = "Service description is required")]
    [StringLength(256, ErrorMessage = "Description cannot exceed 256 characters")]
    public required string Description { get; set; }

    /// <summary>
    /// Service price
    /// </summary>
    [Required(ErrorMessage = "Service price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    /// <summary>
    /// Active status
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Convert request to Service entity
    /// </summary>
    public Service ToEntity()
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            Description = Description.Trim(),
            Price = Price,
            IsActive = IsActive ?? true,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            UpdatedAt = DateOnly.FromDateTime(DateTime.Now)
        };
    }
} 