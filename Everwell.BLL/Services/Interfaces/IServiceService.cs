 
 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<Service>> GetAllServicesAsync();
    Task<Service?> GetServiceByIdAsync(Guid id);
    Task<Service> CreateServiceAsync(Service service);
    Task<Service?> UpdateServiceAsync(Guid id, Service service);
    Task<bool> DeleteServiceAsync(Guid id);
}