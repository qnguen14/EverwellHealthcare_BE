using Everwell.DAL.Data.Requests.Notifications;
using Everwell.DAL.Data.Responses.Notifications;

namespace Everwell.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        Task<GetNotificationResponse> CreateNotification(CreateNotificationRequest request);
        Task<List<GetNotificationResponse>> GetUserNotifications(Guid userId);
        Task<bool> MarkAsRead(Guid notificationId);
        Task<bool> DeleteNotification(Guid notificationId);
        
        // Special notification creators based on your screenshot
        // Task<GetNotificationRequest> CreateAppointmentNotification(Guid userId, Guid appointmentId);
        // Task<GetNotificationRequest> CreateTestResultNotification(Guid userId, Guid testResultId);
        // Task<GetNotificationRequest> CreateHealthUpdateNotification(Guid userId);
        // Task<GetNotificationRequest> CreatePaymentNotification(Guid userId);
        // Task<GetNotificationRequest> CreateMedicationAlertNotification(Guid userId, string medicationName);
    }
}