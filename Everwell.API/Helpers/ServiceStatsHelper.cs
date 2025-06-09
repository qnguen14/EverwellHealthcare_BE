using Everwell.API.Models.Responses;
using Everwell.DAL.Data.Entities;

namespace Everwell.API.Helpers;

/// <summary>
/// Helper class for generating service statistics
/// </summary>
public static class ServiceStatsHelper
{
    /// <summary>
    /// Get service statistics
    /// </summary>
    public static ServiceStatistics GetServiceStatistics(IEnumerable<Service> services)
    {
        var serviceList = services.ToList();
        
        return new ServiceStatistics
        {
            TotalServices = serviceList.Count,
            ActiveServices = serviceList.Count(s => s.IsActive),
            InactiveServices = serviceList.Count(s => !s.IsActive),
            AveragePrice = serviceList.Any() ? serviceList.Average(s => s.Price) : 0,
            MinPrice = serviceList.Any() ? serviceList.Min(s => s.Price) : 0,
            MaxPrice = serviceList.Any() ? serviceList.Max(s => s.Price) : 0,
            PriceRanges = GetPriceRangeBreakdown(serviceList),
            CategoryBreakdown = GetCategoryBreakdown(serviceList)
        };
    }

    /// <summary>
    /// Get price range breakdown
    /// </summary>
    private static Dictionary<string, int> GetPriceRangeBreakdown(IEnumerable<Service> services)
    {
        var serviceList = services.ToList();
        
        return new Dictionary<string, int>
        {
            ["Under $50"] = serviceList.Count(s => s.Price < 50),
            ["$50 - $100"] = serviceList.Count(s => s.Price >= 50 && s.Price < 100),
            ["$100 - $200"] = serviceList.Count(s => s.Price >= 100 && s.Price < 200),
            ["$200 - $500"] = serviceList.Count(s => s.Price >= 200 && s.Price < 500),
            ["$500+"] = serviceList.Count(s => s.Price >= 500)
        };
    }

    /// <summary>
    /// Get category breakdown
    /// </summary>
    private static Dictionary<string, int> GetCategoryBreakdown(IEnumerable<Service> services)
    {
        var categories = new[]
        {
            "test", "polyclinic", "vaccination", "home", "women", 
            "men", "pregnancy", "std", "cancer", "cardiology", "diabetes"
        };
        
        var breakdown = new Dictionary<string, int>();

        foreach (var category in categories)
        {
            var count = ServiceCategoryHelper.FilterByCategory(services, category).Count();
            breakdown[category] = count;
        }

        return breakdown;
    }
} 