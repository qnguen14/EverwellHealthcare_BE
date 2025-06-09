using Everwell.API.Models.Requests;
using Everwell.DAL.Data.Entities;

namespace Everwell.API.Models.Responses;

/// <summary>
/// Response model for service search with pagination
/// </summary>
public class SearchServiceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<Service> Data { get; set; } = new List<Service>();
    public int Count { get; set; }
    public SearchServiceRequest? Filters { get; set; }
    public PaginationInfo? Pagination { get; set; }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static SearchServiceResponse CreateError(string message)
    {
        return new SearchServiceResponse
        {
            Success = false,
            Message = message,
            Data = new List<Service>(),
            Count = 0
        };
    }
}

/// <summary>
/// Pagination information
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Service summary response for lightweight operations
/// </summary>
public class ServiceSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
} 