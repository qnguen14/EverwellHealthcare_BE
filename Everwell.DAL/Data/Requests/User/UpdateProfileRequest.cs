using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.User
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Name field is required")]
        [MaxLength(100, ErrorMessage = "Name cannot be more than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email field is required")]
        [MaxLength(256, ErrorMessage = "Email cannot be more than 256 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number field is required")]
        [MaxLength(15, ErrorMessage = "Phone number cannot be more than 15 characters")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address field is required")]
        public string Address { get; set; }
    }
} 