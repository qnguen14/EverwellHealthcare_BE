using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services;
using Microsoft.AspNetCore.Http;

namespace Everwell.BLL.Services.Implements;

public class PostService : BaseService<PostService>, IPostService
{
    public PostService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<PostService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync()
    {
        try
        {
            var posts = await _unitOfWork.GetRepository<Post>()
                .GetListAsync(
                    include: p => p.Include(post => post.Staff));
            
            return posts ?? new List<Post>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all posts");
            throw;
        }
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<Post>()
                .FirstOrDefaultAsync(
                    predicate: p => p.Id == id,
                    include: p => p.Include(post => post.Staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting post by id: {Id}", id);
            throw;
        }
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                post.Id = Guid.NewGuid();
                post.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                
                await _unitOfWork.GetRepository<Post>().InsertAsync(post);
                return post;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating post");
            throw;
        }
    }

    public async Task<Post?> UpdatePostAsync(Guid id, Post post)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingPost = await _unitOfWork.GetRepository<Post>()
                    .FirstOrDefaultAsync(predicate: p => p.Id == id);
                
                if (existingPost == null) return null;

                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.Status = post.Status;
                existingPost.Category = post.Category;
                
                _unitOfWork.GetRepository<Post>().UpdateAsync(existingPost);
                return existingPost;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating post with id: {Id}", id);
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
                    .FirstOrDefaultAsync(predicate: p => p.Id == id);
                
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