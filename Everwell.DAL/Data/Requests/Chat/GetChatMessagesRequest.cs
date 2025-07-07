using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Chat
{
    public class GetChatMessagesRequest
    {
        [Required]
        public Guid AppointmentId { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
        
        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }
    }
} 