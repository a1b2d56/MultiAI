#nullable enable
using OpenAI.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAI.Providers
{
    public class OpenAIProvider
    {
        private ChatClient? _client;

        public void Initialize(string apiKey, string model = "gpt-4o-mini")
        {
            _client = new ChatClient(model, apiKey);
        }

        public async Task<string> SendMessageAsync(string message, List<Models.Message> history)
        {
            if (_client == null) return "Error: Provider not initialized with API Key.";

            var chatMessages = new List<ChatMessage>();
            
            foreach(var msg in history)
            {
                if (msg.Role == "user")
                    chatMessages.Add(new UserChatMessage(msg.Content));
                else
                    chatMessages.Add(new AssistantChatMessage(msg.Content));
            }
            
            chatMessages.Add(new UserChatMessage(message));

            try
            {
                ChatCompletion completion = await _client.CompleteChatAsync(chatMessages);
                return completion.Content[0].Text;
            }
            catch (System.Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
