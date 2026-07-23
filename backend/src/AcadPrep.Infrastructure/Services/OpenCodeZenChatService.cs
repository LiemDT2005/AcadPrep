using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.AiQna.Commands.AskAi;
using Application.Common.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class OpenCodeZenChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly AiChatSettings _settings;
    private readonly ILogger<OpenCodeZenChatService> _logger;

    public OpenCodeZenChatService(
        HttpClient httpClient,
        IOptions<AiChatSettings> options,
        ILogger<OpenCodeZenChatService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            var baseUrl = _settings.BaseUrl;
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
        
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }
    }

    public async Task<Result<AiChatResponse>> AskAsync(
        string systemPrompt, 
        IReadOnlyList<ChatMessageDto> history, 
        string newMessage, 
        CancellationToken cancellationToken = default)
    {
        if (_settings.Models == null || !_settings.Models.Any())
        {
            return Result<AiChatResponse>.Failure("Không có cấu hình AI model nào khả dụng.");
        }

        foreach (var modelOption in _settings.Models)
        {
            try
            {
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                if (history != null)
                {
                    foreach (var msg in history)
                    {
                        messages.Add(new { role = msg.Role, content = msg.Content });
                    }
                }

                messages.Add(new { role = "user", content = newMessage });

                var requestBody = new
                {
                    model = modelOption.Id,
                    messages = messages
                };

                using var modelCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                modelCts.CancelAfter(TimeSpan.FromSeconds(modelOption.TimeoutSeconds));

                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, modelCts.Token);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseContent);

                var root = doc.RootElement;
                var text = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                int tokens = root.GetProperty("usage").GetProperty("total_tokens").GetInt32();

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning($"AI returned empty response for model {modelOption.Id}");
                    continue; // Thử model tiếp theo
                }

                _logger.LogInformation("Successfully received AI response from model {ModelId}. Tokens used: {Tokens}", modelOption.Id, tokens);
                
                return Result<AiChatResponse>.Success(new AiChatResponse(text, tokens, modelOption.Id));
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogWarning(ex, $"Failed to get response from AI model {modelOption.Id}. Trying next one...");
                continue; // Thử model tiếp theo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error communicating with AI model {modelOption.Id}.");
                break; // Dừng lại vì lỗi ngoài ý muốn
            }
        }

        return Result<AiChatResponse>.Failure("AI đang bận, thử lại sau.");
    }
}
