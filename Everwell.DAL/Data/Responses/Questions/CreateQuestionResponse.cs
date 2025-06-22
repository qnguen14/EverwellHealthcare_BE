namespace Everwell.DAL.Data.Responses.Questions;

public class CreateQuestionResponse
{
    public Guid QuestionId { get; set; }
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
} 