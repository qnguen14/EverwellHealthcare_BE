using System.Linq;
using System.Text;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Requests.Chat;
using Everwell.DAL.Data.Responses.Chat;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Supabase;

namespace Everwell.BLL.Services.Implements
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork<EverwellDbContext> _unitOfWork;
        private readonly ILogger<ChatService> _logger;
        private readonly Client _supabase;

        public ChatService(
            IUnitOfWork<EverwellDbContext> unitOfWork,
            ILogger<ChatService> logger,
            Client supabase
        )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _supabase = supabase;
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendChatMessageAsync(
            SendChatMessageRequest request,
            Guid senderId
        )
        {
            try
            {
                _logger.LogInformation(
                    "Sending chat message for appointment {AppointmentId} from user {SenderId}",
                    request.AppointmentId,
                    senderId
                );

                // Validate input
                if (request == null)
                {
                    _logger.LogWarning("SendChatMessageRequest is null");
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                        null,
                        400,
                        "Invalid request",
                        "Bad request"
                    );
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    _logger.LogWarning("Message is null or empty");
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                        null,
                        400,
                        "Message cannot be empty",
                        "Bad request"
                    );
                }

                // Verify appointment exists and user has access
                var appointment = await _unitOfWork
                    .GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning(
                        "Appointment {AppointmentId} not found",
                        request.AppointmentId
                    );
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                        null,
                        404,
                        "Cuộc hẹn không tồn tại",
                        "Appointment not found"
                    );
                }

                // Check if user is participant in the appointment
                if (
                    !request.IsSystemMessage
                    && appointment.CustomerId != senderId
                    && appointment.ConsultantId != senderId
                )
                {
                    _logger.LogWarning(
                        "User {SenderId} is not a participant in appointment {AppointmentId}. Customer: {CustomerId}, Consultant: {ConsultantId}",
                        senderId,
                        request.AppointmentId,
                        appointment.CustomerId,
                        appointment.ConsultantId
                    );
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                        null,
                        403,
                        "Bạn không có quyền gửi tin nhắn trong cuộc hẹn này",
                        "Forbidden"
                    );
                }

                // Get sender information
                User sender = null;
                if (!request.IsSystemMessage)
                {
                    sender = await _unitOfWork
                        .GetRepository<User>()
                        .FirstOrDefaultAsync(predicate: u => u.Id == senderId);

                    if (sender == null)
                    {
                        _logger.LogWarning("Sender {SenderId} not found", senderId);
                        return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                            null,
                            404,
                            "Người gửi không tồn tại",
                            "Sender not found"
                        );
                    }
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
                    IsSystemMessage = request.IsSystemMessage,
                };

                _logger.LogInformation(
                    "Attempting to save chat message with ID {MessageId}",
                    chatMessage.Id
                );

                await _unitOfWork.GetRepository<ChatMessage>().InsertAsync(chatMessage);
                var saveResult = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Chat message saved successfully with ID {MessageId}. SaveChanges result: {SaveResult}",
                    chatMessage.Id,
                    saveResult
                );

                var response = new ChatMessageResponse
                {
                    Id = chatMessage.Id,
                    AppointmentId = chatMessage.AppointmentId,
                    SenderId = chatMessage.SenderId,
                    Message = chatMessage.Message,
                    SentAt = chatMessage.SentAt,
                    SenderName = chatMessage.SenderName,
                    SenderRole = chatMessage.SenderRole,
                    IsSystemMessage = chatMessage.IsSystemMessage,
                };

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Chat message sent successfully",
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending chat message for appointment {AppointmentId}. Exception: {ExceptionMessage}",
                    request?.AppointmentId,
                    ex.Message
                );
                return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                    null,
                    500,
                    "Failed to send chat message",
                    "Internal server error"
                );
            }
        }

        public async Task<ApiResponse<GetChatMessagesResponse>> GetChatMessagesAsync(
            GetChatMessagesRequest request,
            Guid requesterId
        )
        {
            try
            {
                _logger.LogInformation(
                    "Getting chat messages for appointment {AppointmentId} by user {RequesterId}",
                    request.AppointmentId,
                    requesterId
                );

                // Verify appointment exists and user has access
                var appointment = await _unitOfWork
                    .GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);

                if (appointment == null)
                {
                    return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(
                        null,
                        404,
                        "Appointment not found",
                        "Appointment not found"
                    );
                }

                // Check if user is participant in the appointment
                if (
                    appointment.CustomerId != requesterId
                    && appointment.ConsultantId != requesterId
                )
                {
                    return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(
                        null,
                        403,
                        "User is not a participant in this appointment",
                        "Forbidden"
                    );
                }

                // Build query - get all messages for this appointment
                var allMessages = await _unitOfWork
                    .GetRepository<ChatMessage>()
                    .GetListAsync(predicate: cm => cm.AppointmentId == request.AppointmentId);

                // Apply date filters
                var filteredMessages = allMessages.AsQueryable();

                if (request.FromDate.HasValue)
                {
                    filteredMessages = filteredMessages.Where(cm =>
                        cm.SentAt >= request.FromDate.Value
                    );
                }

                if (request.ToDate.HasValue)
                {
                    filteredMessages = filteredMessages.Where(cm =>
                        cm.SentAt <= request.ToDate.Value
                    );
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
                        IsSystemMessage = cm.IsSystemMessage,
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
                    HasPreviousPage = request.Page > 1,
                };

                _logger.LogInformation(
                    "Retrieved {Count} chat messages for appointment {AppointmentId}",
                    messages.Count,
                    request.AppointmentId
                );

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Chat messages retrieved successfully",
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting chat messages for appointment {AppointmentId}",
                    request.AppointmentId
                );
                return ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(
                    null,
                    500,
                    "Failed to retrieve chat messages",
                    "Internal server error"
                );
            }
        }

        public async Task<ApiResponse<bool>> DeleteChatMessageAsync(
            Guid messageId,
            Guid requesterId
        )
        {
            try
            {
                _logger.LogInformation(
                    "Deleting chat message {MessageId} by user {RequesterId}",
                    messageId,
                    requesterId
                );

                var chatMessage = await _unitOfWork
                    .GetRepository<ChatMessage>()
                    .FirstOrDefaultAsync(predicate: cm => cm.Id == messageId);

                if (chatMessage == null)
                {
                    return ApiResponseBuilder.BuildErrorResponse<bool>(
                        false,
                        404,
                        "Chat message not found",
                        "Chat message not found"
                    );
                }

                // Only sender can delete their own message (or system messages)
                if (chatMessage.SenderId != requesterId && !chatMessage.IsSystemMessage)
                {
                    return ApiResponseBuilder.BuildErrorResponse<bool>(
                        false,
                        403,
                        "You can only delete your own messages",
                        "Forbidden"
                    );
                }

                _unitOfWork.GetRepository<ChatMessage>().DeleteAsync(chatMessage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Chat message {MessageId} deleted successfully", messageId);

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Chat message deleted successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat message {MessageId}", messageId);
                return ApiResponseBuilder.BuildErrorResponse<bool>(
                    false,
                    500,
                    "Failed to delete chat message",
                    "Internal server error"
                );
            }
        }

        public async Task<ApiResponse<List<ChatMessageResponse>>> GetRecentChatMessagesAsync(
            Guid appointmentId,
            int count = 10
        )
        {
            try
            {
                _logger.LogInformation(
                    "Getting recent {Count} chat messages for appointment {AppointmentId}",
                    count,
                    appointmentId
                );

                var allMessages = await _unitOfWork
                    .GetRepository<ChatMessage>()
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
                        IsSystemMessage = cm.IsSystemMessage,
                    })
                    .ToList();

                // Reverse to get chronological order
                messages.Reverse();

                _logger.LogInformation(
                    "Retrieved {Count} recent chat messages for appointment {AppointmentId}",
                    messages.Count,
                    appointmentId
                );

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Recent chat messages retrieved successfully",
                    messages
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting recent chat messages for appointment {AppointmentId}",
                    appointmentId
                );
                return ApiResponseBuilder.BuildErrorResponse<List<ChatMessageResponse>>(
                    null,
                    500,
                    "Failed to retrieve recent chat messages",
                    "Internal server error"
                );
            }
        }

        private string GetUserRole(User? user, Appointment appointment)
        {
            if (user == null)
                return "System";

            if (user.Id == appointment.ConsultantId)
                return "Consultant";
            else if (user.Id == appointment.CustomerId)
                return "Patient";
            else
                return "Unknown";
        }

        public async Task<object> GetDebugInfoAsync(Guid appointmentId, Guid userId)
        {
            try
            {
                var appointment = await _unitOfWork
                    .GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == appointmentId);

                var user = await _unitOfWork
                    .GetRepository<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == userId);

                return new
                {
                    AppointmentId = appointmentId,
                    UserId = userId,
                    Appointment = appointment == null
                        ? null
                        : new
                        {
                            appointment.Id,
                            appointment.CustomerId,
                            appointment.ConsultantId,
                            appointment.Status,
                            appointment.AppointmentDate,
                            appointment.Slot,
                            appointment.IsVirtual,
                            appointment.CreatedAt,
                        },
                    User = user == null
                        ? null
                        : new
                        {
                            user.Id,
                            user.Name,
                            user.Email,
                            user.Role,
                        },
                    IsParticipant = appointment != null
                        && (appointment.CustomerId == userId || appointment.ConsultantId == userId),
                    UserRole = GetUserRole(user, appointment),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting debug info for appointment {AppointmentId} and user {UserId}",
                    appointmentId,
                    userId
                );
                return new { Error = ex.Message };
            }
        }

        public async Task<ApiResponse<ChatMessageResponse>> SyncDailyMessageAsync(
            SyncDailyMessageRequest request,
            Guid senderId
        )
        {
            try
            {
                _logger.LogInformation(
                    "Syncing Daily.co message for appointment {AppointmentId} by user {SenderId}",
                    request.AppointmentId,
                    senderId
                );

                // Verify appointment exists
                var appointment = await _unitOfWork
                    .GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning(
                        "Appointment {AppointmentId} not found",
                        request.AppointmentId
                    );
                    return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                        null,
                        404,
                        "Appointment not found",
                        "Appointment not found"
                    );
                }

                // Get sender info
                var sender = await _unitOfWork
                    .GetRepository<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == senderId);

                // Create chat message
                var chatMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = request.AppointmentId,
                    SenderId = senderId,
                    Message = request.Message,
                    SentAt = request.SentAt ?? DateTime.UtcNow,
                    SenderName = request.SenderName ?? sender?.Name ?? "Unknown",
                    SenderRole = GetUserRole(sender, appointment),
                    IsSystemMessage = request.IsSystemMessage,
                };

                _logger.LogInformation(
                    "Attempting to save Daily.co chat message with ID {MessageId}",
                    chatMessage.Id
                );

                await _unitOfWork.GetRepository<ChatMessage>().InsertAsync(chatMessage);
                var saveResult = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Daily.co chat message saved successfully with ID {MessageId}. SaveChanges result: {SaveResult}",
                    chatMessage.Id,
                    saveResult
                );

                var response = new ChatMessageResponse
                {
                    Id = chatMessage.Id,
                    AppointmentId = chatMessage.AppointmentId,
                    SenderId = chatMessage.SenderId,
                    Message = chatMessage.Message,
                    SentAt = chatMessage.SentAt,
                    SenderName = chatMessage.SenderName,
                    SenderRole = chatMessage.SenderRole,
                    IsSystemMessage = chatMessage.IsSystemMessage,
                };

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Daily.co chat message synced successfully",
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error syncing Daily.co message for appointment {AppointmentId}. Exception: {ExceptionMessage}",
                    request?.AppointmentId,
                    ex.Message
                );
                return ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(
                    null,
                    500,
                    "Failed to sync Daily.co message",
                    "Internal server error"
                );
            }
        }

        #region Save Chat Log

        public async Task<ApiResponse<string>> SaveChatLogAsync(
            SaveChatLogRequest request,
            Guid userId
        )
        {
            try
            {
                _logger.LogInformation(
                    "Saving chat log for appointment {AppointmentId} by user {UserId}",
                    request.AppointmentId,
                    userId
                );

                // Validate input
                if (request == null)
                {
                    _logger.LogWarning("SaveChatLogRequest is null");
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        400,
                        "Invalid request",
                        "Bad request"
                    );
                }

                if (string.IsNullOrWhiteSpace(request.LogContent))
                {
                    _logger.LogWarning("Log content is null or empty");
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        400,
                        "Log content cannot be empty",
                        "Bad request"
                    );
                }

                // Verify appointment exists and user has access
                var appointment = await _unitOfWork
                    .GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == request.AppointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning(
                        "Appointment {AppointmentId} not found",
                        request.AppointmentId
                    );
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        404,
                        "Appointment not found",
                        "Appointment not found"
                    );
                }

                // Check if user is participant in the appointment
                if (appointment.CustomerId != userId && appointment.ConsultantId != userId)
                {
                    _logger.LogWarning(
                        "User {UserId} is not a participant in appointment {AppointmentId}. Customer: {CustomerId}, Consultant: {ConsultantId}",
                        userId,
                        request.AppointmentId,
                        appointment.CustomerId,
                        appointment.ConsultantId
                    );
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        403,
                        "You don't have permission to save chat log for this appointment",
                        "Forbidden"
                    );
                }

                // Get user information for file path
                var user = await _unitOfWork
                    .GetRepository<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        404,
                        "User not found",
                        "User not found"
                    );
                }

                // Create file path with timestamp for uniqueness
                const string bucketName = "chat-logs";
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var filePath = $"{userId}/{request.AppointmentId}-{timestamp}.txt";

                // Add metadata to log content
                var enhancedContent = CreateEnhancedLogContent(
                    request.LogContent,
                    appointment,
                    user,
                    timestamp
                );
                var contentBytes = Encoding.UTF8.GetBytes(enhancedContent);

                _logger.LogInformation(
                    "Uploading chat log to Supabase storage. Bucket: {Bucket}, Path: {Path}",
                    bucketName,
                    filePath
                );

                // Upload to Supabase Storage
                var uploadResult = await _supabase
                    .Storage.From(bucketName)
                    .Upload(
                        contentBytes,
                        filePath,
                        new Supabase.Storage.FileOptions
                        {
                            ContentType = "text/plain;charset=utf-8",
                            Upsert = true, // Overwrite if file already exists
                        }
                    );

                if (string.IsNullOrEmpty(uploadResult))
                {
                    _logger.LogError("Upload to Supabase failed - received empty response");
                    return ApiResponseBuilder.BuildErrorResponse<string>(
                        null,
                        500,
                        "Failed to upload chat log",
                        "Upload failed"
                    );
                }

                _logger.LogInformation("Chat log saved successfully to {Path}", filePath);

                // Optionally save a reference in the database for audit purposes
                await SaveChatLogReference(
                    request.AppointmentId,
                    userId,
                    filePath,
                    enhancedContent.Length
                );

                return ApiResponseBuilder.BuildResponse(
                    200,
                    "Chat log saved successfully",
                    filePath
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error saving chat log for appointment {AppointmentId} by user {UserId}. Exception: {ExceptionMessage}",
                    request?.AppointmentId,
                    userId,
                    ex.Message
                );
                return ApiResponseBuilder.BuildErrorResponse<string>(
                    null,
                    500,
                    "Failed to save chat log",
                    "Internal server error"
                );
            }
        }

        private string CreateEnhancedLogContent(
            string originalContent,
            Appointment appointment,
            User user,
            string timestamp
        )
        {
            var header = new StringBuilder();
            header.AppendLine("=".PadLeft(60, '='));
            header.AppendLine("EVERWELL HEALTHCARE - CHAT LOG");
            header.AppendLine("=".PadLeft(60, '='));
            header.AppendLine($"Appointment ID: {appointment.Id}");
            header.AppendLine(
                $"Appointment Date: {appointment.AppointmentDate:yyyy-MM-dd HH:mm:ss}"
            );
            header.AppendLine($"Appointment Slot: {appointment.Slot}");
            header.AppendLine($"Is Virtual: {appointment.IsVirtual}");
            header.AppendLine($"Status: {appointment.Status}");
            header.AppendLine($"Saved by: {user.Name} ({user.Email})");
            header.AppendLine($"User Role: {GetUserRole(user, appointment)}");
            header.AppendLine($"Export Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            header.AppendLine($"Export Timestamp: {timestamp}");
            header.AppendLine("=".PadLeft(60, '='));
            header.AppendLine();
            header.AppendLine("CHAT MESSAGES:");
            header.AppendLine("-".PadLeft(40, '-'));
            header.AppendLine();

            return header.ToString() + originalContent;
        }

        private async Task SaveChatLogReference(
            Guid appointmentId,
            Guid userId,
            string filePath,
            int contentLength
        )
        {
            try
            {
                // You can create a ChatLogReference entity to track saved logs
                // This is optional but useful for audit purposes
                var logReference = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    SenderId = userId,
                    Message = $"Chat log exported to: {filePath} (Size: {contentLength} bytes)",
                    SentAt = DateTime.UtcNow,
                    SenderName = "System",
                    SenderRole = "System",
                    IsSystemMessage = true,
                };

                await _unitOfWork.GetRepository<ChatMessage>().InsertAsync(logReference);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Chat log reference saved for appointment {AppointmentId}",
                    appointmentId
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to save chat log reference for appointment {AppointmentId}",
                    appointmentId
                );
                // Don't throw here as the main operation (saving to storage) was successful
            }
        }

        #endregion
    }
}
