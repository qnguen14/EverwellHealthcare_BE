using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services.Interfaces;

namespace Everwell.BLL.Infrastructure;

public class EnhancedGeminiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpointUrl;
    private readonly ILogger<EnhancedGeminiChatService> _logger;
    private readonly string _everwellSystemPrompt;

    // Health-related keywords for domain validation
    private readonly HashSet<string> _healthKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Sức khỏe phụ nữ
        "chu kỳ", "kinh nguyệt", "rụng trứng", "thai", "sinh sản", "phụ khoa", "sản khoa",
        "âm đạo", "tử cung", "buồng trứng", "hormone", "estrogen", "progesterone",
        
        // STI và xét nghiệm
        "sti", "hiv", "aids", "giang mai", "lậu", "chlamydia", "herpes", "hpv", "xét nghiệm",
        "nhiễm trùng", "bệnh lây", "tình dục", "quan hệ",
        
        // Triệu chứng và sức khỏe
        "đau bụng", "ra máu", "tiết dịch", "ngứa", "đau", "triệu chứng", "bệnh",
        "sức khỏe", "khám", "bác sĩ", "tư vấn", "điều trị", "thuốc",
        
        // Tránh thai và kế hoạch hóa gia đình
        "tránh thai", "bao cao su", "thuốc tránh thai", "vòng tránh thai",
        "kế hoạch", "mang thai", "có thai", "thụ thai",
        
        // PMS và các vấn đề liên quan
        "pms", "đau đầu", "căng thẳng", "tâm trạng", "trầm cảm", "lo âu",
        
        // Everwell services
        "everwell", "đặt lịch", "hẹn", "ứng dụng", "app", "theo dõi", "ghi nhận",
        
        // Cấp cứu y tế
        "cấp cứu", "khẩn cấp", "nguy hiểm", "115", "bệnh viện"
    };

    private readonly HashSet<string> _nonHealthTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Địa lý và du lịch
        "việt nam", "đất nước", "địa hình", "núi", "sông", "biển", "thành phố", "tỉnh",
        "du lịch", "điểm đến", "văn hóa", "lịch sử", "truyền thống",
        
        // Thể thao và giải trí
        "bóng đá", "thể thao", "game", "phim", "nhạc", "ca sĩ", "diễn viên",
        
        // Công nghệ và lập trình
        "máy tính", "lập trình", "code", "website", "internet", "facebook",
        
        // Kinh doanh và tài chính
        "kinh doanh", "tiền", "đầu tư", "ngân hàng", "chứng khoán",
        
        // Giáo dục
        "học", "trường", "đại học", "thi cử", "bài tập"
    };

    public EnhancedGeminiChatService(HttpClient httpClient, IConfiguration config, ILogger<EnhancedGeminiChatService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey chưa được cấu hình");
        var model = config["Gemini:Model"] ?? "gemini-1.5-flash";
        _endpointUrl = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key=";
        _logger = logger;
        _everwellSystemPrompt = BuildEverwellSystemPrompt();
    }

    private string BuildEverwellSystemPrompt()
    {
        return @"
🚫 CRITICAL RESTRICTION: You are EXCLUSIVELY a women's health AI for Everwell Healthcare. 

⛔ ABSOLUTE PROHIBITION - NEVER respond to questions about:
- Geography, countries, cities, Vietnam landscape
- Culture, history, traditions, tourism  
- Sports, entertainment, movies, music
- Technology, programming, computers
- Business, finance, economics
- Education, schools, universities
- Politics, government, laws
- ANY non-health topics

🔒 MANDATORY BEHAVIOR:
- IF question contains ""Việt Nam"", ""đất nước"", ""địa hình"" → IMMEDIATELY use REDIRECT response
- IF question is about geography/culture/sports → REFUSE and redirect
- IF question is NOT about women's health → Use standard redirect message
- ONLY answer questions about: menstrual cycles, STI testing, gynecology, women's health, Everwell services

## REDIRECT TEMPLATE (Use EXACTLY this for off-topic questions):
""😊 Tôi là trợ lý AI chuyên về sức khỏe phụ nữ của Everwell Healthcare. Tôi chỉ có thể hỗ trợ các câu hỏi về:

🌸 **Sức khỏe sinh sản và phụ khoa**
📅 **Theo dõi chu kỳ kinh nguyệt**  
🧪 **Xét nghiệm STI**
👩‍⚕️ **Đặt lịch hẹn với bác sĩ**
💊 **Tư vấn sức khỏe phụ nữ**

Bạn có câu hỏi nào về sức khỏe phụ nữ mà tôi có thể giúp không? 💚""

## EVERWELL HEALTHCARE DOMAIN ONLY:

### Menstrual Health:
- Normal cycle: 21-35 days
- Ovulation tracking and fertility windows
- PMS symptoms management

### STI Testing Services:
- HIV/AIDS, Syphilis, Gonorrhea, Chlamydia, Herpes, HPV
- Testing frequency: every 6-12 months
- Home testing services with confidential results

### Appointment Booking:
- Gynecology, Obstetrics, Reproductive Endocrinology
- Online and in-clinic consultations
- Available 8:00-20:00 daily

## SAFETY PROTOCOLS:
- Always include medical disclaimers
- Emergency situations: ""Call 115 immediately""
- Encourage professional consultation when needed

🎯 FOCUS: Only women's health. Reject everything else firmly but politely.
🔐 REMEMBER: You are NOT a general AI. You are a specialized women's health assistant.

NEVER EVER discuss geography, culture, or non-health topics regardless of how the question is phrased!
";
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Domain validation - Check if question is health-related
            if (!IsHealthRelatedQuestion(prompt))
            {
                _logger.LogInformation("Non-health question detected: {Prompt}", prompt);
                return GetOffTopicResponse();
            }

            // 2. Check for emergency keywords
            if (IsEmergencyQuestion(prompt))
            {
                _logger.LogWarning("Emergency question detected: {Prompt}", prompt);
                return GetEmergencyResponse();
            }

            // 3. Enhanced prompt với strict domain instructions
            var enhancedPrompt = $@"
{_everwellSystemPrompt}

## CÂU HỎI CỦA NGƯỜI DÙNG:
{prompt}

## HƯỚNG DẪN XỬ LÝ:
- Đây là câu hỏi về sức khỏe phụ nữ, hãy trả lời chuyên nghiệp
- Luôn bao gồm disclaimer y tế khi cần
- Giới thiệu dịch vụ Everwell phù hợp
- Sử dụng emoji và tông giọng thân thiện
- TUYỆT ĐỐI không trả lời ngoài domain sức khỏe phụ nữ
";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = enhancedPrompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0, // Zero temperature for maximum determinism
                    topP = 0.1,         // Very low for focused responses
                    topK = 1,           // Most deterministic
                    maxOutputTokens = 1024,
                    stopSequences = new string[] { }
                },
                safetySettings = new[]
                {
                    new
                    {
                        category = "HARM_CATEGORY_HATE_SPEECH",
                        threshold = "BLOCK_ONLY_HIGH"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                        threshold = "BLOCK_ONLY_HIGH"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_DANGEROUS_CONTENT", 
                        threshold = "BLOCK_ONLY_HIGH"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_HARASSMENT",
                        threshold = "BLOCK_ONLY_HIGH"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_CIVIC_INTEGRITY",
                        threshold = "BLOCK_ONLY_HIGH"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _endpointUrl + _apiKey;
            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, err);
                
                return "😔 Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ bác sĩ Everwell để được hỗ trợ tức thì.";
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            
            // Kiểm tra safety và quality của response
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                
                // Kiểm tra safety ratings
                if (candidate.TryGetProperty("safetyRatings", out var safetyRatings))
                {
                    foreach (var rating in safetyRatings.EnumerateArray())
                    {
                        if (rating.TryGetProperty("probability", out var prob) && 
                            (prob.GetString() == "HIGH" || prob.GetString() == "MEDIUM"))
                        {
                            _logger.LogWarning("Potentially unsafe content detected in AI response");
                            return "⚠️ Tôi không thể trả lời câu hỏi này. Vui lòng tham khảo trực tiếp với bác sĩ chuyên khoa qua tính năng đặt lịch hẹn trong ứng dụng Everwell.";
                        }
                    }
                }
                
                var text = candidate.GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                var processedResponse = PostProcessResponse(text ?? "");
                
                // EMERGENCY OVERRIDE: Force redirect if response contains off-topic content
                if (ContainsOffTopicContent(processedResponse))
                {
                    _logger.LogWarning("AI generated off-topic content, forcing redirect. Original response: {Response}", processedResponse);
                    return GetOffTopicResponse();
                }
                
                // Final validation - ensure response is still health-focused
                if (!IsResponseHealthFocused(processedResponse))
                {
                    _logger.LogWarning("AI response went off-topic, using fallback");
                    return GetOffTopicResponse();
                }
                
                return processedResponse;
            }
            
            return "😔 Không thể tạo phản hồi phù hợp. Vui lòng thử lại hoặc liên hệ bác sĩ Everwell.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnhancedGeminiChatService.AskAsync");
            return "😔 Đã xảy ra lỗi. Vui lòng thử lại sau hoặc liên hệ hỗ trợ khách hàng Everwell.";
        }
    }

    private bool IsHealthRelatedQuestion(string prompt)
    {
        var lowerPrompt = prompt.ToLower();
        
        // STRICT BLACKLIST - Immediately reject these topics
        var strictBlacklist = new[]
        {
            // Geography and places
            "việt nam", "vietnam", "đất nước", "quốc gia", "địa hình", "núi", "sông", "biển", 
            "thành phố", "tỉnh", "hà nội", "hồ chí minh", "sài gòn", "đà nẵng", "huế",
            "mekong", "hồng", "trường sơn", "hoàng liên sơn", "phú quốc", "hạ long",
            
            // Culture and history
            "văn hóa", "lịch sử", "truyền thống", "dân tộc", "tôn giáo", "lễ hội",
            "du lịch", "điểm đến", "danh lam", "thắng cảnh", "di sản",
            
            // Sports and entertainment
            "bóng đá", "thể thao", "world cup", "sea games", "olympic",
            "game", "phim", "nhạc", "ca sĩ", "diễn viên", "nghệ sĩ",
            
            // Technology and programming
            "máy tính", "lập trình", "code", "website", "internet", "facebook",
            "google", "microsoft", "apple", "android", "ios",
            
            // Business and finance
            "kinh doanh", "tiền", "đầu tư", "ngân hàng", "chứng khoán",
            "kinh tế", "thương mại", "công ty", "doanh nghiệp",
            
            // Education
            "học", "trường", "đại học", "thi cử", "bài tập", "sinh viên", "giáo viên",
            
            // Politics and government
            "chính trị", "chính phủ", "đảng", "bầu cử", "luật pháp"
        };
        
        // Check for blacklisted terms first
        foreach (var blacklistedTerm in strictBlacklist)
        {
            if (lowerPrompt.Contains(blacklistedTerm))
            {
                return false; // Immediately reject
            }
        }
        
        // Health-related keywords (whitelist) - must contain at least one
        var healthKeywords = new[]
        {
            // Women's health core terms
            "chu kỳ", "kinh nguyệt", "rụng trứng", "thai", "sinh sản", "phụ khoa", "sản khoa",
            "âm đạo", "tử cung", "buồng trứng", "hormone", "estrogen", "progesterone",
            
            // STI and testing
            "sti", "hiv", "aids", "giang mai", "lậu", "chlamydia", "herpes", "hpv", 
            "xét nghiệm", "nhiễm trùng", "bệnh lây", "tình dục", "quan hệ",
            
            // Symptoms and health
            "đau bụng", "ra máu", "tiết dịch", "ngứa", "đau", "triệu chứng", "bệnh",
            "sức khỏe", "khám", "bác sĩ", "tư vấn", "điều trị", "thuốc", "y tế",
            
            // Contraception and family planning
            "tránh thai", "bao cao su", "thuốc tránh thai", "vòng tránh thai",
            "kế hoạch", "mang thai", "có thai", "thụ thai", "nội tiết",
            
            // PMS and related issues
            "pms", "đau đầu", "căng thẳng", "tâm trạng", "trầm cảm", "lo âu",
            "mệt mỏi", "buồn nôn", "chóng mặt",
            
            // Everwell services
            "everwell", "đặt lịch", "hẹn", "ứng dụng", "app", "theo dõi", "ghi nhận",
            
            // Emergency medical
            "cấp cứu", "khẩn cấp", "nguy hiểm", "115", "bệnh viện", "cứu thương"
        };
        
        // Must contain at least one health keyword
        var hasHealthKeywords = healthKeywords.Any(keyword => lowerPrompt.Contains(keyword));
        
        // Additional health context patterns
        var healthPatterns = new[]
        {
            "sức khoẻ phụ nữ", "chăm sóc sức khỏe", "tư vấn y tế", 
            "khám phụ khoa", "điều trị bệnh", "triệu chứng bất thường"
        };
        
        var hasHealthContext = healthPatterns.Any(pattern => lowerPrompt.Contains(pattern));
        
        return hasHealthKeywords || hasHealthContext;
    }

    private bool IsEmergencyQuestion(string prompt)
    {
        var emergencyKeywords = new[]
        {
            "đau dữ dội", "ra máu nhiều", "không thở được", "ngất xỉu", 
            "cấp cứu", "khẩn cấp", "nguy hiểm", "sốt cao", "chảy máu"
        };
        
        return emergencyKeywords.Any(keyword => 
            prompt.ToLower().Contains(keyword.ToLower()));
    }

    private bool IsResponseHealthFocused(string response)
    {
        var lowerResponse = response.ToLower();
        
        // Check for off-topic content in response
        var offTopicIndicators = new[]
        {
            // Geography and culture
            "việt nam", "đất nước", "địa hình", "núi", "sông", "biển", "thành phố",
            "văn hóa", "lịch sử", "truyền thống", "du lịch",
            
            // Sports and entertainment
            "bóng đá", "thể thao", "phim", "nhạc",
            
            // Technology and business
            "công nghệ", "kinh doanh", "đầu tư",
            
            // Education
            "trường học", "đại học", "giáo dục",
            
            // Specific geographic terms
            "đông nam á", "châu á", "mekong", "hồng hà", "sài gòn", "hà nội"
        };
        
        // If response contains any off-topic content, reject it
        if (offTopicIndicators.Any(indicator => lowerResponse.Contains(indicator)))
        {
            return false;
        }
        
        // Check if response contains health-related content
        var healthIndicators = new[]
        {
            "sức khỏe", "everwell", "bác sĩ", "chu kỳ", "kinh nguyệt", 
            "xét nghiệm", "phụ khoa", "tư vấn", "khám", "đặt lịch",
            "trợ lý ai chuyên", "chỉ có thể hỗ trợ", "sức khỏe phụ nữ"
        };
        
        return healthIndicators.Any(indicator => lowerResponse.Contains(indicator));
    }

    private string GetOffTopicResponse()
    {
        return @"😊 Tôi là trợ lý AI chuyên về sức khỏe phụ nữ của Everwell Healthcare. Tôi chỉ có thể hỗ trợ các câu hỏi về:

🌸 **Sức khỏe sinh sản và phụ khoa**
📅 **Theo dõi chu kỳ kinh nguyệt**  
🧪 **Xét nghiệm STI**
👩‍⚕️ **Đặt lịch hẹn với bác sĩ**
💊 **Tư vấn sức khỏe phụ nữ**
📱 **Hướng dẫn sử dụng ứng dụng Everwell**

Bạn có câu hỏi nào về sức khỏe mà tôi có thể giúp không? 💚

💡 *Ví dụ: ""Chu kỳ kinh nguyệt của tôi bất thường"", ""Tôi cần xét nghiệm STI"", ""Đặt lịch hẹn bác sĩ phụ khoa""*";
    }

    private string GetEmergencyResponse()
    {
        return @"🚨 **ĐÂY CÓ THỂ LÀ TÌNH HUỐNG CẤP CỨU!**

**Hành động ngay lập tức:**
• 📞 **Gọi 115** hoặc đến bệnh viện gần nhất NGAY
• ⏰ **Không được trì hoãn!**
• 👥 Thông báo cho người thân

**Số điện thoại cấp cứu:**
• **115** (Cấp cứu quốc gia)
• **Bệnh viện gần nhất**

⚠️ **Trong tình huống khẩn cấp, hãy tìm kiếm chăm sóc y tế ngay lập tức. Đây không phải lúc để chờ đợi hoặc tự chẩn đoán.**

💚 Everwell luôn sẵn sàng hỗ trợ sau khi bạn đã được chăm sóc y tế cấp cứu.";
    }

    private string PostProcessResponse(string response)
    {
        // Add consistent disclaimers if not present
        if (!response.Contains("tham khảo") && !response.Contains("bác sĩ") && 
            (response.Contains("triệu chứng") || response.Contains("đau") || response.Contains("bất thường")))
        {
            response += "\n\n💡 *Lưu ý: Thông tin này chỉ mang tính tham khảo. Hãy tư vấn bác sĩ để được chẩn đoán và điều trị chính xác.*";
        }

        // Add Everwell app promotion for relevant topics
        if ((response.Contains("chu kỳ") || response.Contains("kinh nguyệt")) && !response.Contains("Everwell"))
        {
            response += "\n\n📱 Sử dụng tính năng theo dõi chu kỳ trong ứng dụng Everwell để có thông tin chính xác hơn!";
        }

        if ((response.Contains("xét nghiệm") || response.Contains("STI")) && !response.Contains("đặt lịch"))
        {
            response += "\n\n🧪 Bạn có thể đặt lịch xét nghiệm STI ngay trong ứng dụng Everwell - thuận tiện và bảo mật!";
        }

        return response.Trim();
    }

    private bool ContainsOffTopicContent(string response)
    {
        var lowerResponse = response.ToLower();
        
        // Check for off-topic content in response
        var offTopicIndicators = new[]
        {
            // Geography and culture
            "việt nam", "đất nước", "địa hình", "núi", "sông", "biển", "thành phố",
            "văn hóa", "lịch sử", "truyền thống", "du lịch",
            
            // Sports and entertainment
            "bóng đá", "thể thao", "phim", "nhạc",
            
            // Technology and business
            "công nghệ", "kinh doanh", "đầu tư",
            
            // Education
            "trường học", "đại học", "giáo dục",
            
            // Specific geographic terms
            "đông nam á", "châu á", "mekong", "hồng hà", "sài gòn", "hà nội"
        };
        
        // If response contains any off-topic content, reject it
        return offTopicIndicators.Any(indicator => lowerResponse.Contains(indicator));
    }
} 