using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;

namespace Everwell.DAL.Data.Entities;


[Table("Feedback")]
public class Feedback
{
    [Key]
    [Required]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [Column("id")]
    [ForeignKey("Customer")]
    public Guid CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
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
    [ForeignKey("Service")]
    [Column("appoinement_id")]
    public Guid AppointmentId { get; set; }
    // public virtual Appointment Appointment { get; set; }
    
    [Required]
    [Column("rating")]
    public int Rating { get; set; }
    
    [Required]
    [Column("comment")]
    public string Comment { get; set; }
    
    [Required]
    [Column("created_at")]
    public DateOnly CreatedAt { get; set; }
}