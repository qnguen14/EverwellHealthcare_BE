using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

[Table("MenstrualCycleTracking")]
public class MenstrualCycleTracking
{
    [Key]
    [Column("tracking_id")]
    public Guid TrackingId { get; set; }

    [Required]
    [Column("customer_id")]
    public Guid CustomerId { get; set; }
    public virtual User Customer { get; set; }

    [Required]
    [Column("cycle_start_date", TypeName = "date")]
    public DateTime CycleStartDate { get; set; }

    [Column("cycle_end_date", TypeName = "date")]
    public DateTime? CycleEndDate { get; set; }

    [Column("symptoms", TypeName = "text")]
    public string? Symptoms { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Notification Preferences
    [Column("notify_before_days")]
    public int? NotifyBeforeDays { get; set; }

    [Column("notification_enabled")]
    public bool NotificationEnabled { get; set; } = false;

    public virtual ICollection<MenstrualCycleNotification> Notifications { get; set; }
}