using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.Post;

public class UpdatePostRequest
{
    [Required]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; }

    [Required]
    [StringLength(10000, ErrorMessage = "Content cannot exceed 10000 characters.")]
    public string Content { get; set; }
        
    [Required(ErrorMessage = "Image URL is required")]
    public string ImageUrl { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [EnumDataType(typeof(PostCategory))]
    public PostCategory Category { get; set; } 
}