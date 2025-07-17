using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Chat;
using Everwell.DAL.Data.Responses.Chat;
using Everwell.DAL.Data.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Supabase;
using System.Text;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.API.Constants;

namespace Everwell.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly Client _supabase;

        public ChatController(IChatService chatService, ILogger<ChatController> logger, Client supabase)
        {
            _chatService = chatService;
            _logger = logger;
            _supabase = supabase;
        }

        #region Chat Endpoints

        /// <summary>
        /// Gửi tin nhắn chat trong cuộc hẹn
        /// </summary>
        /// <param name="request">Thông tin tin nhắn chat</param>
        /// <returns>Thông tin tin nhắn đã gửi</returns>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<ChatMessageResponse>>> SendChatMessage([FromBody] SendChatMessageRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 401, "User not authenticated", "Unauthorized"));
                }

                var result = await _chatService.SendChatMessageAsync(request, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendChatMessage endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 500, "Internal server error", "Internal server error"));
            }
        }

        /// <summary>
        /// Lấy danh sách tin nhắn chat của cuộc hẹn
        /// </summary>
        /// <param name="request">Thông tin lọc tin nhắn</param>
        /// <returns>Danh sách tin nhắn chat</returns>
        [HttpPost("messages")]
        public async Task<ActionResult<ApiResponse<GetChatMessagesResponse>>> GetChatMessages([FromBody] GetChatMessagesRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 401, "User not authenticated", "Unauthorized"));
                }

                var result = await _chatService.GetChatMessagesAsync(request, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatMessages endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 500, "Internal server error", "Internal server error"));
            }
        }

        /// <summary>
        /// Lấy tin nhắn chat gần đây của cuộc hẹn
        /// </summary>
        /// <param name="appointmentId">ID cuộc hẹn</param>
        /// <param name="count">Số lượng tin nhắn cần lấy (mặc định 10)</param>
        /// <returns>Danh sách tin nhắn gần đây</returns>
        [HttpGet("recent/{appointmentId}")]
        public async Task<ActionResult<ApiResponse<List<ChatMessageResponse>>>> GetRecentChatMessages(
            Guid appointmentId,
            [FromQuery] int count = 10)
        {
            try
            {
                var result = await _chatService.GetRecentChatMessagesAsync(appointmentId, count);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRecentChatMessages endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<List<ChatMessageResponse>>(null, 500, "Internal server error", "Internal server error"));
            }
        }

        /// <summary>
        /// Xóa tin nhắn chat
        /// </summary>
        /// <param name="messageId">ID tin nhắn cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{messageId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteChatMessage(Guid messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<bool>(false, 401, "User not authenticated", "Unauthorized"));
                }

                var result = await _chatService.DeleteChatMessageAsync(messageId, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteChatMessage endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<bool>(false, 500, "Internal server error", "Internal server error"));
            }
        }

        /// <summary>
        /// Lấy danh sách tin nhắn chat bằng GET (cho dễ sử dụng)
        /// </summary>
        /// <param name="appointmentId">ID cuộc hẹn</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách tin nhắn chat</returns>
        [HttpGet("{appointmentId}")]
        public async Task<ActionResult<ApiResponse<GetChatMessagesResponse>>> GetChatMessagesByGet(
            Guid appointmentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 401, "User not authenticated", "Unauthorized"));
                }

                var request = new GetChatMessagesRequest
                {
                    AppointmentId = appointmentId,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _chatService.GetChatMessagesAsync(request, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatMessagesByGet endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<GetChatMessagesResponse>(null, 500, "Internal server error", "Internal server error"));
            }
        }

        /// <summary>
        /// Debug endpoint để kiểm tra thông tin appointment và user
        /// </summary>
        /// <param name="appointmentId">ID cuộc hẹn</param>
        /// <returns>Thông tin debug</returns>
        [HttpGet("debug/{appointmentId}")]
        public async Task<ActionResult> DebugAppointmentInfo(Guid appointmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _chatService.GetDebugInfoAsync(appointmentId, userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugAppointmentInfo endpoint");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Sync tin nhắn từ Daily.co vào database
        /// </summary>
        /// <param name="request">Thông tin tin nhắn từ Daily.co</param>
        /// <returns>Tin nhắn đã được lưu</returns>
        [HttpPost("sync-daily-message")]
        public async Task<ActionResult<ApiResponse<ChatMessageResponse>>> SyncDailyMessage([FromBody] SyncDailyMessageRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 401, "User not authenticated", "Unauthorized"));
                }

                var result = await _chatService.SyncDailyMessageAsync(request, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncDailyMessage endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<ChatMessageResponse>(null, 500, "Internal server error", "Internal server error"));
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        #endregion


        [HttpPost(ApiEndpointConstants.Chat.SaveChatLogEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveChatLog([FromBody] SaveChatLogRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponseBuilder.BuildErrorResponse<string>(null, 401, "User not authenticated", "Unauthorized"));
                }

                var result = await _chatService.SaveChatLogAsync(request, userId.Value);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    400 => BadRequest(result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveChatLog endpoint");
                return StatusCode(500, ApiResponseBuilder.BuildErrorResponse<string>(null, 500, "Internal server error", "Internal server error"));
            }


        }
    }
}