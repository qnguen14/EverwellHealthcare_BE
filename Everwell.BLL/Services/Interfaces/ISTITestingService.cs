 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface ISTITestingService
{
    Task<IEnumerable<STITesting>> GetAllSTITestingsAsync();
    Task<STITesting?> GetSTITestingByIdAsync(Guid id);
    Task<STITesting> CreateSTITestingAsync(STITesting stiTesting);
    Task<STITesting?> UpdateSTITestingAsync(Guid id, STITesting stiTesting);
    Task<bool> DeleteSTITestingAsync(Guid id);
}