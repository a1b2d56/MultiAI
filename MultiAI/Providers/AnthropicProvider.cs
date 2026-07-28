#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MultiAI.Models;

    /// <summary>
    /// LLM Provider implementation for Anthropic Claude models via REST API with SSE streaming support.
    /// </summary>
    public class AnthropicProvider : ILLMProvider
    {
        private string _apiKey = string.Empty;
        private string _model = "claude-3-5-sonnet-latest";
        private static readonly HttpClient _httpClient = new HttpClient();

        public string Name => "Anthropic";

        public void Initialize(string apiKey, string model = "claude-3-5-sonnet-latest")
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "claude-3-5-sonnet-latest" : model;
        }

        public Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            var defaultModels = new List<string>
            {
                "claude-3-7-sonnet-latest",
                "claude-3-5-sonnet-latest",
                "claude-3-5-haiku-latest",
                "claude-3-opus-latest"
            };
            return Task.FromResult(defaultModels);
        }

        public async Task<string> SendMessageAsync(string message, List<Message> history)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "Error: Anthropic Provider not initialized with API Key.";

            const string url = "https://api.anthropic.com/v1/messages";

            var messagesPayload = BuildMessagesPayload(message, history);

            var requestBody = new
            {
                model = _model,
                max_tokens = 4096,
                messages = messagesPayload
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: Anthropic API returned {response.StatusCode}. Details: {responseJson}";
                }

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
                {
                    foreach (var item in contentArray.EnumerateArray())
                    {
                        if (item.TryGetProperty("text", out var textEl))
                        {
                            return textEl.GetString() ?? string.Empty;
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> StreamMessageAsync(string message, List<Message> history, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                yield return "Error: Anthropic Provider not initialized with API Key.";
                yield break;
            }

            const string url = "https://api.anthropic.com/v1/messages";
            var messagesPayload = BuildMessagesPayload(message, history);

            var requestBody = new
            {
                model = _model,
                max_tokens = 4096,
                stream = true,
                messages = messagesPayload
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync(cancellationToken);
                yield return $"Error: API returned {response.StatusCode}. Details: {errContent}";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var dataStr = line.Substring(6).Trim();
                    if (dataStr == "[DONE]") break;

                    string? token = ExtractTokenFromSseEvent(dataStr);
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
        }

        private List<object> BuildMessagesPayload(string currentMessage, List<Message> history)
        {
            var list = new List<object>();
            foreach (var msg in history)
            {
                string role = (msg.Role == "You" || msg.Role == "user") ? "user" : "assistant";
                list.Add(new { role, content = msg.Content });
            }
            list.Add(new { role = "user", content = currentMessage });
            return list;
        }

        private string? ExtractTokenFromSseEvent(string eventJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(eventJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "content_block_delta")
                {
                    if (root.TryGetProperty("delta", out var deltaEl) && deltaEl.TryGetProperty("text", out var textEl))
                    {
                        return textEl.GetString();
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
