using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities
{
    public enum NotificationType
    {
        Appointment,
        TestResult,
        STITest,
        Payment,
        Question,
    }

    public enum NotificationPriority
    {
        Low,
        Medium,
        High
    }

    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }
        public virtual User Customer { get; set; }

        [Required]
        [Column("title")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Column("message")]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        [Column("notification_type")]
        public NotificationType Type { get; set; }

        [Required]
        [Column("priority")]
        public NotificationPriority Priority { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        // Optional relationships
        [Column("appointment_id")]
        public Guid? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        [Column("test_result_id")]
        public Guid? TestResultId { get; set; }
        public virtual TestResult? TestResult { get; set; }
        
        [Column("stitesting_id")]
        public Guid? STITestingId { get; set; }
        public virtual STITesting? STITesting { get; set; }
        
        [Column("question_id")]
        public Guid? QuestionId { get; set; }
        public virtual Question? Question { get; set; }
    }
}   