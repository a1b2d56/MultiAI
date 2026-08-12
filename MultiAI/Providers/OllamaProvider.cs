#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.Chat;
using MultiAI.Models;

namespace MultiAI.Providers
{
    public class OllamaProvider : ILLMProvider
    {
        private ChatClient? _client;
        private string _model = "llama3.2";
        private string _hostUri = "http://localhost:11434/v1";

        public string Name => "Ollama (Local)";

        public void Initialize(string apiKey, string model = "llama3.2")
        {
            _model = string.IsNullOrWhiteSpace(model) ? "llama3.2" : model;
            string keyToUse = string.IsNullOrWhiteSpace(apiKey) ? "ollama" : apiKey;
            
            if (apiKey.StartsWith("http://") || apiKey.StartsWith("https://"))
            {
                _hostUri = apiKey.TrimEnd('/') + "/v1";
                keyToUse = "ollama";
            }
            else
            {
                _hostUri = "http://localhost:11434/v1";
            }

            var options = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(_hostUri) };
            _client = new ChatClient(_model, new System.ClientModel.ApiKeyCredential(keyToUse), options);
        }

        public async Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            var models = new List<string>();
            string baseHost = "http://localhost:11434";
            if (!string.IsNullOrWhiteSpace(apiKey) && (apiKey.StartsWith("http://") || apiKey.StartsWith("https://")))
            {
                baseHost = apiKey.TrimEnd('/');
            }

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var response = await http.GetAsync($"{baseHost}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("models", out var modelsArr))
                    {
                        foreach (var el in modelsArr.EnumerateArray())
                        {
                            if (el.TryGetProperty("name", out var nameProp))
                            {
                                string name = nameProp.GetString() ?? "";
                                if (!string.IsNullOrEmpty(name))
                                {
                                    models.Add(name);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Local Ollama server offline or unreachable, fall back to defaults
            }

            if (models.Count == 0)
            {
                models.AddRange(new[]
                {
                    "llama3.2",
                    "deepseek-r1",
                    "mistral",
                    "gemma2",
                    "phi4"
                });
            }

            return models;
        }

        public async Task<string> SendMessageAsync(string message, List<Message> history)
        {
            if (_client == null) Initialize("ollama", _model);

            var chatMessages = BuildChatMessages(message, history);

            try
            {
                ChatCompletion completion = await _client!.CompleteChatAsync(chatMessages);
                if (completion.Content != null && completion.Content.Count > 0)
                {
                    return completion.Content[0].Text;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"Error connecting to Ollama: {ex.Message}. Make sure Ollama is running at http://localhost:11434";
            }
        }

        public async IAsyncEnumerable<string> StreamMessageAsync(
            string message,
            List<Message> history,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_client == null) Initialize("ollama", _model);

            var chatMessages = BuildChatMessages(message, history);
            string? initError = null;
            IAsyncEnumerable<StreamingChatCompletionUpdate>? streamingUpdates = null;

            try
            {
                streamingUpdates = _client!.CompleteChatStreamingAsync(chatMessages, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                initError = $"Error connecting to Ollama: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(initError))
            {
                yield return initError;
                yield break;
            }

            if (streamingUpdates != null)
            {
                await foreach (var update in streamingUpdates)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    foreach (var contentPart in update.ContentUpdate)
                    {
                        if (!string.IsNullOrEmpty(contentPart.Text))
                        {
                            yield return contentPart.Text;
                        }
                    }
                }
            }
        }

        private List<ChatMessage> BuildChatMessages(string currentMessage, List<Message> history)
        {
            var chatMessages = new List<ChatMessage>();
            foreach (var msg in history)
            {
                if (msg.Role == "You" || msg.Role == "user")
                    chatMessages.Add(new UserChatMessage(msg.Content));
                else
                    chatMessages.Add(new AssistantChatMessage(msg.Content));
            }
            chatMessages.Add(new UserChatMessage(currentMessage));
            return chatMessages;
        }
    }
}
