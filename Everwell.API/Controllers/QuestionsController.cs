using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet(ApiEndpointConstants.Question.GetAllQuestionsEndpoint)]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Question>>> GetAllQuestions()
    {
        try
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Question.GetQuestionEndpoint)]
    [Authorize]
    public async Task<ActionResult<Question>> GetQuestionById(Guid id)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });
            
            return Ok(question);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
} 