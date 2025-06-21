using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Everwell.DAL.Data.Responses.TestResult;
using Everwell.DAL.Data.Requests.TestResult;
using Everwell.DAL.Data.Requests.Notifications;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class TestResultService : BaseService<TestResultService>, ITestResultService
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    public TestResultService(
        IUnitOfWork<EverwellDbContext> unitOfWork,
        ILogger<TestResultService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        INotificationService notificationService,
        IUserService userService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _notificationService = notificationService;
        _userService = userService;

    }

    public async Task<IEnumerable<CreateTestResultResponse>> GetAllTestResultsAsync()
    {
        try
        {
            var testResults = await _unitOfWork.GetRepository<TestResult>()
                .GetListAsync(
                    predicate: t => t.STITesting.Customer.IsActive == true,
                    include: t => t.Include(tr => tr.STITesting)
                                  .Include(tr => tr.STITesting.Customer)
                                  .Include(tr => tr.Staff));  

            if (testResults == null || !testResults.Any())
            {
                _logger.LogWarning("Không tìm thấy kết quả nào trong hệ thống.");
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

    public async Task<IEnumerable<CreateTestResultResponse>> GetTestResultsBySTITestingIdAsync(Guid stiTestingId)
    {
        try
        {
            var testResults = await _unitOfWork.GetRepository<TestResult>()
                .GetListAsync(
                    predicate: t => t.STITestingId == stiTestingId && 
                                   t.STITesting.Customer.IsActive == true,
                    include: t => t.Include(tr => tr.STITesting)
                                  .Include(tr => tr.STITesting.Customer)
                                  .Include(tr => tr.Staff));

            if (testResults == null || !testResults.Any())
            {
                _logger.LogWarning("Không tìm thấy kết quả nào cho STI Test với ID {StiTestingId}", stiTestingId);
                return Enumerable.Empty<CreateTestResultResponse>();
            }

            return _mapper.Map<IEnumerable<CreateTestResultResponse>>(testResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting test results for STI Testing with ID {StiTestingId}", stiTestingId);
            throw;
        }
    }

    public async Task<CreateTestResultResponse> GetTestResultByIdAsync(Guid id)
    {
        try
        {
            var testresult = await _unitOfWork.GetRepository<TestResult>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == id,
                    include: t => t.Include(tr => tr.STITesting)
                                  .Include(tr => tr.STITesting.Customer)
                                  .Include(tr => tr.Staff));

            if (testresult == null) 
            {
                _logger.LogWarning("Kết quả với ID: {Id} not found", id);
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
    
        public async Task<IEnumerable<CreateTestResultResponse>> GetTestResultByCustomerAsync(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Khách hang không hợp lệ: {CustomerId}", customerId);
                return null;
            }

            var customer = await _userService.GetUserById(customerId);
            if (customer.Role != "Customer")
            {
                _logger.LogWarning("Tài khoản với ID: {CustomerId} không phải là khách hàng", customerId);
                return null;
            }

            var testresult = await _unitOfWork.GetRepository<TestResult>()
                .GetListAsync(
                    predicate: t => t.STITesting.CustomerId == customerId && 
                                   t.STITesting.Customer.IsActive == true,
                    include: t => t.Include(tr => tr.STITesting)
                                  .Include(tr => tr.STITesting.Customer)
                                  .Include(tr => tr.Staff));

            if (testresult == null) 
            {
                _logger.LogWarning("Test result with customer id {Id} not found", customerId);
                return null;
            }

            return _mapper.Map<IEnumerable<CreateTestResultResponse>>(testresult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting test result by id: {Id}", customerId);
            throw;
        }
    }

    public async Task<CreateTestResultResponse> CreateTestResultAsync(CreateTestResultRequest request)
    {
        try
        {
            TestResult testResult = null;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Validate STI Testing exists
                var stiTesting = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(
                        predicate: s => s.Id == request.STITestingId,
                        include: s => s.Include(sti => sti.Customer)
                    );


                if (stiTesting == null)
                {
                    _logger.LogWarning("STI Testing with id {STITestingId} not found", request.STITestingId);
                    throw new KeyNotFoundException($"STI Testing with id {request.STITestingId} not found");
                }


                // Create test result
                testResult = _mapper.Map<TestResult>(request);
                testResult.Outcome = ResultOutcome.Pending;

                await _unitOfWork.GetRepository<TestResult>().InsertAsync(testResult);

                return _mapper.Map<CreateTestResultResponse>(testResult);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating test result");
            throw;
        }
    }

    public async Task<CreateTestResultResponse> UpdateTestResultAsync(Guid id, UpdateTestResultRequest request)
    {
        try
        {
            TestResult existingTestResult = null;
            ResultOutcome? previousOutcome = null;


            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                existingTestResult = await _unitOfWork.GetRepository<TestResult>()
                    .FirstOrDefaultAsync(
                        predicate: t => t.Id == id,
                        include: t => t.Include(tr => tr.STITesting)
                            .Include(tr => tr.STITesting.Customer)
                            .Include(tr => tr.Staff));

                if (existingTestResult == null)
                {
                    _logger.LogWarning("Test result with id {Id} not found", id);
                    throw new KeyNotFoundException($"Test result with id {id} not found");
                }

                previousOutcome = existingTestResult.Outcome;

                if (request.Comments != null)
                {
                    existingTestResult.Comments = request.Comments;
                }

                if (request.StaffId.HasValue)
                {
                    existingTestResult.StaffId = request.StaffId.Value;
                }

                if (request.Outcome.HasValue)
                {
                    existingTestResult.Outcome = request.Outcome;

                    if (request.Outcome == ResultOutcome.Positive || request.Outcome == ResultOutcome.Negative)
                    {
                        existingTestResult.ProcessedAt = DateTime.UtcNow;
                    }
                }

                _unitOfWork.GetRepository<TestResult>().UpdateAsync(existingTestResult);
                return true;
            });

            // Check if the outcome has changed
            if (existingTestResult != null)
            {
                if (previousOutcome != existingTestResult.Outcome)
                {
                    // Check if all test results for this STI testing are complete
                    await CheckAndUpdateStiTestingStatus(existingTestResult.STITestingId);
                    
                    // If the outcome is positive, create a notification
                    if (existingTestResult.Outcome == ResultOutcome.Positive)
                    {
                        await CreatePositiveResultNotification(existingTestResult.STITesting.CustomerId, existingTestResult.Id);
                    }
                    else if (existingTestResult.Outcome == ResultOutcome.Negative)
                    {
                        await CreateNegativeResultNotification(existingTestResult.STITesting.CustomerId, existingTestResult.Id);
                    }
                }
            }
                
            return _mapper.Map<CreateTestResultResponse>(existingTestResult);
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
                    .FirstOrDefaultAsync(predicate: t => t.Id == id);

                if (existingTestResult == null)
                {
                    _logger.LogWarning("Test result with id {Id} not found", id);
                    return false;
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

    #region Helper Methods
    private async Task CheckAndUpdateStiTestingStatus(Guid stiTestingId)
    {
        // Get all test results for this STI testing
        var testResults = await _unitOfWork.GetRepository<TestResult>()
            .GetListAsync(predicate: tr => tr.STITestingId == stiTestingId);
        
        // Get the STI testing record
        var stiTesting = await _unitOfWork.GetRepository<STITesting>()
            .FirstOrDefaultAsync(predicate: s => s.Id == stiTestingId);
            
        if (stiTesting == null) return;
        
        // If no pending results and at least one result exists, mark as completed
        if (testResults.Any() && !testResults.Any(tr => tr.Outcome == ResultOutcome.Pending))
        {
            stiTesting.Status = TestingStatus.Completed;
            stiTesting.CompletedAt = DateTime.UtcNow;
            
            _unitOfWork.GetRepository<STITesting>().UpdateAsync(stiTesting);
            
            // Send notification
            var request = new CreateNotificationRequest
            {
                UserId = stiTesting.CustomerId,
                Title = "Kết quả xét nghiệm đã hoàn thành",
                Message = "Tất cả kết quả xét nghiệm STI của bạn đã hoàn tất. Vui lòng xem chi tiết trong hồ sơ cá nhân.",
                Type = NotificationType.TestResult,
                Priority = NotificationPriority.High,
                STITestingId = stiTestingId
            };
            
            await _notificationService.CreateNotification(request);
        }
    }
    
    private async Task CreatePositiveResultNotification(Guid customerId, Guid testResultId)
    {
        // Get the test result to include details in notification
        var testResult = await _unitOfWork.GetRepository<TestResult>()
            .FirstOrDefaultAsync(
                predicate: tr => tr.Id == testResultId,
                include: tr => tr.Include(t => t.STITesting)
            );
            
        if (testResult == null) return;
        
        // Create notification for positive result
        var request = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Kết quả xét nghiệm dương tính",
            Message = $"Kết quả xét nghiệm cho {testResult.Parameter} của bạn cần được theo dõi thêm. Vui lòng liên hệ với bác sĩ để được tư vấn.",
            Type = NotificationType.TestResult,
            Priority = NotificationPriority.High,
            TestResultId = testResultId,
            STITestingId = testResult.STITestingId
        };
        
        await _notificationService.CreateNotification(request);
        
        // Mark notification as sent
        // testResult.NotificationSent = true;
        _unitOfWork.GetRepository<TestResult>().UpdateAsync(testResult);
    }
    
    private async Task CreateNegativeResultNotification(Guid customerId, Guid testResultId)
    {
        // Get the test result to include details in notification
        var testResult = await _unitOfWork.GetRepository<TestResult>()
            .FirstOrDefaultAsync(
                predicate: tr => tr.Id == testResultId,
                include: tr => tr.Include(t => t.STITesting)
            );
            
        if (testResult == null) return;
        
        // Create notification for positive result
        var request = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Kết quả xét nghiệm âm tính",
            Message = $"Kết quả xét nghiệm cho {testResult.Parameter} của bạn cho thấy bạn không nhiễm bệnh STI. Tuy nhiên, nếu bạn có triệu chứng hoặc lo ngại, vui lòng liên hệ với bác sĩ để được tư vấn.",
            Type = NotificationType.TestResult,
            Priority = NotificationPriority.High,
            TestResultId = testResultId,
            STITestingId = testResult.STITestingId
        };
        
        await _notificationService.CreateNotification(request);
        
        // Mark notification as sent
        // testResult.NotificationSent = true;
        _unitOfWork.GetRepository<TestResult>().UpdateAsync(testResult);
    }


    #endregion
}