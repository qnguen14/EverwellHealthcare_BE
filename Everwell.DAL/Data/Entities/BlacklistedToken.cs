using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities
{
    [Table("BlacklistedTokens")]
    public class BlacklistedToken
    {
        [Key]
        [Required]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("tokenHash")]
        [StringLength(256)]
        public string TokenHash { get; set; }

        [Required]
        [Column("blacklistedAt")]
        public DateTime BlacklistedAt { get; set; }

        [Required]
        [Column("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        // Helper method to set times in UTC+7
        public static DateTime ToUtcPlus7(DateTime utcTime)
        {
            var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, utcPlus7);
        }

        public static DateTime GetCurrentUtcPlus7()
        {
            return ToUtcPlus7(DateTime.UtcNow);
        }
    }
}