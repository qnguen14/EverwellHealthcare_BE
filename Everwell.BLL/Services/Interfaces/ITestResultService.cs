 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface ITestResultService
{
    Task<IEnumerable<TestResult>> GetAllTestResultsAsync();
    Task<TestResult?> GetTestResultByIdAsync(Guid id);
    Task<TestResult> CreateTestResultAsync(TestResult testResult);
    Task<TestResult?> UpdateTestResultAsync(Guid id, TestResult testResult);
    Task<bool> DeleteTestResultAsync(Guid id);
}