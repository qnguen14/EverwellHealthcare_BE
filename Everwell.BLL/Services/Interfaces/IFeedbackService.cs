using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Feedback;
using Everwell.DAL.Data.Responses.Feedback;

namespace Everwell.BLL.Services.Interfaces;

public interface IFeedbackService
{
    // Legacy methods (keep for backward compatibility)
    Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
    Task<Feedback?> GetFeedbackByIdAsync(Guid id);
    Task<Feedback> CreateFeedbackAsync(Feedback feedback);
    Task<Feedback?> UpdateFeedbackAsync(Guid id, Feedback feedback);
    Task<bool> DeleteFeedbackAsync(Guid id);
    
    // New DTO-based methods
    Task<CreateFeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request);
    Task<FeedbackResponse?> UpdateFeedbackAsync(Guid id, UpdateFeedbackRequest request);
    Task<FeedbackResponse?> GetFeedbackResponseByIdAsync(Guid id);
    Task<IEnumerable<FeedbackResponse>> GetAllFeedbackResponsesAsync();
    
    // User-specific methods
    Task<IEnumerable<FeedbackResponse>> GetFeedbacksByCustomerAsync(Guid? customerId = null); // null = current user
    Task<IEnumerable<FeedbackResponse>> GetFeedbacksByConsultantAsync(Guid? consultantId = null); // null = current user
    Task<FeedbackResponse?> GetFeedbackByAppointmentAsync(Guid appointmentId);
    
    // Validation methods
    Task<bool> CanCustomerProvideFeedbackAsync(Guid appointmentId, Guid? customerId = null);
    Task<bool> HasCustomerAlreadyProvidedFeedbackAsync(Guid appointmentId, Guid? customerId = null);
}