using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Entities
{
    public enum Status
    {
        Pending,
        InProgress,
        Completed
    }

    [Table("STITesting")]
    public class STITesting
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("appointment_id")]
        [ForeignKey("AppointmentId")]
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }

        [Required]
        [StringLength(100)] // Sets the maximum string length, matching VARCHAR(100)
        [Column("test_type")]
        public string TestType { get; set; }

        [Required]
        [Column("status")]
        public Status Status { get; set; } = Status.Pending;

        [Column("collected_date", TypeName = "date")]
        public DateOnly? CollectedDate { get; set; }
    }
}
