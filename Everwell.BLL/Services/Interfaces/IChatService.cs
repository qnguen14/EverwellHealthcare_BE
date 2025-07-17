using Everwell.DAL.Data.Requests.Chat;
using Everwell.DAL.Data.Responses.Chat;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Appointments;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IChatService
    {
        Task<ApiResponse<ChatMessageResponse>> SendChatMessageAsync(SendChatMessageRequest request, Guid senderId);
        Task<ApiResponse<GetChatMessagesResponse>> GetChatMessagesAsync(GetChatMessagesRequest request, Guid requesterId);
        Task<ApiResponse<bool>> DeleteChatMessageAsync(Guid messageId, Guid requesterId);
        Task<ApiResponse<List<ChatMessageResponse>>> GetRecentChatMessagesAsync(Guid appointmentId, int count = 10);
        Task<object> GetDebugInfoAsync(Guid appointmentId, Guid userId);
        Task<ApiResponse<ChatMessageResponse>> SyncDailyMessageAsync(SyncDailyMessageRequest request, Guid senderId);

        Task<ApiResponse<string>> SaveChatLogAsync(SaveChatLogRequest request, Guid userId);
    }
} 