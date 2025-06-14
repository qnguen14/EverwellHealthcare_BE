using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Everwell.DAL.Data.Responses.TestResult;
using Everwell.DAL.Data.Requests.TestResult;

namespace Everwell.BLL.Services.Implements;

public class TestResultService : BaseService<TestResultService>, ITestResultService
{
    public TestResultService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<TestResultService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<CreateTestResultResponse>> GetAllTestResultsAsync()
    {
        try
        {
            var testResults = await _unitOfWork.GetRepository<TestResult>()
                .GetListAsync(
                    predicate: t => t.Customer.IsActive == true 
                                    && t.Staff.IsActive == true,
                    include: t => t.Include(tr => tr.STITesting)
                                   .Include(tr => tr.Customer)
                                   .Include(tr => tr.Staff));

            if (testResults == null || !testResults.Any())
            {
                _logger.LogWarning("No test results found");
                return Enumerable.Empty<CreateTestResultResponse>();
            }

            return _mapper.Map<IEnumerable<CreateTestResultResponse>>(testResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all test results");
            throw;
        }
    }

    public async Task<CreateTestResultResponse> GetTestResultByIdAsync(Guid id)
    {
        try
        {
            var testresult = await _unitOfWork.GetRepository<TestResult>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == id &&
                                    t.Customer.IsActive == true &&
                                    t.Staff.IsActive == true,
                    include: t => t.Include(tr => tr.STITesting)
                                   .Include(tr => tr.Customer)
                                   .Include(tr => tr.Staff));

            if (testresult == null) 
            {
                _logger.LogWarning("Test result with id {Id} not found", id);
                return null;
            }

            return _mapper.Map<CreateTestResultResponse>(testresult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting test result by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateTestResultResponse> CreateTestResultAsync(CreateTestResultRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {

                var existingTestResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(predicate: t => t.STITestingId == request.STITestingId &&
                                                         t.CustomerId == request.CustomerId &&
                                                         t.StaffId == request.StaffId,
                                         include: t => t.Include(tr => tr.STITesting)
                                                        .Include(tr => tr.Customer)
                                                        .Include(tr => tr.Staff));

                if (existingTestResult != null)
                {
                    _logger.LogWarning("Test result with STITestingId {STITestingId} already exists", request.STITestingId);
                    return _mapper.Map<CreateTestResultResponse>(request);
                }

                await _unitOfWork.GetRepository<TestResult>().InsertAsync(existingTestResult);

                return _mapper.Map<CreateTestResultResponse>(request);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating test result");
            throw;
        }
    }

    public async Task<CreateTestResultResponse> UpdateTestResultAsync(Guid id, CreateTestResultRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingTestResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(predicate: t => t.Id == id &&
                                                         t.Customer.IsActive == true && 
                                                         t.Staff.IsActive == true,
                                         include: t => t.Include(tr => tr.STITesting)
                                                        .Include(tr => tr.Customer)
                                                        .Include(tr => tr.Staff));
                
                if (existingTestResult == null)
                {
                    _logger.LogWarning("Test result with id {Id} not found", id);
                }

                existingTestResult.ResultData = request.ResultData;
                existingTestResult.Status = request.Status;
                existingTestResult.ExaminedAt = request.ExaminedAt;
                existingTestResult.SentAt = request.SentAt;
                
                _unitOfWork.GetRepository<TestResult>().UpdateAsync(existingTestResult);
                return _mapper.Map<CreateTestResultResponse>(request);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating test result with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTestResultAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingTestResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(predicate: t => t.Id == id &&
                                                         t.Customer.IsActive == true &&
                                                         t.Staff.IsActive == true,
                                         include: t => t.Include(tr => tr.STITesting)
                                                        .Include(tr => tr.Customer)
                                                        .Include(tr => tr.Staff));

                if (existingTestResult == null)
                {
                    _logger.LogWarning("Test result with id {Id} not found", id);
                }

                _unitOfWork.GetRepository<TestResult>().DeleteAsync(existingTestResult);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting test result with id: {Id}", id);
            throw;
        }
    }
} 