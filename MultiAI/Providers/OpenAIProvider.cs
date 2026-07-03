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
    public class OpenAIProvider : ILLMProvider
    {
        private ChatClient? _client;
        private string _model = "gpt-4o-mini";
        private string _apiKey = string.Empty;

        public string Name => "OpenAI";

        public void Initialize(string apiKey, string model = "gpt-4o-mini")
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;
            if (!string.IsNullOrEmpty(apiKey))
            {
                _client = new ChatClient(_model, _apiKey);
            }
        }

        public Task<List<string>> GetAvailableModelsAsync(string apiKey)
        {
            var defaultModels = new List<string>
            {
                "gpt-4o",
                "gpt-4o-mini",
                "o1-preview",
                "o1-mini",
                "gpt-4-turbo"
            };
            return Task.FromResult(defaultModels);
        }

        public async Task<string> SendMessageAsync(string message, List<Message> history)
        {
            if (_client == null) return "Error: OpenAI Provider not initialized with API Key.";

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

        public async async IAsyncEnumerable<string> StreamMessageAsync(
            string message, 
            List<Message> history, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_client == null)
            {
                yield return "Error: OpenAI Provider not initialized with API Key.";
                yield break;
            }

            var chatMessages = BuildChatMessages(message, history);

            AsyncCollectionResult<StreamingChatCompletionUpdate> streamingUpdates;
            try
            {
                streamingUpdates = _client.CompleteChatStreamingAsync(chatMessages, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                yield return $"Error initializing stream: {ex.Message}";
                yield break;
            }

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
