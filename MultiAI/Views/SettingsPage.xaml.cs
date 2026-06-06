using Microsoft.UI.Xaml.Controls;
using MultiAI.Services;

namespace MultiAI.Views
{
    public sealed partial class SettingsPage : Page
    {
        private SecureStorageService _secureStorage;

        public SettingsPage()
        {
            this.InitializeComponent();
            _secureStorage = new SecureStorageService();
            
            var key = _secureStorage.GetKey("OpenAI");
            if (key != null) OpenAIApiKeyBox.Password = key;
            
            var geminiKey = _secureStorage.GetKey("Gemini");
            if (geminiKey != null) GeminiApiKeyBox.Password = geminiKey;

            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue("AppTheme", out object? themeObj) && themeObj is string themeStr)
            {
                if (themeStr == "Light") ThemeComboBox.SelectedIndex = 1;
                else if (themeStr == "Dark") ThemeComboBox.SelectedIndex = 2;
                else ThemeComboBox.SelectedIndex = 0;
            }
            else
            {
                ThemeComboBox.SelectedIndex = 0;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppTheme"] = tag;
                if (System.Enum.TryParse(tag, out Microsoft.UI.Xaml.ElementTheme theme))
                {
                    (App.Current as App)?.MainWindowRef?.SetTheme(theme);
                }
            }
        }

        private async void SaveOpenAI_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _secureStorage.SaveKey("OpenAI", OpenAIApiKeyBox.Password);
            SaveSuccessInfoBar.IsOpen = true;
            await System.Threading.Tasks.Task.Delay(3000);
            SaveSuccessInfoBar.IsOpen = false;
        }

        private async void SaveGemini_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _secureStorage.SaveKey("Gemini", GeminiApiKeyBox.Password);
            SaveSuccessInfoBar.Title = "Saved";
            SaveSuccessInfoBar.Message = "Your API key has been securely saved to the Windows Credential Manager.";
            SaveSuccessInfoBar.Severity = InfoBarSeverity.Success;
            SaveSuccessInfoBar.IsOpen = true;
            await System.Threading.Tasks.Task.Delay(3000);
            SaveSuccessInfoBar.IsOpen = false;
        }

        private async void ValidateGemini_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var provider = new Providers.GeminiProvider();
            var models = await provider.GetAvailableModelsAsync(GeminiApiKeyBox.Password);
            
            if (models.Count > 2) 
            {
                SaveSuccessInfoBar.Title = "Valid Key!";
                SaveSuccessInfoBar.Message = $"Successfully connected to Google API. Found {models.Count} models.";
                SaveSuccessInfoBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                SaveSuccessInfoBar.Title = "Validation Failed";
                SaveSuccessInfoBar.Message = "Could not reach API. Key might be invalid or rate limited.";
                SaveSuccessInfoBar.Severity = InfoBarSeverity.Error;
            }
            SaveSuccessInfoBar.IsOpen = true;
            await System.Threading.Tasks.Task.Delay(4000);
            SaveSuccessInfoBar.IsOpen = false;
        }
    }
}
