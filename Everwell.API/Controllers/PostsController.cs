// ============================================================================
// POSTS CONTROLLER
// ============================================================================
// This controller manages educational content, community posts, and health articles
// It handles content creation, moderation, and knowledge sharing within the platform
// 
// CONTENT MANAGEMENT FLOW:
// 1. CONTENT CREATION: Healthcare professionals create educational posts
// 2. CONTENT REVIEW: Medical team reviews for accuracy and appropriateness
// 3. CONTENT PUBLICATION: Approved content published to community
// 4. CONTENT ENGAGEMENT: Users interact through views, likes, comments
// 5. CONTENT ANALYTICS: Track engagement and educational impact
// 6. CONTENT UPDATES: Regular updates based on latest medical guidelines
// 
// POST TYPES:
// - Educational articles: Women's health topics, preventive care
// - Health tips: Daily wellness advice, lifestyle recommendations
// - Medical updates: Latest research, treatment options
// - Community stories: Patient experiences, success stories
// - FAQ posts: Common questions and expert answers
// 
// CONTENT CATEGORIES:
// - Reproductive health: Menstrual health, fertility, contraception
// - STI prevention: Testing, treatment, prevention strategies
// - Mental health: Stress management, emotional wellness
// - Lifestyle: Nutrition, exercise, sleep hygiene
// - Preventive care: Screening schedules, health checkups
// 
// QUALITY ASSURANCE:
// - Medical accuracy verification by healthcare professionals
// - Content moderation for appropriateness and sensitivity
// - Regular updates based on current medical guidelines
// - User feedback integration for content improvement
// 
// ENGAGEMENT FEATURES:
// - Content rating and feedback system
// - Bookmark functionality for important articles
// - Share capabilities for spreading health awareness
// - Comment system for community discussion
// - Personalized content recommendations

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

    /// <summary>
    /// GET ALL POSTS
    /// =============
    /// Retrieves all published educational content and community posts
    /// 
    /// CONTENT DELIVERY:
    /// - Published educational articles and health tips
    /// - Community-generated content and success stories
    /// - Latest medical updates and research findings
    /// - FAQ posts and expert answers
    /// 
    /// CONTENT FILTERING:
    /// - Only approved and published content
    /// - Content sorted by relevance and recency
    /// - Category-based organization
    /// - User engagement metrics included
    /// 
    /// EDUCATIONAL VALUE:
    /// - Evidence-based health information
    /// - Expert-reviewed medical content
    /// - Practical health tips and advice
    /// - Community support and shared experiences
    /// 
    /// USE CASES:
    /// - Health education: Learning about women's health topics
    /// - Community engagement: Reading shared experiences
    /// - Medical updates: Staying informed about latest research
    /// - Preventive care: Understanding health screening schedules
    /// - Lifestyle improvement: Wellness tips and recommendations
    /// 
    /// PUBLIC ACCESS:
    /// - No authentication required for educational content
    /// - Promotes health awareness and education
    /// - Supports community health initiatives
    /// </summary>
    [HttpGet(ApiEndpointConstants.Post.GetAllPostsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreatePostResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    
    public async Task<IActionResult> GetAllPosts()
    {
        try
        {
            // Retrieve all published educational content and community posts
            // Content is pre-filtered for approval status and publication date
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
                Data = posts // Educational content, health tips, community posts
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