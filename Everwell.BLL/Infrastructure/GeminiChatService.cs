using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Everwell.BLL.Services.Interfaces;

namespace Everwell.BLL.Infrastructure;

public class GeminiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpointUrl;
    private readonly ILogger<GeminiChatService> _logger;
    private readonly IConfiguration _configuration;

    public GeminiChatService(HttpClient httpClient, IConfiguration config, ILogger<GeminiChatService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey chưa được cấu hình");
        var model = config["Gemini:Model"] ?? "gemini-1.5-flash";
        _endpointUrl = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key=";
        _logger = logger;
        _configuration = config;
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var systemPrompt = _configuration?["Gemini:SystemPrompt"] ?? "Hãy trả lời bằng tiếng Việt.";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = systemPrompt + "\n\n" + prompt } }
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
            throw new InvalidOperationException($"Gemini API error: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = doc.RootElement;
        // candidates[0].content.parts[0].text
        var text = root.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        return text ?? string.Empty;
    }
} 