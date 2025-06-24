 using Everwell.DAL.Data.Entities;
 using Everwell.DAL.Data.Requests.Post;
 using Everwell.DAL.Data.Responses.Post;

 namespace Everwell.BLL.Services.Interfaces;

public interface IPostService
{
    Task<IEnumerable<CreatePostResponse>> GetAllPostsAsync();
    Task<IEnumerable<CreatePostResponse>> GetFilteredPosts(FilterPostsRequest request);
    Task<CreatePostResponse> GetPostByIdAsync(Guid id);
    Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request);
    Task<CreatePostResponse> UpdatePostAsync(Guid id, UpdatePostRequest request);
    Task<CreatePostResponse> ApprovePostAsync(Guid id, PostStatus status);
    Task<bool> DeletePostAsync(Guid id);
}