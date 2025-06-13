using System;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string toEmail, string resetCode, string userName);
        Task SendMenstrualCycleReminderAsync(string toEmail, string userName, DateTime nextCycleDate);
        Task SendAppointmentConfirmationAsync(string toEmail, string userName, DateTime appointmentDate, string serviceName);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}