using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Requests.Post;
using Everwell.DAL.Data.Responses.Post;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class PostService : BaseService<PostService>, IPostService
{
    public PostService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<PostService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<IEnumerable<CreatePostResponse>> GetAllPostsAsync()
    {
        try
        {
            var posts = await _unitOfWork.GetRepository<Post>()
                .GetListAsync(
                    include: p => p.Include(post => post.Staff));
            
            if (posts == null || !posts.Any())
            {
                _logger.LogInformation("No posts found");
                return Enumerable.Empty<CreatePostResponse>();
            }
            
            return _mapper.Map<IEnumerable<CreatePostResponse>>(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all posts");
            throw;
        }
    }

    public async Task<IEnumerable<CreatePostResponse>> GetFilteredPosts(FilterPostsRequest request)
    {
        try
        {
            var posts = await _unitOfWork.GetRepository<Post>()
            .GetListAsync(
                predicate: p => 
                    (string.IsNullOrEmpty(request.Title) || p.Title.ToLower().Contains(request.Title.ToLower())) && 
                    (string.IsNullOrEmpty(request.Content) || p.Content.ToLower().Contains(request.Content.ToLower())) &&
                    (!request.Status.HasValue || p.Status == request.Status) &&
                    (!request.Category.HasValue || p.Category == request.Category) &&
                    (!request.Staffid.HasValue || p.StaffId == request.Staffid) &&
                    (!request.CreatedAt.HasValue || p.CreatedAt.Date == request.CreatedAt.Value.Date),
                include: p =>p.AsNoTracking() 
                    .Include(post => post.Staff));

            if (posts == null || !posts.Any())
            {
                _logger.LogInformation("No posts found matching the filter criteria");
                return null;
            }
            
            return _mapper.Map<IEnumerable<CreatePostResponse>>(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while filtering posts");
            throw;
        }
    }

    public async Task<CreatePostResponse> GetPostByIdAsync(Guid id)
    {
        try
        {
            var post = await _unitOfWork.GetRepository<Post>()
                .FirstOrDefaultAsync(
                    predicate: p => p.Id == id,
                    include: p => p.Include(post => post.Staff));

            if (post == null)
            {
                _logger.LogInformation("No post found");
                return null;
            }
            
            return _mapper.Map<CreatePostResponse>(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting post by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var userRole = GetCurrentUserRole();
                
                if (userRole != "Staff" && userRole != "Admin")
                {
                    _logger.LogWarning("Tài khoản này không có quyền hạn sử dụng: {Role}", userRole);
                    return _mapper.Map<CreatePostResponse>(null);
                }
                
                var existingPost = await _unitOfWork.GetRepository<Post>()
                    .FirstOrDefaultAsync(predicate: p => p.Title == request.Title,
                        include: p => p.Include(post => post.Staff));

                if (existingPost != null)
                {
                    _logger.LogWarning("Post with title {Title} already exists", request.Title);
                }
                
                if (request == null)
                {
                    _logger.LogWarning("request is null");
                    throw new ArgumentNullException(nameof(request), "request cannot be null");
                }
                
                var post = _mapper.Map<Post>(request);
                post.Status = PostStatus.Pending;
                post.StaffId = GetCurrentUserId();

                await _unitOfWork.GetRepository<Post>().InsertAsync(post);

                return _mapper.Map<CreatePostResponse>(post);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating post");
            throw;
        }
    }

    public async Task<CreatePostResponse> UpdatePostAsync(Guid id, UpdatePostRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var currentUserId = GetCurrentUserId();

                
                var userRole = GetCurrentUserRole();
                if (userRole != "Staff" && userRole != "Admin")
                {
                    _logger.LogWarning("Tài khoản này không có quyền hạn sử dụng: {Role}", userRole);
                    return _mapper.Map<CreatePostResponse>(null);
                }

                var existingPost = await _unitOfWork.GetRepository<Post>()
                    .FirstOrDefaultAsync(predicate: p => p.Id == id,
                        include: p => p.Include(post => post.Staff));

                if (existingPost == null)
                {
                    _logger.LogInformation("No post found");
                    throw new ArgumentNullException(nameof(existingPost), "Post not found");
                }
                
                _mapper.Map(request, existingPost);
                existingPost.StaffId = GetCurrentUserId();  
                
                _unitOfWork.GetRepository<Post>().UpdateAsync(existingPost);
                return _mapper.Map<CreatePostResponse>(existingPost);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating post with id: {Id}", id);
            throw;
        }
    }

    public async Task<CreatePostResponse> ApprovePostAsync(Guid id, PostStatus status)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole != "Manager")
                {
                    _logger.LogWarning("Tài khoản này không có quyền hạn sử dụng: {Role}", currentUserRole);
                }

                var post = await _unitOfWork.GetRepository<Post>()
                    .FirstOrDefaultAsync(predicate: p => p.Id == id,
                        include: p => p.Include(post => post.Staff));

                if (status == PostStatus.Approved && post.Status == PostStatus.Approved)
                {
                    _logger.LogWarning("Post with id {Id} is already approved", id);
                }

                if (post == null)
                {
                    _logger.LogInformation("No post found for approval");
                }

                post.Status = status;
                
                _unitOfWork.GetRepository<Post>().UpdateAsync(post);
                return _mapper.Map<CreatePostResponse>(post);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving post with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var post = await _unitOfWork.GetRepository<Post>()
                    .FirstOrDefaultAsync(predicate: p => p.Id == id,
                        include: p => p.Include(post => post.Staff));
                
                if (post == null) return false;

                _unitOfWork.GetRepository<Post>().DeleteAsync(post);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting post with id: {Id}", id);
            throw;
        }
    }
} 