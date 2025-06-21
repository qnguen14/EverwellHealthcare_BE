using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Everwell.DAL.Data.Requests.STITests;
using Everwell.DAL.Data.Responses.STITests;
using Everwell.DAL.Data.Requests.Notifications;
using Everwell.DAL.Data.Requests.TestResult;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class STITestingService : BaseService<STITestingService>, ISTITestingService
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly ITestResultService _testResultService;
    
    public STITestingService(
        IUnitOfWork<EverwellDbContext> unitOfWork, 
        ILogger<STITestingService> logger, 
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        INotificationService notificationService,
        IUserService userService,
        ITestResultService testResultService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _notificationService = notificationService;
        _userService = userService;
        _testResultService = testResultService;
    }

    public async Task<IEnumerable<CreateSTITestResponse>> GetAllSTITestingsAsync()
    {
        try
        {
            var stiTestings = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    predicate: null,
                    include: s => s
                        .Include(sti => sti.Customer)
                        .Include(sti => sti.TestResults)
                );
            if (stiTestings == null || !stiTestings.Any())
            {
                _logger.LogWarning("No STI tests found");
                return Enumerable.Empty<CreateSTITestResponse>();
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
                    predicate: s => s.Id == id,
                    include: s => s.Include(sti => sti.Customer)
                                 .Include(sti => sti.TestResults)
                );
            
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

    public async Task<IEnumerable<CreateSTITestResponse>> GetCurrentUserSTITests()
    {
        try
        {
            var customerId = GetCurrentUserId();
            
            if (customerId == Guid.Empty)
            {
                _logger.LogError("Customer ID is empty");
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
            }

            var stiTests = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    predicate: s => s.CustomerId == customerId,
                    include: s => s.Include(sti => sti.Customer)
                                 .Include(sti => sti.TestResults)
                );
            
            if (stiTests == null)
            {
                _logger.LogWarning("STI tests with customer {Id} not found", customerId);
                return null;
            }

            return _mapper.Map<IEnumerable<CreateSTITestResponse>>(stiTests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user's STI tests");
            throw;
        }
    }

    public async Task<IEnumerable<CreateSTITestResponse>> GetSTITestsByCustomer(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
            {
                _logger.LogError("Customer ID is empty");
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
            }

            var stiTests = await _unitOfWork.GetRepository<STITesting>()
                .GetListAsync(
                    predicate: s => s.CustomerId == customerId,
                    include: s => s.Include(sti => sti.Customer)
                                 .Include(sti => sti.TestResults)
                );

            if (stiTests == null)
            {
                _logger.LogWarning("No STI tests with customer ID: {ID} found", customerId);
                return null;
            }

            return _mapper.Map<IEnumerable<CreateSTITestResponse>>(stiTests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting STI tests for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CreateSTITestResponse> CreateSTITestingAsync(CreateSTITestRequest request)
{
    try
    {
        STITesting newSTITest = null;

        var currentUserId = GetCurrentUserId();

        // Execute database operations in transaction
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (request == null)
            {
                _logger.LogError("Create request is null");
                throw new ArgumentNullException(nameof(request), "Request cannot be null");
            }

            // Check for duplicate test on same day and slot
            var existingTest = await _unitOfWork.GetRepository<STITesting>()
                .FirstOrDefaultAsync(
                    predicate: s => s.CustomerId == currentUserId &&
                                  s.ScheduleDate == request.ScheduleDate &&
                                  s.Slot == request.Slot &&
                                  s.Status != TestingStatus.Cancelled,
                    include: s => s.Include(sti => sti.Customer)
                );

            if (existingTest != null)
            {
                _logger.LogWarning("A similar STI test is already scheduled for this time slot");
                throw new InvalidOperationException("A similar STI test is already scheduled for this time slot");
            }

            // Set price based on package
            decimal price = request.TestPackage switch
            {
                TestPackage.Basic => 300000,
                TestPackage.Advanced => 550000,
                TestPackage.Custom => 330000, // Default to advanced price for custom
                _ => 550000
            };
 
            // Create new STI test
            newSTITest = _mapper.Map<STITesting>(request);
            
            newSTITest.Id = Guid.NewGuid();
            newSTITest.CustomerId = currentUserId;
            newSTITest.Status = TestingStatus.Scheduled;
            newSTITest.TotalPrice = price;
            newSTITest.IsPaid = false;
            newSTITest.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<STITesting>().InsertAsync(newSTITest);
            // Return true to commit the transaction
            return true;
        });

        // Create notification outside the transaction
        if (newSTITest != null)
        {
            // Create corresponding TestResult records based on the package
            await CreateTestResultsForPackage(newSTITest.Id, request.TestPackage, request.CustomParameters);
            
            
            var customer = await _unitOfWork.GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.Id == currentUserId);

            await CreateSTITestBookingNotification(customer.Id, newSTITest.Id, request.ScheduleDate, request.TestPackage);
        }
        
        // Get the complete STI test with all test results
        var completeSTITest = await _unitOfWork.GetRepository<STITesting>()
            .FirstOrDefaultAsync(
                predicate: s => s.Id == newSTITest.Id,
                include: s => s.Include(sti => sti.Customer)
                    .Include(sti => sti.TestResults)
            );

        return _mapper.Map<CreateSTITestResponse>(completeSTITest);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while creating STI testing");
        throw;
    }
}

    public async Task<CreateSTITestResponse> UpdateSTITestingAsync(Guid id, UpdateSTITestRequest request)
    {
        try
        {
            STITesting existingSTITest = null;
            TestingStatus? previousStatus = null;
            bool? isPaid = null;
            
            var currentUserId = GetCurrentUserId();
                
            if (currentUserId == null)
            {
                _logger.LogError("Current user is not authenticated");
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            
            // Execute database operations in transaction
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                existingSTITest = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(
                        predicate: s => s.Id == id,
                        include: s => s.Include(sti => sti.Customer)
                    );
                
                if (existingSTITest == null)
                {
                    _logger.LogWarning("STI testing with id {Id} not found", id);
                    throw new KeyNotFoundException($"STI testing with id {id} not found");
                }
                
                // Store previous status for later notification decisions
                previousStatus = existingSTITest.Status;
                
                // Update only allowed fields based on current status
                if (request.Status.HasValue)
                {
                    existingSTITest.Status = request.Status;
                    
                    // If sample was taken, record the time
                    if (request.Status == TestingStatus.SampleTaken)
                    {
                        existingSTITest.SampleTakenAt = DateTime.UtcNow;
                    }
                    
                    // If test was completed, record the time
                    if (request.Status == TestingStatus.Completed)
                    {
                        existingSTITest.CompletedAt = DateTime.UtcNow;
                    }
                }
                
                if (request.Notes != null)
                {
                    existingSTITest.Notes = request.Notes;
                }
                
                if (request.IsPaid.HasValue)
                {
                    existingSTITest.IsPaid = request.IsPaid.Value;
                }
                
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(existingSTITest);
                return true;
            });
            
            // Send notifications outside the transaction
            if (existingSTITest != null)
            {
                // If status changed to SampleTaken
                if (previousStatus != TestingStatus.SampleTaken && 
                    existingSTITest.Status == TestingStatus.SampleTaken)
                {
                    await CreateSampleCollectedNotification(existingSTITest.CustomerId, existingSTITest.Id);
                }
                
                // If status changed to Completed
                if (previousStatus != TestingStatus.Completed && 
                    existingSTITest.Status == TestingStatus.Completed)
                {
                    await CreateTestCompletedNotification(existingSTITest.CustomerId, existingSTITest.Id);
                }
                
                // If payment status changed to paid
                if (existingSTITest.IsPaid)
                {
                    await CreatePaymentConfirmationNotification(existingSTITest.CustomerId, existingSTITest.Id);
                }
            }
            
            return _mapper.Map<CreateSTITestResponse>(existingSTITest);
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
            STITesting existingSTITest = null;
            
            // Execute database operations in transaction
            bool result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                existingSTITest = await _unitOfWork.GetRepository<STITesting>()
                    .FirstOrDefaultAsync(
                        predicate: s => s.Id == id,
                        include: s => s.Include(sti => sti.TestResults)
                    );
                
                if (existingSTITest == null)
                {
                    _logger.LogWarning("STI testing with id {Id} not found", id);
                    throw new KeyNotFoundException($"STI testing with id {id} not found");
                }

                // Instead of hard delete, cancel the test
                existingSTITest.Status = TestingStatus.Cancelled;
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(existingSTITest);
                
                return true;
            });
            
            // Send cancellation notification outside the transaction
            if (existingSTITest != null)
            {
                await CreateCancellationNotification(existingSTITest.CustomerId, existingSTITest.Id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting STI testing with id: {Id}", id);
            throw;
        }
    }

    #region Test Package Methods
     
    private async Task CreateTestResultsForPackage(Guid stiTestingId, TestPackage package, List<TestParameter> customParameters = null)
    {
        // Define which parameters to test based on the package
        List<TestParameter> parametersToTest = new List<TestParameter>();
        
        // Basic package includes these tests
        if (package == TestPackage.Basic)
        {
            parametersToTest.AddRange(new[]
            {
                TestParameter.Chlamydia,
                TestParameter.Gonorrhoeae,
                TestParameter.Syphilis
            });
        }
        // Advanced package includes all tests
        else if (package == TestPackage.Advanced)
        {
            parametersToTest.AddRange(new[]
            {
                TestParameter.Chlamydia,
                TestParameter.Gonorrhoeae,
                TestParameter.Syphilis,
                TestParameter.HIV,
                TestParameter.HPV,
                TestParameter.Herpes,
                TestParameter.HepatitisB,
                TestParameter.HepatitisC,
                TestParameter.Trichomonas,
                TestParameter.MycoplasmaGenitalium
            });
        }
        // Custom package would need additional logic to determine which tests to include
        else if (package == TestPackage.Custom)
        {
            if (customParameters != null && customParameters.Count >= 3)
            {
                parametersToTest.AddRange(customParameters);
            }
            else
            {
                _logger.LogWarning("Custom package requires at least 3 parameters, using basic package tests");
                parametersToTest.AddRange(new[]
                {
                    TestParameter.Chlamydia,
                    TestParameter.Gonorrhoeae,
                    TestParameter.Syphilis
                });
            }
        }

        // Create a TestResult for each parameter
        foreach (var parameter in parametersToTest)
        {
            var createRequest = new CreateTestResultRequest()
            {
                STITestingId = stiTestingId,
                Parameter = parameter,
                Comments = null,
                StaffId = Guid.Empty,
                ProcessedAt = null, // ProcessedAt will be set when the test is processed
            };

            await _testResultService.CreateTestResultAsync(createRequest);
        }
    }
        

    #endregion

    #region Notification Methods
    private async Task CreateSTITestBookingNotification(Guid customerId, Guid stiTestingId, DateOnly scheduledDate, TestPackage testPackage)
    {
        string packageName = testPackage == TestPackage.Basic ? "Cơ bản" : "Nâng cao";
        
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Đặt lịch xét nghiệm STI thành công",
            Message = $"Bạn đã đặt lịch xét nghiệm STI gói {packageName} vào ngày {scheduledDate:dd/MM/yyyy}. Vui lòng đến đúng giờ.",
            Type = NotificationType.Appointment,
            Priority = NotificationPriority.Medium,
            AppointmentId = null,
            TestResultId = null,
            STITestingId = stiTestingId
        };
        
        await _notificationService.CreateNotification(notificationRequest);
    }
    
    private async Task CreateSampleCollectedNotification(Guid customerId, Guid stiTestingId)
    {
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Mẫu xét nghiệm đã được thu thập",
            Message = "Mẫu xét nghiệm của bạn đã được thu thập thành công và đang được xử lý. Kết quả sẽ được thông báo cho bạn trong thời gian sớm nhất.",
            Type = NotificationType.TestResult,
            Priority = NotificationPriority.Medium,
            AppointmentId = null,
            TestResultId = null,
            STITestingId = stiTestingId
        };
        
        await _notificationService.CreateNotification(notificationRequest);
    }
    
    private async Task CreateTestCompletedNotification(Guid customerId, Guid stiTestingId)
    {
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Kết quả xét nghiệm đã có",
            Message = "Kết quả xét nghiệm STI của bạn đã sẵn sàng. Vui lòng kiểm tra trong hồ sơ cá nhân của bạn.",
            Type = NotificationType.TestResult,
            Priority = NotificationPriority.High,
            AppointmentId = null,
            TestResultId = null,
            STITestingId = stiTestingId
        };
        
        await _notificationService.CreateNotification(notificationRequest);
    }
    
    private async Task CreatePaymentConfirmationNotification(Guid customerId, Guid stiTestingId)
    {
        var stiTesting = await _unitOfWork.GetRepository<STITesting>()
            .FirstOrDefaultAsync(predicate: s => s.Id == stiTestingId);
            
        if (stiTesting == null) return;
        
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Thanh toán hóa đơn dịch vụ",
            Message = $"Thanh toán thành công hóa đơn dịch vụ xét nghiệm STI với số tiền {stiTesting.TotalPrice:N0} đồng. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.",
            Type = NotificationType.Payment,
            Priority = NotificationPriority.Medium,
            AppointmentId = null,
            TestResultId = null,
            STITestingId = stiTestingId
        };
        
        await _notificationService.CreateNotification(notificationRequest);
    }
    
    private async Task CreateCancellationNotification(Guid customerId, Guid stiTestingId)
    {
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = customerId,
            Title = "Hủy lịch xét nghiệm STI",
            Message = "Lịch xét nghiệm STI của bạn đã được hủy thành công. Nếu bạn muốn đặt lại lịch, vui lòng liên hệ với chúng tôi.",
            Type = NotificationType.STITest,
            Priority = NotificationPriority.Medium,
            AppointmentId = null,
            TestResultId = null,
            STITestingId = stiTestingId
        };
        
        await _notificationService.CreateNotification(notificationRequest);
    }
    #endregion
}