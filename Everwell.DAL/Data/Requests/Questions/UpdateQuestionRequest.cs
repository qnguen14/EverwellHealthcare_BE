using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.Questions;

public class UpdateQuestionRequest
{
    [StringLength(255, ErrorMessage = "Title must be at most 255 characters")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "Question text must be at most 2000 characters")]
    public string? QuestionText { get; set; }

    [StringLength(5000, ErrorMessage = "Answer text must be at most 5000 characters")]
    public string? AnswerText { get; set; }

    public QuestionStatus? Status { get; set; }
} 