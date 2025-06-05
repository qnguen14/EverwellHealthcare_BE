using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;

namespace Everwell.BLL.Services.Implements;

public class STITestingService : BaseService<STITestingService>, ISTITestingService
{
    public STITestingService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<STITestingService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<STITesting>> GetAllSTITestingsAsync()
    {
        try
        {
            var stiTestings = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    include: s => s.Include(sti => sti.Customer)
                                   .Include(sti => sti.Appointment)
                                   .Include(sti => sti.TestResults));
            
            return stiTestings ?? new List<STITesting>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all STI testings");
            throw;
        }
    }

    public async Task<STITesting?> GetSTITestingByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<STITesting>()
                .FirstOrDefaultAsync(
                    predicate: s => s.Id == id,
                    include: s => s.Include(sti => sti.Customer)
                                   .Include(sti => sti.Appointment)
                                   .Include(sti => sti.TestResults));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting STI testing by id: {Id}", id);
            throw;
        }
    }

    public async Task<STITesting> CreateSTITestingAsync(STITesting stiTesting)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                stiTesting.Id = Guid.NewGuid();
                
                await _unitOfWork.GetRepository<STITesting>().InsertAsync(stiTesting);
                return stiTesting;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating STI testing");
            throw;
        }
    }

    public async Task<STITesting?> UpdateSTITestingAsync(Guid id, STITesting stiTesting)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingSTITesting = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(predicate: s => s.Id == id);
                
                if (existingSTITesting == null) return null;

                existingSTITesting.TestType = stiTesting.TestType;
                existingSTITesting.Method = stiTesting.Method;
                existingSTITesting.Status = stiTesting.Status;
                existingSTITesting.CollectedDate = stiTesting.CollectedDate;
                
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(existingSTITesting);
                return existingSTITesting;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating STI testing with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteSTITestingAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var stiTesting = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(predicate: s => s.Id == id);
                
                if (stiTesting == null) return false;

                _unitOfWork.GetRepository<STITesting>().DeleteAsync(stiTesting);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting STI testing with id: {Id}", id);
            throw;
        }
    }
} 