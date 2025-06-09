namespace Everwell.API.Models.Responses;

/// <summary>
/// Response model for service statistics
/// </summary>
public class ServiceStatisticsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ServiceStatistics? Data { get; set; }
}

/// <summary>
/// Service statistics data
/// </summary>
public class ServiceStatistics
{
    public int TotalServices { get; set; }
    public int ActiveServices { get; set; }
    public int InactiveServices { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public Dictionary<string, int> PriceRanges { get; set; } = new();
    public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
} 