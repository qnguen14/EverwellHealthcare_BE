using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

public enum MenstrualCyclePhase
{
    Menstrual,   // Day 1-5 (bleeding phase)
    Follicular,  // Day 1-13 (egg development - overlaps with menstrual)
    Ovulation,   // Day 14 ±2 (egg release)
    Luteal       // Day 15-28 (after ovulation)
}

[Table("MenstrualCycleNotification")]
public class MenstrualCycleNotification
{
    [Key]
    [Column("notification_id")]
    public Guid NotificationId { get; set; }

    [Required]
    [Column("tracking_id")]
    public Guid TrackingId { get; set; }
    public virtual MenstrualCycleTracking Tracking { get; set; }

    [Required]
    [Column("phase")]
    public MenstrualCyclePhase Phase { get; set; }

    [Required]
    [Column("sent_at")]
    public DateTime SentAt { get; set; }

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; }

    [Column("is_sent")]
    public bool IsSent { get; set; } = false;
}