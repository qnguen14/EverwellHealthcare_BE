using AutoMapper;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Notifications;
using Everwell.DAL.Data.Responses.Notifications;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.DAL.Data.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Everwell.DAL.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Everwell.BLL.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;
        
        public NotificationService(
            IUnitOfWork<EverwellDbContext> unitOfWork, 
            IMapper mapper, 
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<GetNotificationResponse> CreateNotification(CreateNotificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogError("CreateNotification request is null");
                    throw new ArgumentNullException(nameof(request), "Request cannot be null");
                }
                
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    Priority = request.Priority,
                    AppointmentId = request.AppointmentId,
                    TestResultId = request.TestResultId,
                    STITestingId = request.STITestingId,
                    QuestionId = request.QuestionId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);

                return _mapper.Map<GetNotificationResponse>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }
        
        public async Task<List<GetNotificationResponse>> GetUserNotifications(Guid userId)
        {
            try
            {
                var notifications = await _unitOfWork.GetRepository<Notification>()
                    .GetListAsync(
                        predicate: n => n.UserId == userId,
                        include: a => a.Include(nt => nt.TestResult)
                            .Include(nt => nt.Appointment)
                            .Include(nt => nt.TestResult)
                            .Include(nt => nt.STITesting)
                            .Include(nt => nt.Question)
                            .Include(nt => nt.Customer),
                        orderBy: n => n.OrderByDescending(x => x.CreatedAt));

                if (notifications == null || notifications.Count() == 0)
                {
                    return new List<GetNotificationResponse>();
                }
                
                return _mapper.Map<List<GetNotificationResponse>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user notifications");
                throw;
            }
        }
        
        public async Task<bool> MarkAsRead(Guid notificationId)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var notification = await _unitOfWork.GetRepository<Notification>()
                        .FirstOrDefaultAsync(
                            predicate: n => n.Id == notificationId &&
                                            n.IsRead == false && n.Customer.IsActive == true,  
                            include: n => n.Include(nt => nt.TestResult)
                                .Include(nt => nt.Appointment)
                                .Include(nt => nt.TestResult)
                                .Include(nt => nt.Customer));

                    if (notification == null)
                    {
                        return false;
                    }
                    
                    notification.IsRead = true;
                    _unitOfWork.GetRepository<Notification>().UpdateAsync(notification);
                
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }
        
        public async Task<bool> DeleteNotification(Guid notificationId)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var notification = await _unitOfWork.GetRepository<Notification>()
                        .FirstOrDefaultAsync(
                            predicate: n => n.Id == notificationId &&
                                            n.IsRead == false && n.Customer.IsActive == true,  
                            include: n => n.Include(nt => nt.TestResult)
                                .Include(nt => nt.Appointment)
                                .Include(nt => nt.TestResult)
                                .Include(nt => nt.Customer));

                    if (notification == null)
                    {
                        return false;
                    }
                    
                    notification.IsRead = true;
                    _unitOfWork.GetRepository<Notification>().DeleteAsync(notification);
                
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }
        
        // Implementation of specialized notification creators based on the screenshot
        // public async Task<NotificationResponse> CreateAppointmentNotification(Guid userId, Guid appointmentId)
        // {
        //     var appointment = await _unitOfWork.GetRepository<Appointment>().GetByIdAsync(appointmentId);
        //     if (appointment == null)
        //         throw new KeyNotFoundException("Appointment not found");
        //     
        //     var doctor = await _unitOfWork.GetRepository<User>().GetByIdAsync(appointment.ConsultantId);
        //     if (doctor == null)
        //         throw new KeyNotFoundException("Doctor not found");
        //         
        //     var request = new CreateNotificationRequest
        //     {
        //         UserId = userId,
        //         Title = "Nhắc nhở cuộc hẹn sắp tới",
        //         Message = $"Bạn có lịch hẹn khám với Bác sĩ {doctor.Name} vào ngày {appointment.AppointmentDate.ToString("dd/MM/yyyy")} lúc {appointment.StartTime.ToString("HH:mm")}.",
        //         Type = NotificationType.Appointment,
        //         Priority = NotificationPriority.High,
        //         AppointmentId = appointmentId
        //     };
        //     
        //     return await CreateNotification(request);
        // }
        
        // public async Task<GetNotificationResponse> CreateTestResultNotification(Guid userId, Guid testResultId)
        // {
        //     var testResult = await _unitOfWork.GetRepository<TestResult>().FirstOrDefaultAsync(
        //         predicate: tr => tr.Id == testResultId,);
        //         include: tr => tr.Include(t => t.Customer));
        //     if (testResult == null)
        //         throw new KeyNotFoundException("Test result not found");
        //     
        //     var request = new CreateNotificationRequest
        //     {
        //         UserId = userId,
        //         Title = "Kết quả xét nghiệm đã có",
        //         Message = $"Kết quả xét nghiệm của bạn từ ngày {testResult.ResultDate.ToString("dd/MM/yyyy")} đã được cập nhật. Vui lòng kiểm tra hồ sơ y tế của bạn.",
        //         Type = NotificationType.TestResult,
        //         Priority = NotificationPriority.Medium,
        //         TestResultId = testResultId
        //     };
        //     
        //     return await CreateNotification(request);
        // }
        
        // public async Task<GetNotificationResponse> CreateHealthUpdateNotification(Guid userId)
        // {
        //     var request = new CreateNotificationRequest
        //     {
        //         UserId = userId,
        //         Title = "Cập nhật thông tin sức khỏe",
        //         Message = "Đã đến lúc cập nhật thông tin sức khỏe hàng tháng của bạn. Điều này giúp chúng tôi cung cấp dịch vụ tốt hơn.",
        //         Type = NotificationType.HealthUpdate,
        //         Priority = NotificationPriority.Low
        //     };
        //     
        //     return await CreateNotification(request);
        // }
        //
        // public async Task<GetNotificationResponse> CreatePaymentNotification(Guid userId)
        // {
        //     var request = new CreateNotificationRequest
        //     {
        //         UserId = userId,
        //         Title = "Thanh toán hóa đơn dịch vụ",
        //         Message = "Thanh toán thành công hóa đơn dịch vụ khám sức khỏe tổng quát. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.",
        //         Type = NotificationType.Payment,
        //         Priority = NotificationPriority.Medium
        //     };
        //     
        //     return await CreateNotification(request);
        // }
        //
        // public async Task<GetNotificationResponse> CreateMedicationAlertNotification(Guid userId, string medicationName)
        // {
        //     var request = new CreateNotificationRequest
        //     {
        //         UserId = userId,
        //         Title = "Cảnh báo dị ứng thuốc",
        //         Message = $"Dựa trên hồ sơ y tế của bạn, chúng tôi nhận thấy bạn có thể bị dị ứng với loại thuốc {medicationName} mới được kê đơn. Vui lòng liên hệ bác sĩ của bạn.",
        //         Type = NotificationType.MedicationAlert,
        //         Priority = NotificationPriority.High
        //     };
        //     
        //     return await CreateNotification(request);
        // }
    }
}