using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Responses.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Role Role { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Statistics (can be useful for user profile)
        public int TotalPosts { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalSTITests { get; set; }
    }
} 