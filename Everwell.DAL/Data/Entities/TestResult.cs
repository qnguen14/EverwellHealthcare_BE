using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities
{
    public enum TestParameter
    {
        Chlamydia,                  // Vi khuẩn Chlamydia
        Gonorrhoeae,                // Vi khuẩn lậu cầu
        Syphilis,                   // Vi khuẩn giang mai
        HIV,                        // HIV 1+2 gộp
        Herpes,                     // Herpes simplex virus (HSV1 + HSV2)
        HepatitisB,                 // Viêm gan B
        HepatitisC,                 // Viêm gan C
        Trichomonas,                // Ký sinh trùng Trichomonas vaginalis
        MycoplasmaGenitalium,       // Mycoplasma genitalium
        HPV,                        // Human Papillomavirus (HPV)
    }
    
    public enum ResultOutcome
    {
        Negative,      // Negative result (-)
        Positive,      // Positive result (+)
        Pending        // Result pending
    }

    [Table("TestResults")]
    public class TestResult
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("sti_testing_id")]
        [ForeignKey("STITesting")]
        public Guid STITestingId { get; set; }
        public virtual STITesting STITesting { get; set; }

        [Required]
        [Column("parameter")]
        public TestParameter Parameter { get; set; } 
        
        [Required]
        [Column("outcome")]
        public ResultOutcome? Outcome { get; set; } = ResultOutcome.Pending;

        [Column("comments")]
        [StringLength(500)]
        public string? Comments { get; set; }

        [Column("staff_id")]
        [ForeignKey("Staff")]
        public Guid? StaffId { get; set; }
        public virtual User? Staff { get; set; }

        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }
    }
}