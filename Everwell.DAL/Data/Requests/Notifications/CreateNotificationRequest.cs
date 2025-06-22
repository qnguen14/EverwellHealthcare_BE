using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Notifications
{
    public class CreateNotificationRequest
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        [Required]
        public NotificationPriority Priority { get; set; }
        
        public Guid? AppointmentId { get; set; }
        
        public Guid? TestResultId { get; set; }
        
        public Guid? STITestingId { get; set; }
        
        public Guid? QuestionId { get; set; }
    }
}