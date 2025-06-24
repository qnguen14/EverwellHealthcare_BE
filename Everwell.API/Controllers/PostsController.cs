using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.Post;
using Everwell.DAL.Data.Responses.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet(ApiEndpointConstants.Post.GetAllPostsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreatePostResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetAllPosts()
    {
        try
        {
            var posts = await _postService.GetAllPostsAsync();

            if (posts == null || !posts.Any())
            {
                return NotFound(new { message = "No posts found" });
            }

            var apiResponse = new ApiResponse<IEnumerable<CreatePostResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Posts retrieved successfully",
                IsSuccess = true,
                Data = posts
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Post.GetPostEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            var apiResponse = new ApiResponse<CreatePostResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Post retrieved successfully",
                IsSuccess = true,
                Data = post
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Post.GetFilteredPostsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreatePostResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> GetFilteredPosts(
        [FromQuery] string? title,
        [FromQuery] string? content,
        [FromQuery] PostStatus? status,
        [FromQuery] PostCategory? category,
        [FromQuery] Guid staffid,
        [FromQuery] DateTime? createdAt
            )
    {
        try
        {
            var filter = new FilterPostsRequest
            {
                Title = title,
                Content = content,
                Status = status,
                Category = category,
                Staffid = staffid,
                CreatedAt = createdAt
            };

            var posts = await _postService.GetFilteredPosts(filter);

            var apiResponse = new ApiResponse<IEnumerable<CreatePostResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Filtered posts retrieved successfully",
                IsSuccess = true,
                Data = posts
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPost(ApiEndpointConstants.Post.CreatePostEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Staff, Admin")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            var createdPost = await _postService.CreatePostAsync(request);

            var apiResponse = new ApiResponse<CreatePostResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Post created successfully",
                IsSuccess = true,
                Data = createdPost
            };

            return CreatedAtAction(nameof(GetPostById), new { id = createdPost.Id }, apiResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating post", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Post.UpdatePostEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Staff, Admin")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var updatedPost = await _postService.UpdatePostAsync(id, request);
            
            var apiResponse = new ApiResponse<CreatePostResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Post updated successfully",
                IsSuccess = true,
                Data = updatedPost
            };
            
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating post", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Post.ApprovePostEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> ApprovePost(Guid id, [FromQuery] PostStatus status)
    {
        try
        {
            var approvedPost = await _postService.ApprovePostAsync(id, status);
            if (approvedPost == null)
            {
                return NotFound(new { message = "Bài đăng không tồn tại hoặc đã được duyệt/không duyệt" });
            }
            var apiResponse = new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Bài đăng được duyệt thành công",
                IsSuccess = true,
                Data = null
            };
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.Post.DeletePostEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Staff, Manager, Admin")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            var isDeleted = await _postService.DeletePostAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = "Post not found" });
            }

            var apiResponse = new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Post deleted successfully",
                IsSuccess = true,
                Data = null
            };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
} 