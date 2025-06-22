using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Feedback;

public class UpdateFeedbackRequest
{
    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [Required(ErrorMessage = "Comment is required")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    [MinLength(10, ErrorMessage = "Comment must be at least 10 characters")]
    public string Comment { get; set; }
} 