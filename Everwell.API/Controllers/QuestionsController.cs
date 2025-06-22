using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Questions;
using Everwell.DAL.Data.Responses.Questions;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetAllQuestions()
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
    public async Task<ActionResult<QuestionResponse>> GetQuestionById(Guid id)
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

    [HttpGet(ApiEndpointConstants.Question.QuestionEndpoint + "/customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestionsByCustomer(Guid customerId)
    {
        try
        {
            var questions = await _questionService.GetQuestionsByCustomerAsync(customerId);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Question.QuestionEndpoint + "/consultant/{consultantId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestionsByConsultant(Guid consultantId)
    {
        try
        {
            var questions = await _questionService.GetQuestionsByConsultantAsync(consultantId);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Question.QuestionEndpoint + "/unassigned")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetUnassignedQuestions()
    {
        try
        {
            var questions = await _questionService.GetUnassignedQuestionsAsync();
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost(ApiEndpointConstants.Question.CreateQuestionEndpoint)]
    [Authorize]
    public async Task<ActionResult<CreateQuestionResponse>> CreateQuestion(CreateQuestionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _questionService.CreateQuestionAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Question.UpdateQuestionEndpoint)]
    [Authorize]
    public async Task<ActionResult<QuestionResponse>> UpdateQuestion(Guid id, UpdateQuestionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _questionService.UpdateQuestionAsync(id, request);
            if (result == null)
                return NotFound(new { message = "Question not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Question.QuestionEndpoint + "/assign/{questionId}/consultant/{consultantId}")]
    [Authorize]
    public async Task<ActionResult<QuestionResponse>> AssignQuestionToConsultant(Guid questionId, Guid consultantId)
    {
        try
        {
            var result = await _questionService.AssignQuestionToConsultantAsync(questionId, consultantId);
            if (result == null)
                return NotFound(new { message = "Question not found" });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut(ApiEndpointConstants.Question.QuestionEndpoint + "/answer/{id}")]
    [Authorize]
    public async Task<ActionResult<QuestionResponse>> AnswerQuestion(Guid id, [FromBody] string answer)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(answer))
                return BadRequest(new { message = "Answer cannot be empty" });

            var result = await _questionService.AnswerQuestionAsync(id, answer);
            if (result == null)
                return NotFound(new { message = "Question not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete(ApiEndpointConstants.Question.DeleteQuestionEndpoint)]
    [Authorize]
    public async Task<ActionResult> DeleteQuestion(Guid id)
    {
        try
        {
            var result = await _questionService.DeleteQuestionAsync(id);
            if (!result)
                return NotFound(new { message = "Question not found" });

            return Ok(new { message = "Question deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet(ApiEndpointConstants.Question.QuestionEndpoint + "/debug/consultants")]
    [Authorize]
    public async Task<ActionResult> DebugConsultants()
    {
        try
        {
            // This is a temporary debug endpoint
            var unitOfWork = HttpContext.RequestServices.GetRequiredService<IUnitOfWork<EverwellDbContext>>();
            
            var allUsers = await unitOfWork.GetRepository<User>()
                .GetListAsync(include: u => u.Include(user => user.Role));
            
            var consultants = await unitOfWork.GetRepository<User>()
                .GetListAsync(
                    predicate: u => u.RoleId == (int)RoleName.Consultant && u.IsActive,
                    include: u => u.Include(user => user.Role));

            return Ok(new
            {
                TotalUsers = allUsers.Count(),
                AllUsers = allUsers.Select(u => new { u.Id, u.Name, u.Email, u.RoleId, RoleName = u.Role?.Name, u.IsActive }),
                ConsultantRoleId = (int)RoleName.Consultant,
                ConsultantsFound = consultants.Count(),
                Consultants = consultants.Select(c => new { c.Id, c.Name, c.Email, c.RoleId, RoleName = c.Role?.Name, c.IsActive })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Debug error", details = ex.Message });
        }
    }
} 