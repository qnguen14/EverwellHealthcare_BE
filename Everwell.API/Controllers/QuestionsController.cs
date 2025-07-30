// ============================================================================
// QUESTIONS CONTROLLER
// ============================================================================
// This controller manages the Q&A system, knowledge base, and expert consultations
// It handles question submission, expert answers, and community knowledge sharing
// 
// Q&A SYSTEM FLOW:
// 1. QUESTION SUBMISSION: Users submit health-related questions
// 2. QUESTION MODERATION: Staff reviews for appropriateness and medical relevance
// 3. EXPERT ASSIGNMENT: Questions routed to appropriate healthcare professionals
// 4. EXPERT RESPONSE: Medical experts provide evidence-based answers
// 5. ANSWER REVIEW: Medical team validates answer accuracy and completeness
// 6. PUBLICATION: Approved Q&A pairs added to knowledge base
// 7. COMMUNITY ACCESS: Users search and browse answered questions
// 
// QUESTION CATEGORIES:
// - Reproductive health: Menstrual cycles, fertility, contraception
// - STI concerns: Testing, symptoms, treatment, prevention
// - General wellness: Lifestyle, nutrition, mental health
// - Preventive care: Screening schedules, health maintenance
// - Symptom inquiries: Health concerns, when to seek care
// 
// EXPERT NETWORK:
// - Gynecologists: Reproductive health specialists
// - General practitioners: Primary care physicians
// - Mental health professionals: Counselors and therapists
// - Nutritionists: Diet and wellness experts
// - Pharmacists: Medication and supplement guidance
// 
// KNOWLEDGE BASE FEATURES:
// - Searchable Q&A database
// - Category-based organization
// - Expert-verified medical information
// - Related question suggestions
// - Popular questions highlighting
// 
// QUALITY ASSURANCE:
// - Medical accuracy verification by licensed professionals
// - Regular content updates based on latest guidelines
// - User feedback integration for answer improvement
// - Disclaimer and medical advice limitations
// 
// PRIVACY & SAFETY:
// - Anonymous question submission option
// - Personal information protection
// - Emergency situation identification and referral
// - Professional medical consultation recommendations

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

    /// <summary>
    /// GET ALL QUESTIONS
    /// =================
    /// Retrieves all questions from the knowledge base and Q&A system
    /// 
    /// KNOWLEDGE BASE ACCESS:
    /// - Expert-answered questions with verified medical information
    /// - Community Q&A database for health education
    /// - Frequently asked questions about women's health
    /// - Categorized questions for easy browsing
    /// 
    /// QUESTION TYPES:
    /// - Answered questions: Expert-verified responses available
    /// - Popular questions: Most viewed and helpful content
    /// - Recent questions: Latest additions to knowledge base
    /// - Category-specific: Organized by health topics
    /// 
    /// EDUCATIONAL VALUE:
    /// - Evidence-based medical information
    /// - Expert insights from healthcare professionals
    /// - Common health concerns and solutions
    /// - Preventive care guidance and recommendations
    /// 
    /// USE CASES:
    /// - Health education: Learning about women's health topics
    /// - Self-assessment: Understanding symptoms and concerns
    /// - Preventive care: Guidance on health maintenance
    /// - Decision support: Information for healthcare decisions
    /// - Community learning: Shared health knowledge
    /// 
    /// PUBLIC ACCESS:
    /// - No authentication required for knowledge base
    /// - Promotes health literacy and awareness
    /// - Supports informed healthcare decisions
    /// </summary>
    [HttpGet(ApiEndpointConstants.Question.GetAllQuestionsEndpoint)]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetAllQuestions()
    {
        try
        {
            // Retrieve all questions from the knowledge base
            // Includes expert-answered questions and community Q&A
            var questions = await _questionService.GetAllQuestionsAsync();
            return Ok(questions); // Knowledge base Q&A, expert answers, health education
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