using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;


public enum AppointmentStatus
{
    Scheduled,
    Completed,
    Cancelled,
    NoShow,
    Temp
}

public enum ShiftSlot
{
    Morning1, // 8:00 AM - 10:00 AM
    Morning2, // 10:00 AM - 12:00 PM
    Afternoon1, // 1:00 PM - 3:00 PM
    Afternoon2, // 3:00 PM - 5:00 PM
}

[Table("Appointment")]
public class Appointment
{
    [Key]
    [Required]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("customer_id")]
    [ForeignKey("Customer")]
    public Guid CustomerId { get; set; }    
    public virtual User Customer { get; set; }

    [Required]
    [Column("consultant_id")]
    [ForeignKey("Consultant")]
    public Guid ConsultantId { get; set; }
    public virtual User Consultant { get; set; }

    [Required]
    [Column("appointment_date")]
    public DateOnly AppointmentDate { get; set; }
    
    [Required]
    [Column("shift_slot")]
    public ShiftSlot Slot { get; set; }

    [Required]
    [Column("status")]
    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    // Virtual meeting properties for Jitsi Meet
    [Column("google_meet_url")]
    public string? GoogleMeetLink { get; set; }

    [Column("google_event_id")]
    public string? GoogleEventId { get; set; }

    [Column("meeting_id")]
    public string? MeetingId { get; set; }

    [Column("is_virtual")]
    public bool IsVirtual { get; set; } = false;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Actual join/leave times (UTC)
    [Column("check_in_utc")]
    public DateTime? CheckInTimeUtc { get; set; }

    [Column("check_out_utc")]
    public DateTime? CheckOutTimeUtc { get; set; }
}