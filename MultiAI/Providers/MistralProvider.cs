#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.Chat;
using MultiAI.Models;

namespace MultiAI.Providers
{
    public class MistralProvider : ILLMProvider
    {
        private ChatClient? _client;
        private string _model = "mistral-large-latest";
        private string _apiKey = string.Empty;
        private const string EndpointUri = "https://api.mistral.ai/v1";

        public string Name => "Mistral AI";

        public void Initialize(string apiKey, string model = "mistral-large-latest")
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "mistral-large-latest" : model;
            if (!string.IsNullOrEmpty(apiKey))
            {
                var options = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(EndpointUri) };
                _client = new ChatClient(_model, new System.ClientModel.ApiKeyCredential(_apiKey), options);
            }
        }

        public Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            var defaultModels = new List<string>
            {
                "mistral-large-latest",
                "pixtral-large-latest",
                "codestral-latest",
                "mistral-small-latest",
                "open-mistral-nemo"
            };
            return Task.FromResult(defaultModels);
        }

        public async Task<string> SendMessageAsync(string message, List<Message> history)
        {
            if (_client == null) return "Error: Mistral AI Provider not initialized with API Key.";

            var chatMessages = BuildChatMessages(message, history);

            try
            {
                ChatCompletion completion = await _client.CompleteChatAsync(chatMessages);
                if (completion.Content != null && completion.Content.Count > 0)
                {
                    return completion.Content[0].Text;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> StreamMessageAsync(
            string message,
            List<Message> history,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_client == null)
            {
                yield return "Error: Mistral AI Provider not initialized with API Key.";
                yield break;
            }

            var chatMessages = BuildChatMessages(message, history);
            string? initError = null;
            IAsyncEnumerable<StreamingChatCompletionUpdate>? streamingUpdates = null;

            try
            {
                streamingUpdates = _client.CompleteChatStreamingAsync(chatMessages, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                initError = $"Error initializing stream: {ex.Message}";
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
