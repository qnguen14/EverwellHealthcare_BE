using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities
{
    public enum TestParameter
    {
        Chlamydia,                  // Vi khuẩn Chlamydia
        Candida,                    // Nấm Candida
        Treponema,                  // Xoắn khuẩn gây bệnh giang mai
        HerpesSimplexType1,         // Virus Herpes Simplex 1
        HerpesSimplexType2,         // Virus Herpes Simplex 2
        UreaplasmaParv,             // Vi khuẩn Ureaplasma parvum
        Trichomonas,                // Trùng roi âm đạo
        MycoplasmaGenitalium,       // Vi khuẩn Mycoplasma genitalium
        MycoplasmaHominis,          // Vi khuẩn Mycoplasma hominis
        Gonorrhoeae,                // Vi khuẩn lậu cầu
        UreaplasmaUrealyticum,      // Vi khuẩn Ureaplasma urealyticum
        Haemophilus,                // Vi khuẩn gây bệnh hạ cam
        Gardnerella,                // Vi khuẩn Gardnerella vaginalis
        HIV,                        // HIV 1+2 gộp
        HIVCombo,                   // HIV Combo Ag + Ab
        GenitalHPV,                 // Kháng thể kháng đặc hiệu giang mai
        HPV                         // HPV 40 kiểu từ tự lấy mẫu
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
        public Guid STITestingId { get; set; }
        public virtual STITesting STITesting { get; set; }

        [Required]
        [Column("parameter")]
        public TestParameter[] Parameter { get; set; } = Array.Empty<TestParameter>();
        
        [Required]
        [Column("outcome")]
        public ResultOutcome Outcome { get; set; } = ResultOutcome.Pending;

        [Column("comments")]
        [StringLength(500)]
        public string? Comments { get; set; }

        [Column("staff_id")]
        public Guid? StaffId { get; set; }
        public virtual User? Staff { get; set; }

        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }
    }
}