using Everwell.API.Models.Requests;
using Everwell.API.Models.Responses;
using Everwell.DAL.Data.Entities;

namespace Everwell.API.Helpers;

/// <summary>
/// Helper class for applying search filters to services
/// </summary>
public static class ServiceSearchHelper
{
    /// <summary>
    /// Apply search filters to services collection
    /// </summary>
    public static IEnumerable<Service> ApplyFilters(IEnumerable<Service> services, SearchServiceRequest request)
    {
        var query = services.AsQueryable();

        // Filter by keyword
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(s => 
                s.Name.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by price range
        if (request.PriceMin.HasValue)
        {
            query = query.Where(s => s.Price >= request.PriceMin.Value);
        }

        if (request.PriceMax.HasValue)
        {
            query = query.Where(s => s.Price <= request.PriceMax.Value);
        }

        // Filter by active status
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryFiltered = ServiceCategoryHelper.FilterByCategory(query, request.Category);
            query = categoryFiltered.AsQueryable();
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        return query.ToList();
    }

    /// <summary>
    /// Apply search filters with pagination
    /// </summary>
    public static SearchServiceResponse ApplyFiltersWithPagination(IEnumerable<Service> services, SearchServiceRequest request)
    {
        var query = services.AsQueryable();

        // Filter by keyword
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(s => 
                s.Name.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by price range
        if (request.PriceMin.HasValue)
        {
            query = query.Where(s => s.Price >= request.PriceMin.Value);
        }

        if (request.PriceMax.HasValue)
        {
            query = query.Where(s => s.Price <= request.PriceMax.Value);
        }

        // Filter by active status
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryFiltered = ServiceCategoryHelper.FilterByCategory(query, request.Category);
            query = categoryFiltered.AsQueryable();
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var totalItems = query.Count();
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 10;
        
        // Apply pagination
        var paginatedServices = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new SearchServiceResponse
        {
            Success = true,
            Message = "Service search completed successfully",
            Data = paginatedServices,
            Count = totalItems,
            Filters = request,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }

    /// <summary>
    /// Apply sorting to services query
    /// </summary>
    private static IQueryable<Service> ApplySorting(IQueryable<Service> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderBy(s => s.Name);

        var isDescending = !string.IsNullOrWhiteSpace(sortDirection) && 
                          sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "price" => isDescending ? query.OrderByDescending(s => s.Price) : query.OrderBy(s => s.Price),
            "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(s => s.UpdatedAt) : query.OrderBy(s => s.UpdatedAt),
            "isactive" => isDescending ? query.OrderByDescending(s => s.IsActive) : query.OrderBy(s => s.IsActive),
            _ => query.OrderBy(s => s.Name)
        };
    }

    /// <summary>
    /// Get search suggestions based on partial keyword
    /// </summary>
    public static IEnumerable<string> GetSearchSuggestions(IEnumerable<Service> services, string partialKeyword, int maxSuggestions = 5)
    {
        if (string.IsNullOrWhiteSpace(partialKeyword))
            return [];

        var suggestions = services
            .Where(s => s.Name.Contains(partialKeyword, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Name)
            .Distinct()
            .Take(maxSuggestions)
            .ToList();

        return suggestions;
    }
} 