#nullable enable
using System;
using System.Collections.Generic;

namespace MultiAI.Providers
{
    public static class ProviderFactory
    {
        public static readonly List<string> SupportedProviders = new List<string>
        {
            "OpenAI",
            "Anthropic",
            "Google Gemini"
        };

        public static ILLMProvider GetProvider(string providerName)
        {
            return providerName switch
            {
                "OpenAI" => new OpenAIProvider(),
                "Anthropic" => new AnthropicProvider(),
                "Google Gemini" or "Gemini" => new GeminiProvider(),
                _ => throw new ArgumentException($"Unsupported provider: '{providerName}'", nameof(providerName))
            };
        }
    }
}
