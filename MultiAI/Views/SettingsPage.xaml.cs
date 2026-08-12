#nullable enable
using Microsoft.UI.Xaml.Controls;
using MultiAI.ViewModels;

namespace MultiAI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedIndex >= 0)
            {
                ViewModel.ChangeTheme(ThemeComboBox.SelectedIndex);
            }
        }

        private void SaveOpenAI_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveOpenAIKey(OpenAIApiKeyBox.Password);
        }

        private async void ValidateOpenAI_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("OpenAI");
        }

        private void SaveAnthropic_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveAnthropicKey(AnthropicApiKeyBox.Password);
        }

        private async void ValidateAnthropic_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("Anthropic");
        }

        private void SaveGemini_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveGeminiKey(GeminiApiKeyBox.Password);
        }

        private async void ValidateGemini_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("Google Gemini");
        }

        private void SaveGroq_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveGroqKey(GroqApiKeyBox.Password);
        }

        private async void ValidateGroq_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("Groq");
        }

        private void SaveDeepSeek_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveDeepSeekKey(DeepSeekApiKeyBox.Password);
        }

        private async void ValidateDeepSeek_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("DeepSeek");
        }

        private void SaveMistral_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveMistralKey(MistralApiKeyBox.Password);
        }

        private async void ValidateMistral_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("Mistral AI");
        }

        private void SaveOpenRouter_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveOpenRouterKey(OpenRouterApiKeyBox.Password);
        }

        private async void ValidateOpenRouter_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("OpenRouter");
        }

        private void SaveXAI_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveXAIKey(XAIApiKeyBox.Password);
        }

        private async void ValidateXAI_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("xAI (Grok)");
        }

        private void SaveOllama_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SaveOllamaHost(OllamaHostBox.Text);
        }

        private async void ValidateOllama_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.ValidateKeyAsync("Ollama (Local)");
        }
    }
}
