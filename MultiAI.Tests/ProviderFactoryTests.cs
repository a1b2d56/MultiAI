using MultiAI.Providers;
using System;
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
        public void GetProvider_ShouldReturnCorrectInstance(string name, Type expectedType)
        {
            var provider = ProviderFactory.GetProvider(name);
            Assert.NotNull(provider);
            Assert.IsType(expectedType, provider);
        }

        [Fact]
        public void GetProvider_WithInvalidName_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ProviderFactory.GetProvider("InvalidProviderName"));
        }
    }
}
