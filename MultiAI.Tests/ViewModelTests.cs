using MultiAI.ViewModels;
using System.Threading.Tasks;
using Xunit;

namespace MultiAI.Tests
{
    public class ViewModelTests
    {
        [Fact]
        public void ChatViewModel_DefaultState_ShouldBeValid()
        {
            var vm = new ChatViewModel();
            Assert.NotNull(vm.Messages);
            Assert.Empty(vm.Messages);
            Assert.Equal("Google Gemini", vm.SelectedProvider);
            Assert.False(vm.IsGenerating);
        }

        [Fact]
        public void SettingsViewModel_DefaultState_ShouldLoadProviders()
        {
            var vm = new SettingsViewModel();
            Assert.NotNull(vm.OpenAIApiKey);
            Assert.NotNull(vm.AnthropicApiKey);
            Assert.NotNull(vm.GeminiApiKey);
            Assert.Equal(0, vm.SelectedThemeIndex);
        }

        [Fact]
        public async Task SettingsViewModel_ValidateKeyWithEmptyKey_ShouldShowMessage()
        {
            var vm = new SettingsViewModel();
            await vm.ValidateKeyAsync("OpenAI");
            Assert.True(vm.IsSaveSuccessOpen);
            Assert.Contains("valid key", vm.StatusMessage);
        }
    }
}
