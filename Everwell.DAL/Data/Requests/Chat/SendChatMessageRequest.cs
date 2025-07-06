using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Chat
{
    public class SendChatMessageRequest
    {
        [Required]
        public Guid AppointmentId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }
        
        public bool IsSystemMessage { get; set; } = false;
    }
} 