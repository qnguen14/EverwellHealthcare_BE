using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Responses.Questions;

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public Guid? ConsultantId { get; set; }
    public string? ConsultantName { get; set; }
    public string? ConsultantEmail { get; set; }
    public string Title { get; set; }
    public string QuestionText { get; set; }
    public string? AnswerText { get; set; }
    public QuestionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
} 