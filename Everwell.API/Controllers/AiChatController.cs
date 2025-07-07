using Everwell.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers;

[ApiController]
[Route("api/v2.5/chat/gemini")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;
    private readonly ILogger<AiChatController> _logger;

    public AiChatController(IAiChatService aiChatService, ILogger<AiChatController> logger)
    {
        _aiChatService = aiChatService;
        _logger = logger;
    }

    public record PromptDto(string Message);
    public record ValidateDomainRequest(string Prompt);

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] PromptDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { message = "Prompt cannot be empty" });

        try
        {
            var answer = await _aiChatService.AskAsync(dto.Message);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Gemini service error", detail = ex.Message });
        }
    }

    [HttpPost("validate-domain")]
    public IActionResult ValidateDomain([FromBody] ValidateDomainRequest request)
    {
        try
        {
            var result = new
            {
                prompt = request.Prompt,
                isHealthRelated = IsHealthRelatedPrompt(request.Prompt),
                isEmergency = IsEmergencyPrompt(request.Prompt),
                detectedKeywords = GetDetectedKeywords(request.Prompt),
                recommendation = GetRecommendation(request.Prompt)
            };

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating domain for prompt: {Prompt}", request.Prompt);
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi validate domain" });
        }
    }

    private bool IsHealthRelatedPrompt(string prompt)
    {
        var lowerPrompt = prompt.ToLower();
        
        // Non-health topics (blacklist)
        var nonHealthTopics = new[]
        {
            "việt nam", "đất nước", "địa hình", "núi", "sông", "biển", "thành phố", "tỉnh",
            "du lịch", "điểm đến", "văn hóa", "lịch sử", "truyền thống",
            "bóng đá", "thể thao", "game", "phim", "nhạc", "ca sĩ", "diễn viên",
            "máy tính", "lập trình", "code", "website", "internet", "facebook",
            "kinh doanh", "tiền", "đầu tư", "ngân hàng", "chứng khoán",
            "học", "trường", "đại học", "thi cử", "bài tập"
        };
        
        if (nonHealthTopics.Any(topic => lowerPrompt.Contains(topic)))
        {
            return false;
        }
        
        // Health-related keywords (whitelist)
        var healthKeywords = new[]
        {
            "chu kỳ", "kinh nguyệt", "rụng trứng", "thai", "sinh sản", "phụ khoa", "sản khoa",
            "âm đạo", "tử cung", "buồng trứng", "hormone", "estrogen", "progesterone",
            "sti", "hiv", "aids", "giang mai", "lậu", "chlamydia", "herpes", "hpv", "xét nghiệm",
            "nhiễm trùng", "bệnh lây", "tình dục", "quan hệ",
            "đau bụng", "ra máu", "tiết dịch", "ngứa", "đau", "triệu chứng", "bệnh",
            "sức khỏe", "khám", "bác sĩ", "tư vấn", "điều trị", "thuốc",
            "tránh thai", "bao cao su", "thuốc tránh thai", "vòng tránh thai",
            "kế hoạch", "mang thai", "có thai", "thụ thai",
            "pms", "đau đầu", "căng thẳng", "tâm trạng", "trầm cảm", "lo âu",
            "everwell", "đặt lịch", "hẹn", "ứng dụng", "app", "theo dõi"
        };
        
        return healthKeywords.Any(keyword => lowerPrompt.Contains(keyword));
    }

    private bool IsEmergencyPrompt(string prompt)
    {
        var emergencyKeywords = new[]
        {
            "đau dữ dội", "ra máu nhiều", "không thở được", "ngất xỉu", 
            "cấp cứu", "khẩn cấp", "nguy hiểm", "sốt cao", "chảy máu"
        };
        
        return emergencyKeywords.Any(keyword => prompt.ToLower().Contains(keyword.ToLower()));
    }

    private List<string> GetDetectedKeywords(string prompt)
    {
        var lowerPrompt = prompt.ToLower();
        var allKeywords = new[]
        {
            // Health keywords
            "chu kỳ", "kinh nguyệt", "sức khỏe", "bác sĩ", "đau", "everwell",
            // Non-health keywords  
            "việt nam", "lịch sử", "văn hóa", "thể thao", "công nghệ"
        };
        
        return allKeywords.Where(keyword => lowerPrompt.Contains(keyword)).ToList();
    }

    private string GetRecommendation(string prompt)
    {
        if (!IsHealthRelatedPrompt(prompt))
        {
            return "REDIRECT: Sử dụng off-topic response";
        }
        
        if (IsEmergencyPrompt(prompt))
        {
            return "EMERGENCY: Sử dụng emergency response";
        }
        
        return "PROCESS: Câu hỏi hợp lệ, xử lý bằng AI";
    }
} 