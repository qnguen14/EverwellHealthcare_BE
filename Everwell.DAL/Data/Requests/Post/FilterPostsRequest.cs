using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.Post;

public class FilterPostsRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public PostStatus? Status { get; set; }
    public PostCategory? Category { get; set; }
    public Guid? Staffid { get; set; }
    public DateTime? CreatedAt { get; set; }
}