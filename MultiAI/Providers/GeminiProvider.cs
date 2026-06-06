#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiAI.Providers
{
    public class GeminiProvider
    {
        private string _apiKey = string.Empty;
        private string _model = string.Empty;
        private static readonly HttpClient _httpClient = new HttpClient();

        public void Initialize(string apiKey, string model = "gemini-1.5-flash")
        {
            _apiKey = apiKey;
            _model = model;
        }

        public async Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                // If it fails (e.g. invalid key or no internet), just fall back to the defaults so we don't break the UI
                if (!response.IsSuccessStatusCode) return new List<string> { "gemini-1.5-flash", "gemini-1.5-pro" };

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var models = new List<string>();
                foreach (var model in doc.RootElement.GetProperty("models").EnumerateArray())
                {
                    string name = model.GetProperty("name").GetString() ?? "";
                    
                    // Google prefixes everything with "models/", let's chop that off for a cleaner UI
                    if (name.StartsWith("models/")) name = name.Substring(7);
                    
                    // We only care about gemini models, ignore the old legacy ones
                    if (name.Contains("gemini")) models.Add(name);
                }
                return models.Count > 0 ? models : new List<string> { "gemini-1.5-flash", "gemini-1.5-pro" };
            }
            catch
            {
                // Better safe than sorry!
                return new List<string> { "gemini-1.5-flash", "gemini-1.5-pro" };
            }
        }

        public async Task<string> SendMessageAsync(string message, List<Models.Message> history)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Error: Gemini Provider not initialized with API Key.";

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Map our internal Message format into Google's expected REST JSON format
            var contents = new List<object>();
            foreach(var msg in history)
            {
                contents.Add(new {
                    role = msg.Role == "You" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }
            contents.Add(new {
                role = "user",
                parts = new[] { new { text = message } }
            });

            var requestBody = new { contents = contents };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: API returned {response.StatusCode}. Details: {responseJson}";
                }

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var text = root.GetProperty("candidates")[0]
                               .GetProperty("content")
                               .GetProperty("parts")[0]
                               .GetProperty("text").GetString();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
