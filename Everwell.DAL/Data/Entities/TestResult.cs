using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

public enum TestResultStatus
{
    Pending,
    Completed,
    Sent
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
    [Column("result_data", TypeName = "text")]
    public string ResultData { get; set; }

    [Required]
    [Column("status")]
    public TestResultStatus Status { get; set; } = TestResultStatus.Pending;

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }
    public virtual User Customer { get; set; }

    [Column("staff_id")]
    public Guid? StaffId { get; set; }
    public virtual User Staff { get; set; }

    [Column("examined_at")]
    public DateTime? ExaminedAt { get; set; }

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }
}