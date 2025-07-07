namespace Everwell.DAL.Data.Responses.Chat
{
    public class GetChatMessagesResponse
    {
        public List<ChatMessageResponse> Messages { get; set; } = new List<ChatMessageResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
} 