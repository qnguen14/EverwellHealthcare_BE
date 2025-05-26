using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Requests.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Name field is required")]
        [MaxLength(100, ErrorMessage = "Name cannot be more than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email field is required")]
        [MaxLength(256, ErrorMessage = "Email cannot be more than 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number field is required")]
        [MaxLength(10, ErrorMessage = "Phone number cannot be more than 10 characters")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address field is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Password field is required")]
        [MaxLength(20, ErrorMessage = "Password cannot be more than 20 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role field is required")]
        public string Role { get; set; }
    }
}
