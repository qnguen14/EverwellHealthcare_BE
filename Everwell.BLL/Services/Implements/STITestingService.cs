using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Everwell.DAL.Data.Requests.STITests;
using Everwell.DAL.Data.Responses.STITests;

namespace Everwell.BLL.Services.Implements;

public class STITestingService : BaseService<STITestingService>, ISTITestingService
{
    public STITestingService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<STITestingService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<CreateSTITestResponse>> GetAllSTITestingsAsync()
    {
        try
        {
            var stiTestings = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    predicate:s => s.Appointment.Customer.IsActive == true &&
                                             s.Appointment.Consultant.IsActive == true,
                    include: s => s.Include(sti => sti.Appointment)
                                                    .Include(sti => sti.TestResults)
                                                    .Include(sti => sti.Appointment.Customer)
                                                    .Include(sti => sti.Appointment.Consultant));

            if (stiTestings == null)
            {
                _logger.LogError("There are no STI tests available");
            }
            
            return _mapper.Map<IEnumerable<CreateSTITestResponse>>(stiTestings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all STI testings");
            throw;
        }
    }

    public async Task<CreateSTITestResponse> GetSTITestingByIdAsync(Guid id)
    {
        try
        {
            var stitest = await _unitOfWork.GetRepository<STITesting>()
                .FirstOrDefaultAsync(
                    predicate: s => s.Id == id && 
                                    s.Appointment.Customer.IsActive == true &&
                                   s.Appointment.Consultant.IsActive == true,
                    include: s => s.Include(sti => sti.Appointment)
                                   .Include(sti => sti.TestResults)
                                   .Include(sti => sti.Appointment.Customer)
                                   .Include(sti => sti.Appointment.Consultant));
            
            if (stitest == null)
            {
                _logger.LogWarning("STI testing with id {Id} not found", id);
                return null;
            }
            
            return _mapper.Map<CreateSTITestResponse>(stitest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting STI testing by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateSTITestResponse> CreateSTITestingAsync(CreateSTITestRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (request == null)
                {
                    _logger.LogError("Create request is null");
                    throw new ArgumentNullException(nameof(request), "Request cannot be null");
                }
                
                var existingSTITest = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(predicate: s => s.AppointmentId == request.AppointmentId &&
                                                         s.TestType == request.TestType &&
                                                         s.Method == request.Method,
                                         include: s => s.Include(sti => sti.Appointment));
                
                if (existingSTITest != null && existingSTITest.Status != Enum.Parse<Status>("Completed"))
                {
                    _logger.LogWarning("An STI testing with the same appointment and test type already exists");
                }
                
                var newSTITest = _mapper.Map<STITesting>(request);
                
                await _unitOfWork.GetRepository<STITesting>().InsertAsync(newSTITest);
                return _mapper.Map<CreateSTITestResponse>(newSTITest);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating STI testing");
            throw;
        }
    }

    public async Task<CreateSTITestResponse> UpdateSTITestingAsync(Guid id, CreateSTITestRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingSTITest = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(predicate: s => s.AppointmentId == request.AppointmentId &&
                                                         s.TestType == request.TestType &&
                                                         s.Method == request.Method,
                        include: s => s.Include(sti => sti.Appointment));
                
                if (existingSTITest == null )
                {
                    _logger.LogWarning("STI testing with id {Id} not found", id);
                    throw new KeyNotFoundException($"STI testing with id {id} not found");
                }
                
                existingSTITest.TestType = request.TestType;
                existingSTITest.Method = request.Method;
                existingSTITest.Status = request.Status;
                existingSTITest.CollectedDate = request.CollectedDate;
                
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(existingSTITest);
                return _mapper.Map<CreateSTITestResponse>(existingSTITest);
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
                var existingSTITest = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(predicate: s => s.Id == id,
                        include: s => s.Include(sti => sti.Appointment));
                
                if (existingSTITest == null )
                {
                    _logger.LogWarning("STI testing with id {Id} not found", id);
                    throw new KeyNotFoundException($"STI testing with id {id} not found");
                }

                _unitOfWork.GetRepository<STITesting>().DeleteAsync(existingSTITest);
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