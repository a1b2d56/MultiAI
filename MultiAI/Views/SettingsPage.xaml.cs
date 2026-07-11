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
    }
}
