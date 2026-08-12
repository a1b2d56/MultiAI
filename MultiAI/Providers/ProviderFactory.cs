#nullable enable
using System;
using System.Collections.Generic;

namespace MultiAI.Providers
{
    public static class ProviderFactory
    {
        public static readonly List<string> SupportedProviders = new List<string>
        {
            "Google Gemini",
            "OpenAI",
            "Anthropic",
            "Groq",
            "DeepSeek",
            "Mistral AI",
            "OpenRouter",
            "xAI (Grok)",
            "Ollama (Local)"
        };

        public static ILLMProvider GetProvider(string providerName)
        {
            return providerName switch
            {
                "OpenAI" => new OpenAIProvider(),
                "Anthropic" => new AnthropicProvider(),
                "Google Gemini" or "Gemini" => new GeminiProvider(),
                "Groq" => new GroqProvider(),
                "DeepSeek" => new DeepSeekProvider(),
                "Mistral AI" or "Mistral" => new MistralProvider(),
                "OpenRouter" => new OpenRouterProvider(),
                "xAI (Grok)" or "xAI" or "Grok" => new XAIProvider(),
                "Ollama (Local)" or "Ollama" => new OllamaProvider(),
                _ => throw new ArgumentException($"Unsupported provider: '{providerName}'", nameof(providerName))
            };
        }
    }
}
