using Everwell.DAL.Data.Entities;

namespace Everwell.API.Helpers;

/// <summary>
/// Helper class for filtering services by category based on diag.vn service types
/// </summary>
public static class ServiceCategoryHelper
{
    /// <summary>
    /// Get all available service categories
    /// </summary>
    public static Dictionary<string, string[]> GetServiceCategories()
    {
        return new Dictionary<string, string[]>
        {
            ["test"] = ["test", "tests", "laboratory", "lab"],
            ["polyclinic"] = ["polyclinic", "clinic", "consultation"],
            ["vaccination"] = ["vaccination", "vaccine", "immunization"],
            ["home"] = ["home", "collection", "homeboodcollection"],
            ["women"] = ["women", "womens", "gynecology", "obstetrics"],
            ["men"] = ["men", "mens", "andrology", "urology"],
            ["pregnancy"] = ["pregnancy", "prenatal", "maternity", "pregnant"],
            ["std"] = ["std", "sti", "sexually"],
            ["cancer"] = ["cancer", "oncology", "screening"],
            ["cardiology"] = ["cardiology", "heart", "cardiovascular"],
            ["diabetes"] = ["diabetes", "endocrinology", "hormone"]
        };
    }

    /// <summary>
    /// Filter services by category
    /// </summary>
    public static IEnumerable<Service> FilterByCategory(IEnumerable<Service> services, string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return services;

        return category.ToLower() switch
        {
            "test" or "tests" or "laboratory" or "lab" => FilterByKeywords(services, 
                ["test", "lab", "screen", "laboratory", "analysis", "examination"]),
                
            "polyclinic" or "clinic" or "consultation" => FilterByKeywords(services, 
                ["consultation", "clinic", "checkup", "examination", "visit", "appointment"]),
                
            "vaccination" or "vaccine" or "immunization" => FilterByKeywords(services, 
                ["vaccine", "vaccination", "immunization", "shot", "injection"]),
                
            "home" or "collection" or "homeboodcollection" => FilterByKeywords(services, 
                ["home", "collection", "visit", "doorstep", "house call"]),
                
            "women" or "womens" or "gynecology" or "obstetrics" => FilterByKeywords(services, 
                ["women", "gynecology", "obstetrics", "female", "gynecological", "maternal"]),
                
            "men" or "mens" or "andrology" or "urology" => FilterByKeywords(services, 
                ["men", "andrology", "urology", "male", "prostate", "testosterone"]),
                
            "pregnancy" or "prenatal" or "maternity" or "pregnant" => FilterByKeywords(services, 
                ["pregnancy", "prenatal", "maternity", "pregnant", "fetal", "antenatal"]),
                
            "std" or "sti" or "sexually" => FilterByKeywords(services, 
                ["std", "sti", "sexually", "sexually transmitted", "venereal"]),
                
            "cancer" or "oncology" or "screening" => FilterByKeywords(services, 
                ["cancer", "oncology", "screening", "tumor", "malignant", "neoplasm"]),
                
            "cardiology" or "heart" or "cardiovascular" => FilterByKeywords(services, 
                ["cardiology", "heart", "cardiovascular", "cardiac", "ecg", "echo"]),
                
            "diabetes" or "endocrinology" or "hormone" => FilterByKeywords(services, 
                ["diabetes", "endocrinology", "hormone", "thyroid", "hormonal", "insulin"]),
                
            _ => services
        };
    }

    /// <summary>
    /// Filter services by multiple keywords
    /// </summary>
    private static IEnumerable<Service> FilterByKeywords(IEnumerable<Service> services, string[] keywords)
    {
        return services.Where(s => 
            keywords.Any(keyword => 
                s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Get category name by service
    /// </summary>
    public static string? GetCategoryByService(Service service)
    {
        var categories = GetServiceCategories();
        
        foreach (var category in categories)
        {
            var filteredServices = FilterByKeywords([service], category.Value);
            if (filteredServices.Any())
            {
                return category.Key;
            }
        }
        
        return null;
    }
} 