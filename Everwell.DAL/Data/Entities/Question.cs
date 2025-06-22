using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

public enum QuestionStatus
{
    Pending,     // Question submitted, waiting for consultant
    Assigned,    // Question assigned to a consultant
    Answered,    // Question answered by consultant
    Closed       // Question closed/resolved
}

[Table("Questions")]
public class Question
{
    [Key]
    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [Required]
    [Column("customer_id")]
    public Guid CustomerId { get; set; }
    public virtual User Customer { get; set; }

    [Column("consultant_id")]
    public Guid? ConsultantId { get; set; }
    public virtual User? Consultant { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; }

    [Required]
    [Column("question_text", TypeName = "text")]
    public string QuestionText { get; set; }

    [Column("answer_text", TypeName = "text")]
    public string? AnswerText { get; set; }

    [Required]
    [Column("status")]
    public QuestionStatus Status { get; set; } = QuestionStatus.Pending;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("answered_at")]
    public DateTime? AnsweredAt { get; set; }
}