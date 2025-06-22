namespace Everwell.DAL.Data.Responses.Feedback;

public class FeedbackResponse
{
    public Guid Id { get; set; }
    
    // Customer information
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerAvatar { get; set; }
    
    // Consultant information
    public Guid ConsultantId { get; set; }
    public string ConsultantName { get; set; }
    public string ConsultantEmail { get; set; }
    public string? ConsultantAvatar { get; set; }
    public string? ConsultantSpecialization { get; set; }
    
    // Appointment information
    public Guid AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentStatus { get; set; }
    
    // Feedback details
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateOnly CreatedAt { get; set; }
} 