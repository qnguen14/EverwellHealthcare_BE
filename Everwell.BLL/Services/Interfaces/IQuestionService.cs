 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<Question>> GetAllQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(Guid id);
    Task<Question> CreateQuestionAsync(Question question);
    Task<Question?> UpdateQuestionAsync(Guid id, Question question);
    Task<bool> DeleteQuestionAsync(Guid id);
}