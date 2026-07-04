#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MultiAI.Models;

namespace MultiAI.Providers
{
    public class GeminiProvider : ILLMProvider
    {
        private string _apiKey = string.Empty;
        private string _model = "gemini-1.5-flash";
        private static readonly HttpClient _httpClient = new HttpClient();

        public string Name => "Google Gemini";

        public void Initialize(string apiKey, string model = "gemini-1.5-flash")
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gemini-1.5-flash" : model;
        }

        public async Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return FallbackModels();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var models = new List<string>();

                if (doc.RootElement.TryGetProperty("models", out var modelsArray))
                {
                    foreach (var modelItem in modelsArray.EnumerateArray())
                    {
                        string name = modelItem.GetProperty("name").GetString() ?? "";
                        if (name.StartsWith("models/")) name = name.Substring(7);
                        if (name.Contains("gemini")) models.Add(name);
                    }
                }
                return models.Count > 0 ? models : FallbackModels();
            }
            catch
            {
                return FallbackModels();
            }
        }

        private static List<string> FallbackModels()
        {
            return new List<string> { "gemini-2.0-flash", "gemini-1.5-flash", "gemini-1.5-pro" };
        }

        public async Task<string> SendMessageAsync(string message, List<Message> history)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Error: Gemini Provider not initialized with API Key.";

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var requestContent = PrepareRequestBody(message, history);

            try
            {
                var response = await _httpClient.PostAsync(url, requestContent);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: API returned {response.StatusCode}. Details: {responseJson}";
                }

                return ExtractTextFromResponse(responseJson);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async async IAsyncEnumerable<string> StreamMessageAsync(
            string message, 
            List<Message> history, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                yield return "Error: Gemini Provider not initialized with API Key.";
                yield break;
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:streamGenerateContent?alt=sse&key={_apiKey}";
            var requestContent = PrepareRequestBody(message, history);

            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = requestContent };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errJson = await response.Content.ReadAsStringAsync(cancellationToken);
                yield return $"Error: Gemini API returned {response.StatusCode}. Details: {errJson}";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]") break;

                    string? token = ExtractTextFromResponse(data);
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
        }

        private StringContent PrepareRequestBody(string message, List<Message> history)
        {
            var contents = new List<object>();
            foreach (var msg in history)
            {
                contents.Add(new
                {
                    role = (msg.Role == "You" || msg.Role == "user") ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = message } }
            });

            var requestBody = new { contents = contents };
            var json = JsonSerializer.Serialize(requestBody);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private string ExtractTextFromResponse(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];
                    if (candidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? string.Empty;
                    }
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
