using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Entities
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid AppointmentId { get; set; }
        
        [Required]
        public Guid SenderId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }
        
        [Required]
        public DateTime SentAt { get; set; }
        
        [MaxLength(100)]
        public string? SenderName { get; set; }
        
        [MaxLength(50)]
        public string? SenderRole { get; set; }
        
        public bool IsSystemMessage { get; set; } = false;
        
        // Navigation properties
        public virtual Appointment Appointment { get; set; }
        public virtual User Sender { get; set; }
    }
} 