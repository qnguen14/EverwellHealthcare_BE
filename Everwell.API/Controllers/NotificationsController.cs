using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Everwell.API.Controllers;

[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet(ApiEndpointConstants.Notification.GetUserNotifications)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetNotificationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<IActionResult> GetUserNotifications()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        var notifications = await _notificationService.GetUserNotifications(userId);

        return Ok(ApiResponseBuilder.BuildResponse(
            StatusCodes.Status200OK,
            "Notifications retrieved successfully",
            notifications));
    }

    [HttpPut(ApiEndpointConstants.Notification.MarkAsRead)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetNotificationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsRead(id);

        if (!result)
        {
            return NotFound(new { message = "Notification not found!" });
        }

        var apiResponse = new ApiResponse<GetNotificationResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Notification marked as read.",
            Data = null, // Assuming no data is returned on mark as read
            IsSuccess = true
        };

        return Ok(apiResponse);
    }

    [HttpDelete(ApiEndpointConstants.Notification.DeleteNotificationEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetNotificationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Customer,Consultant")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var result = await _notificationService.DeleteNotification(id);

        if (!result)
        {
            return NotFound(new { message = "Notification not found!" });
        }

        var apiResponse = new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Notification deleted successfully.",
            Data = null, // Assuming no data is returned on delete
            IsSuccess = true
        };

        return Ok(apiResponse);
    }
}