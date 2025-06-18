using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Entities
{

    // public enum Role
    // {
    //     Customer,
    //     Consultant,
    //     Staff,
    //     Manager,
    //     Admin
    // }


    [Table("Users")]
    public class User
    {
        [Key]
        [Required]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("email")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [Column("phone_number")]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [Required]
        [Column("address")]
        public string Address { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        [Column("avatar_url")]
        [StringLength(1000)]
        public string? AvatarUrl { get; set; } = null;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        
        public virtual ICollection<STITesting> STITests { get; set; } = new List<STITesting>(); // For customers

        public virtual ICollection<TestResult> TestResultsExamined { get; set; } = new List<TestResult>(); // For staff who examine results
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}