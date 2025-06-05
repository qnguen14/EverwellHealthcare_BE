using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;


public enum AppointmentStatus
{
    Scheduled,
    Completed,
    Cancelled,
    NoShow
}

[Table("Appointment")]
public class Appointment
{
    [Key]
    [Required]
    [Column("appointment_id")]
    public Guid AppointmentId { get; set; }

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
    [Column("service_id")]
    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; }

    [Required]
    [Column("appointment_date")]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly EndTime { get; set; }

    [Required]
    [Column("status")]
    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}