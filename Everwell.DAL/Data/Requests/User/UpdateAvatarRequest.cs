using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.User
{
    public class UpdateAvatarRequest
    {
        [Required(ErrorMessage = "Avatar URL is required")]
        [MaxLength(1000, ErrorMessage = "Avatar URL cannot be more than 1000 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string AvatarUrl { get; set; }
    }
} 