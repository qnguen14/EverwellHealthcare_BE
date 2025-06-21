 using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Requests.TestResult;
using Everwell.DAL.Data.Responses.TestResult;

namespace Everwell.BLL.Services.Interfaces;

public interface ITestResultService
{
    Task<IEnumerable<CreateTestResultResponse>> GetAllTestResultsAsync();
    Task<CreateTestResultResponse> GetTestResultByIdAsync(Guid id);
    Task<IEnumerable<CreateTestResultResponse>> GetTestResultByCustomerAsync(Guid customerId);
    Task<IEnumerable<CreateTestResultResponse>> GetTestResultsBySTITestingIdAsync(Guid stiTestingId);
    Task<CreateTestResultResponse> CreateTestResultAsync(CreateTestResultRequest request);
    Task<CreateTestResultResponse> UpdateTestResultAsync(Guid id, UpdateTestResultRequest request);
    Task<bool> DeleteTestResultAsync(Guid id);
}