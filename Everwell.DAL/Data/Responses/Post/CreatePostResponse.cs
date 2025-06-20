using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.Post;

public class CreatePostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public PostStatus Status { get; set; }
    public PostCategory Category { get; set; }
    public Guid StaffId { get; set; }
    public GetUserResponse Staff { get; set; }
    public DateTime CreatedAt { get; set; }
}