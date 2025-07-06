using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Chat;
using Everwell.DAL.Data.Responses.Chat;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Everwell.BLL.Services.Implements
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendChatMessageAsync(SendChatMessageRequest request, Guid senderId)
        {
            try
            {
                _logger.LogInformation("Sending chat message for appointment {AppointmentId} from user {SenderId}", 
                    request.AppointmentId, senderId);

                // Verify appointment exists and user has access
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);
                
                if (appointment == null)
                {
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 404, "Appointment not found", "Appointment not found");
                }

                // Check if user is participant in the appointment
                if (!request.IsSystemMessage && appointment.CustomerId != senderId && appointment.ConsultantId != senderId)
                {
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 403, "User is not a participant in this appointment", "Forbidden");
                }

                // Get sender information
                var sender = await _unitOfWork.GetRepository<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == senderId);

                if (sender == null && !request.IsSystemMessage)
                {
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 404, "Sender not found", "Sender not found");
                }

                // Create chat message
                var chatMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = request.AppointmentId,
                    SenderId = senderId,
                    Message = request.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = sender?.Name ?? "System",
                    SenderRole = GetUserRole(sender, appointment),
                    IsSystemMessage = request.IsSystemMessage
                };

                await _unitOfWork.GetRepository<ChatMessage>().InsertAsync(chatMessage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Chat message sent successfully with ID {MessageId}", chatMessage.Id);

                var response = new ChatMessageResponse
                {
                    Id = chatMessage.Id,
                    AppointmentId = chatMessage.AppointmentId,
                    SenderId = chatMessage.SenderId,
                    Message = chatMessage.Message,
                    SentAt = chatMessage.SentAt,
                    SenderName = chatMessage.SenderName,
                    SenderRole = chatMessage.SenderRole,
                    IsSystemMessage = chatMessage.IsSystemMessage
                };

                return ApiResponseBuilder.BuildResponse(200, "Chat message sent successfully", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message for appointment {AppointmentId}", request.AppointmentId);
                return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 500, "Failed to send chat message", "Internal server error");
            }
        }

        public async Task<ApiResponse<GetChatMessagesResponse>> GetChatMessagesAsync(GetChatMessagesRequest request, Guid requesterId)
        {
            try
            {
                _logger.LogInformation("Getting chat messages for appointment {AppointmentId} by user {RequesterId}", 
                    request.AppointmentId, requesterId);

                // Verify appointment exists and user has access
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);
                
                if (appointment == null)
                {
                    return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 404, "Appointment not found", "Appointment not found");
                }

                // Check if user is participant in the appointment
                if (appointment.CustomerId != requesterId && appointment.ConsultantId != requesterId)
                {
                    return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 403, "User is not a participant in this appointment", "Forbidden");
                }

                // Build query - get all messages for this appointment
                var allMessages = await _unitOfWork.GetRepository<ChatMessage>()
                    .GetListAsync(predicate: cm => cm.AppointmentId == request.AppointmentId);

                // Apply date filters
                var filteredMessages = allMessages.AsQueryable();
                
                if (request.FromDate.HasValue)
                {
                    filteredMessages = filteredMessages.Where(cm => cm.SentAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    filteredMessages = filteredMessages.Where(cm => cm.SentAt <= request.ToDate.Value);
                }

                // Get total count
                var totalCount = filteredMessages.Count();

                // Apply pagination and ordering
                var messages = filteredMessages
                    .OrderBy(cm => cm.SentAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(cm => new ChatMessageResponse
                    {
                        Id = cm.Id,
                        AppointmentId = cm.AppointmentId,
                        SenderId = cm.SenderId,
                        Message = cm.Message,
                        SentAt = cm.SentAt,
                        SenderName = cm.SenderName,
                        SenderRole = cm.SenderRole,
                        IsSystemMessage = cm.IsSystemMessage
                    })
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                var response = new GetChatMessagesResponse
                {
                    Messages = messages,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = request.Page < totalPages,
                    HasPreviousPage = request.Page > 1
                };

                _logger.LogInformation("Retrieved {Count} chat messages for appointment {AppointmentId}", 
                    messages.Count, request.AppointmentId);

                return ApiResponseBuilder.BuildResponse(200, "Chat messages retrieved successfully", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat messages for appointment {AppointmentId}", request.AppointmentId);
                return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 500, "Failed to retrieve chat messages", "Internal server error");
            }
        }

        public async Task<ApiResponse<bool>> DeleteChatMessageAsync(Guid messageId, Guid requesterId)
        {
            try
            {
                _logger.LogInformation("Deleting chat message {MessageId} by user {RequesterId}", messageId, requesterId);

                var chatMessage = await _unitOfWork.GetRepository<ChatMessage>()
                    .FirstOrDefaultAsync(predicate: cm => cm.Id == messageId);

                if (chatMessage == null)
                {
                    return ApiResponseBuilder.BuildErrorResponse<bool>(false, 404, "Chat message not found", "Chat message not found");
                }

                // Only sender can delete their own message (or system messages)
                if (chatMessage.SenderId != requesterId && !chatMessage.IsSystemMessage)
                {
                    return ApiResponseBuilder.BuildErrorResponse<bool>(false, 403, "You can only delete your own messages", "Forbidden");
                }

                _unitOfWork.GetRepository<ChatMessage>().DeleteAsync(chatMessage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Chat message {MessageId} deleted successfully", messageId);

                return ApiResponseBuilder.BuildResponse(200, "Chat message deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat message {MessageId}", messageId);
                return ApiResponseBuilder.BuildErrorResponse<bool>(false, 500, "Failed to delete chat message", "Internal server error");
            }
        }

        public async Task<ApiResponse<List<ChatMessageResponse>>> GetRecentChatMessagesAsync(Guid appointmentId, int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting recent {Count} chat messages for appointment {AppointmentId}", 
                    count, appointmentId);

                var allMessages = await _unitOfWork.GetRepository<ChatMessage>()
                    .GetListAsync(predicate: cm => cm.AppointmentId == appointmentId);

                var messages = allMessages
                    .OrderByDescending(cm => cm.SentAt)
                    .Take(count)
                    .Select(cm => new ChatMessageResponse
                    {
                        Id = cm.Id,
                        AppointmentId = cm.AppointmentId,
                        SenderId = cm.SenderId,
                        Message = cm.Message,
                        SentAt = cm.SentAt,
                        SenderName = cm.SenderName,
                        SenderRole = cm.SenderRole,
                        IsSystemMessage = cm.IsSystemMessage
                    })
                    .ToList();

                // Reverse to get chronological order
                messages.Reverse();

                _logger.LogInformation("Retrieved {Count} recent chat messages for appointment {AppointmentId}", 
                    messages.Count, appointmentId);

                return ApiResponseBuilder.BuildResponse(200, "Recent chat messages retrieved successfully", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent chat messages for appointment {AppointmentId}", appointmentId);
                return ApiResponseBuilder.BuildErrorResponse<List<ChatMessageResponse>>(null, 500, "Failed to retrieve recent chat messages", "Internal server error");
            }
        }

        private string GetUserRole(User? user, Appointment appointment)
        {
            if (user == null) return "System";
            
            if (user.Id == appointment.ConsultantId)
                return "Consultant";
            else if (user.Id == appointment.CustomerId)
                return "Patient";
            else
                return "Unknown";
        }
    }
} 