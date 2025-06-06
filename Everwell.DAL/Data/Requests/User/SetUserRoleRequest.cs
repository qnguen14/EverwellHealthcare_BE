using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.User
{
    public class SetUserRoleRequest
    {
        [Required(ErrorMessage = "Role field is required")]
        public string Role { get; set; }
    }
} 