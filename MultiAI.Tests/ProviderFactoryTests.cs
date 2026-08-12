using MultiAI.Providers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MultiAI.Tests
{
    public class ProviderFactoryTests
    {
        [Theory]
        [InlineData("OpenAI", typeof(OpenAIProvider))]
        [InlineData("Anthropic", typeof(AnthropicProvider))]
        [InlineData("Google Gemini", typeof(GeminiProvider))]
        [InlineData("Gemini", typeof(GeminiProvider))]
        [InlineData("Groq", typeof(GroqProvider))]
        [InlineData("DeepSeek", typeof(DeepSeekProvider))]
        [InlineData("Mistral AI", typeof(MistralProvider))]
        [InlineData("OpenRouter", typeof(OpenRouterProvider))]
        [InlineData("xAI (Grok)", typeof(XAIProvider))]
        [InlineData("Ollama (Local)", typeof(OllamaProvider))]
        public void GetProvider_ShouldReturnCorrectInstance(string name, Type expectedType)
        {
            var provider = ProviderFactory.GetProvider(name);
            Assert.NotNull(provider);
            Assert.IsType(expectedType, provider);
        }

        [Theory]
        [InlineData("Groq")]
        [InlineData("DeepSeek")]
        [InlineData("Mistral AI")]
        [InlineData("OpenRouter")]
        [InlineData("xAI (Grok)")]
        [InlineData("Ollama (Local)")]
        public async Task NewProviders_ShouldReturnAvailableModels(string name)
        {
            var provider = ProviderFactory.GetProvider(name);
            var models = await provider.GetAvailableModelsAsync(string.Empty);
            Assert.NotNull(models);
            Assert.NotEmpty(models);
        }

        [Fact]
        public void GetProvider_WithInvalidName_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ProviderFactory.GetProvider("InvalidProviderName"));
        }
    }
}
