namespace Everwell.DAL.Data.Responses.Chat
{
    public class ChatMessageResponse
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public string? SenderName { get; set; }
        public string? SenderRole { get; set; }
        public bool IsSystemMessage { get; set; }
    }
} 