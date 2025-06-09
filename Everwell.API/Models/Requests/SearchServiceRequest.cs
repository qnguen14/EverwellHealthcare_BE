namespace Everwell.API.Models.Requests;

/// <summary>
/// Request model for searching services
/// </summary>
public class SearchServiceRequest
{
    /// <summary>
    /// Search keyword
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    public decimal? PriceMin { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    public decimal? PriceMax { get; set; }

    /// <summary>
    /// Active status filter
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Category filter
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Page number for pagination (starting from 1)
    /// </summary>
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int? PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; } = "Name";

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "asc";
} 