using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;

namespace Everwell.BLL.Services.Implements;

public class ServiceService : BaseService<ServiceService>, IServiceService
{
    public ServiceService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<ServiceService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<Service>> GetAllServicesAsync()
    {
        try
        {
            var services = await _unitOfWork.GetRepository<Service>()
                .GetListAsync(predicate: s => s.IsActive);
            
            return services ?? new List<Service>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all services");
            throw;
        }
    }

    public async Task<Service?> GetServiceByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<Service>()
                .FirstOrDefaultAsync(predicate: s => s.Id == id && s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service by id: {Id}", id);
            throw;
        }
    }

    public async Task<Service> CreateServiceAsync(Service service)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                service.Id = Guid.NewGuid();
                service.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                service.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);
                
                await _unitOfWork.GetRepository<Service>().InsertAsync(service);
                return service;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating service");
            throw;
        }
    }

    public async Task<Service?> UpdateServiceAsync(Guid id, Service service)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingService = await _unitOfWork.GetRepository<Service>()
                    .FirstOrDefaultAsync(predicate: s => s.Id == id && s.IsActive);
                
                if (existingService == null) return null;

                existingService.Name = service.Name;
                existingService.Description = service.Description;
                existingService.Price = service.Price;
                existingService.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);
                
                _unitOfWork.GetRepository<Service>().UpdateAsync(existingService);
                return existingService;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating service with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteServiceAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var service = await _unitOfWork.GetRepository<Service>()
                    .FirstOrDefaultAsync(predicate: s => s.Id == id && s.IsActive);
                
                if (service == null) return false;

                service.IsActive = false;
                service.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);
                _unitOfWork.GetRepository<Service>().UpdateAsync(service);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting service with id: {Id}", id);
            throw;
        }
    }
} 