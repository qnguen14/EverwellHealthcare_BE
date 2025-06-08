using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

[Table("ConsultantSchedule")]
public class ConsultantSchedule
{
    [Key]
    [Column("schedule_id")]
    public Guid Id { get; set; }

    [Required]
    [Column("consultant_id")]
    [ForeignKey("Consultant")]
    public Guid ConsultantId { get; set; }
    public virtual User Consultant { get; set; }

    [Required]
    [Column("work_date")]
    public DateOnly WorkDate { get; set; }

    [Required]
    [Column("shift_slot")]
    public ShiftSlot Slot { get; set; }

    [Required]
    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}