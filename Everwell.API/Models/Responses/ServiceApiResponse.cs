namespace Everwell.API.Models.Responses;

/// <summary>
/// Generic API response wrapper for service operations
/// </summary>
public class ServiceApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int? Count { get; set; }
    public List<string>? Errors { get; set; }
    public object? Metadata { get; set; }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ServiceApiResponse<T> CreateSuccess(string message, T? data = default, int? count = null, object? metadata = null)
    {
        return new ServiceApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Count = count,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Create an error response with details
    /// </summary>
    public static ServiceApiResponse<T> CreateError(string message, string? details = null)
    {
        var response = new ServiceApiResponse<T>
        {
            Success = false,
            Message = message
        };

        if (!string.IsNullOrEmpty(details))
        {
            response.Errors = new List<string> { details };
        }

        return response;
    }

    /// <summary>
    /// Create an error response with multiple errors
    /// </summary>
    public static ServiceApiResponse<T> CreateError(string message, List<string> errors)
    {
        return new ServiceApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    /// <summary>
    /// Create a validation error response
    /// </summary>
    public static ServiceApiResponse<T> CreateValidationError(List<string> validationErrors)
    {
        return new ServiceApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = validationErrors
        };
    }
} 