using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class QuestionService : BaseService<QuestionService>, IQuestionService
{
    public QuestionService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<QuestionService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
    {
        try
        {
            var questions = await _unitOfWork.GetRepository<Question>()
                .GetListAsync(
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
            
            return questions ?? new List<Question>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all questions");
            throw;
        }
    }

    public async Task<Question?> GetQuestionByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<Question>()
                .FirstOrDefaultAsync(
                    predicate: q => q.QuestionId == id,
                    include: q => q.Include(question => question.Customer)
                                   .Include(question => question.Consultant));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting question by id: {Id}", id);
            throw;
        }
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                question.QuestionId = Guid.NewGuid();
                question.CreatedAt = DateTime.UtcNow;
                
                await _unitOfWork.GetRepository<Question>().InsertAsync(question);
                return question;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating question");
            throw;
        }
    }

    public async Task<Question?> UpdateQuestionAsync(Guid id, Question question)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingQuestion = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(predicate: q => q.QuestionId == id);
                
                if (existingQuestion == null) return null;

                existingQuestion.Title = question.Title;
                existingQuestion.QuestionText = question.QuestionText;
                existingQuestion.AnswerText = question.AnswerText;
                existingQuestion.Status = question.Status;
                if (question.Status == QuestionStatus.Answered)
                {
                    existingQuestion.AnsweredAt = DateTime.UtcNow;
                }
                
                _unitOfWork.GetRepository<Question>().UpdateAsync(existingQuestion);
                return existingQuestion;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating question with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var question = await _unitOfWork.GetRepository<Question>()
                    .FirstOrDefaultAsync(predicate: q => q.QuestionId == id);
                
                if (question == null) return false;

                _unitOfWork.GetRepository<Question>().DeleteAsync(question);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting question with id: {Id}", id);
            throw;
        }
    }
} 