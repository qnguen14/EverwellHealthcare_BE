 using Everwell.DAL.Data.Entities;
 using Everwell.DAL.Data.Requests.STITests;
 using Everwell.DAL.Data.Responses.STITests;

 namespace Everwell.BLL.Services.Interfaces;

public interface ISTITestingService
{
    Task<IEnumerable<CreateSTITestResponse>> GetAllSTITestingsAsync();
    Task<CreateSTITestResponse> GetSTITestingByIdAsync(Guid id);
    Task<IEnumerable<CreateSTITestResponse>> GetCurrentUserSTITests();
    Task<IEnumerable<CreateSTITestResponse>> GetSTITestsByCustomer(Guid customerId);
    Task<CreateSTITestResponse> CreateSTITestingAsync(CreateSTITestRequest request);
    Task<CreateSTITestResponse?> UpdateSTITestingAsync(Guid id, UpdateSTITestRequest request);
    Task<bool> DeleteSTITestingAsync(Guid id);
}