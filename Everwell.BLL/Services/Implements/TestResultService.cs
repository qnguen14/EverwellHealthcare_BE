using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;

namespace Everwell.BLL.Services.Implements;

public class TestResultService : BaseService<TestResultService>, ITestResultService
{
    public TestResultService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<TestResultService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<TestResult>> GetAllTestResultsAsync()
    {
        try
        {
            var testResults = await _unitOfWork.GetRepository<TestResult>()
                .GetListAsync(
                    include: t => t.Include(tr => tr.STITesting)
                                   .Include(tr => tr.Customer)
                                   .Include(tr => tr.Staff));
            
            return testResults ?? new List<TestResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all test results");
            throw;
        }
    }

    public async Task<TestResult?> GetTestResultByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<TestResult>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == id,
                    include: t => t.Include(tr => tr.STITesting)
                                   .Include(tr => tr.Customer)
                                   .Include(tr => tr.Staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting test result by id: {Id}", id);
            throw;
        }
    }

    public async Task<TestResult> CreateTestResultAsync(TestResult testResult)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                testResult.Id = Guid.NewGuid();
                
                await _unitOfWork.GetRepository<TestResult>().InsertAsync(testResult);
                return testResult;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating test result");
            throw;
        }
    }

    public async Task<TestResult?> UpdateTestResultAsync(Guid id, TestResult testResult)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingTestResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(predicate: t => t.Id == id);
                
                if (existingTestResult == null) return null;

                existingTestResult.ResultData = testResult.ResultData;
                existingTestResult.Status = testResult.Status;
                existingTestResult.ExaminedAt = testResult.ExaminedAt;
                existingTestResult.SentAt = testResult.SentAt;
                
                _unitOfWork.GetRepository<TestResult>().UpdateAsync(existingTestResult);
                return existingTestResult;
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
                var testResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(predicate: t => t.Id == id);
                
                if (testResult == null) return false;

                _unitOfWork.GetRepository<TestResult>().DeleteAsync(testResult);
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