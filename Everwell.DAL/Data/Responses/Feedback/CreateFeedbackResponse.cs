namespace Everwell.DAL.Data.Responses.Feedback;

public class CreateFeedbackResponse
{
    public Guid FeedbackId { get; set; }
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
} 