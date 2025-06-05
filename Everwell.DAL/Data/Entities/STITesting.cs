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
    
    public enum TestType
    {
        Hiv,
        Syphilis,
        Gonorrhea,
        Chlamydia,
        HepatitisB,
        HepatitisC
    }
    
    public enum Method
    {
        BloodTest,
        UrineTest,
        SwabTest,
        RapidTest
    }
    
    [Table("STITesting")]
    public class STITesting
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("appointment_id")]
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }
        public virtual User Customer { get; set; }

        [Required]
        [Column("test_type")]
        public TestType TestType { get; set; }
        
        [Required]
        [Column("method")]
        public Method Method { get; set; }

        [Required]
        [Column("status")]
        public Status Status { get; set; } = Status.Pending;

        [Column("collected_date", TypeName = "date")]
        public DateOnly? CollectedDate { get; set; }

        // Navigation to Test Results
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
