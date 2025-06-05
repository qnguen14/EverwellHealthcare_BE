 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IPostService
{
    Task<IEnumerable<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<Post> CreatePostAsync(Post post);
    Task<Post?> UpdatePostAsync(Guid id, Post post);
    Task<bool> DeletePostAsync(Guid id);
}