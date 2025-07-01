using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Feedback;

public class CreateFeedbackRequest
{
    public Guid? ConsultantId { get; set; }
    
    [Required(ErrorMessage = "Appointment ID is required")]
    public Guid AppointmentId { get; set; }
    
    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [Required(ErrorMessage = "Comment is required")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    [MinLength(1, ErrorMessage = "Comment must be at least 1 character")]
    public string Comment { get; set; }
} 