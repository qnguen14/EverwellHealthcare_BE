using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Questions;

public class CreateQuestionRequest
{
    public Guid? ConsultantId { get; set; } // Optional - for when customer wants specific consultant

    [Required(ErrorMessage = "Title is required")]
    [StringLength(255, ErrorMessage = "Title must be at most 255 characters")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    [StringLength(2000, ErrorMessage = "Question text must be at most 2000 characters")]
    public string QuestionText { get; set; }
} 