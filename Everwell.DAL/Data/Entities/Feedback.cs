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
    [Column("appointment_id")]
    [ForeignKey("Appointment")]
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; }
    
    [Required]
    [Column("rating")]
    public int Rating { get; set; }
    
    [Column("comment", TypeName = "text")]
    public string Comment { get; set; }
    
    [Required]
    [Column("created_at")]
    public DateOnly CreatedAt { get; set; }
}