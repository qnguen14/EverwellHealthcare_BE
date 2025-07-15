using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Chat
{
    public class SyncDailyMessageRequest
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public string? SenderName { get; set; }

        public DateTime? SentAt { get; set; }

        public bool IsSystemMessage { get; set; } = false;
    }
} 