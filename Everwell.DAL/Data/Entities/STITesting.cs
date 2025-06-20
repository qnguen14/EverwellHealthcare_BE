using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Entities
{
    public enum TestingStatus
    {
        Scheduled,      // Test scheduled
        SampleTaken,    // Sample collected
        Processing,     // Lab processing
        Completed,      // Test completed
        Cancelled       // Test cancelled
    }
    
    public enum TestPackage
    {
        Basic,          // Cơ bản - Basic package (Chlamydia, Gonorrhea, Syphilis)
        Advanced,       // Nâng cao - Full package (Basic + HIV, Herpes, Hepatitis)
        Custom          // Tùy chỉnh - Custom package (User-defined tests)
    }
    
    [Table("STITesting")]
    public class STITesting
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
        [Column("test_package")]
        public TestPackage TestPackage { get; set; }

        [Required]
        [Column("status")]
        public TestingStatus? Status { get; set; } = TestingStatus.Scheduled;

        [Required]
        [Column("schedule_date", TypeName = "date")]
        public DateOnly? ScheduleDate { get; set; }

        [Required]
        [Column("slot")]
        public ShiftSlot Slot { get; set; }

        [Column("notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Column("sample_taken_at")]
        public DateTime? SampleTakenAt { get; set; }
        
        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column("total_price")]
        public decimal TotalPrice { get; set; }
        
        [Required]
        [Column ("is_paid")]
        public bool IsPaid { get; set; } = false;
        
        // Navigation to Test Results
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
