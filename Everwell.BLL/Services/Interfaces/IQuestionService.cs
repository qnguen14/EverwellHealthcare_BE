using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Questions;
using Everwell.DAL.Data.Responses.Questions;

namespace Everwell.BLL.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionResponse>> GetAllQuestionsAsync();
    Task<IEnumerable<QuestionResponse>> GetQuestionsByCustomerAsync(Guid customerId);
    Task<IEnumerable<QuestionResponse>> GetQuestionsByConsultantAsync(Guid consultantId);
    Task<IEnumerable<QuestionResponse>> GetUnassignedQuestionsAsync(); // Questions waiting for consultant assignment
    Task<QuestionResponse?> GetQuestionByIdAsync(Guid id);
    Task<CreateQuestionResponse> CreateQuestionAsync(CreateQuestionRequest request);
    Task<QuestionResponse?> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request);
    Task<QuestionResponse?> AssignQuestionToConsultantAsync(Guid questionId, Guid consultantId); // Consultant claims question
    Task<QuestionResponse?> AnswerQuestionAsync(Guid id, string answer);
    Task<bool> DeleteQuestionAsync(Guid id);
}