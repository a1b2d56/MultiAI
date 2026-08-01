#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MultiAI.Services;
using System;
using System.Threading.Tasks;

namespace MultiAI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SecureStorageService _storageService;

        [ObservableProperty]
        private string _openAIApiKey = string.Empty;

        [ObservableProperty]
        private string _anthropicApiKey = string.Empty;

        [ObservableProperty]
        private string _geminiApiKey = string.Empty;

        [ObservableProperty]
        private bool _isSaveSuccessOpen;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _hasOpenAIKey;

        [ObservableProperty]
        private bool _hasAnthropicKey;

        [ObservableProperty]
        private bool _hasGeminiKey;

        [ObservableProperty]
        private int _selectedThemeIndex;

        public SettingsViewModel()
        {
            _storageService = new SecureStorageService();
            LoadKeys();
            LoadThemeSetting();
        }

        public void LoadKeys()
        {
            OpenAIApiKey = _storageService.GetKey("OpenAI") ?? string.Empty;
            AnthropicApiKey = _storageService.GetKey("Anthropic") ?? string.Empty;
            GeminiApiKey = _storageService.GetKey("Google Gemini") ?? string.Empty;

            HasOpenAIKey = !string.IsNullOrEmpty(OpenAIApiKey);
            HasAnthropicKey = !string.IsNullOrEmpty(AnthropicApiKey);
            HasGeminiKey = !string.IsNullOrEmpty(GeminiApiKey);
        }

        private void LoadThemeSetting()
        {
            try
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue("AppTheme", out object? val) && val is string themeStr)
                {
                    SelectedThemeIndex = themeStr switch
                    {
                        "Light" => 1,
                        "Dark" => 2,
                        _ => 0
                    };
                    return;
                }
            }
            catch
            {
                // Fallback for non-packaged unit test environment
            }
            SelectedThemeIndex = 0;
        }

        [RelayCommand]
        public void SaveOpenAIKey(string key)
        {
            _storageService.SaveKey("OpenAI", key);
            OpenAIApiKey = key;
            HasOpenAIKey = !string.IsNullOrEmpty(key);
            ShowSuccess("OpenAI API Key saved securely.");
        }

        [RelayCommand]
        public void SaveAnthropicKey(string key)
        {
            _storageService.SaveKey("Anthropic", key);
            AnthropicApiKey = key;
            HasAnthropicKey = !string.IsNullOrEmpty(key);
            ShowSuccess("Anthropic API Key saved securely.");
        }

        [RelayCommand]
        public void SaveGeminiKey(string key)
        {
            _storageService.SaveKey("Google Gemini", key);
            GeminiApiKey = key;
            HasGeminiKey = !string.IsNullOrEmpty(key);
            ShowSuccess("Google Gemini API Key saved securely.");
        }

        [RelayCommand]
        public async Task ValidateKeyAsync(string provider)
        {
            string key = provider switch
            {
                "OpenAI" => OpenAIApiKey,
                "Anthropic" => AnthropicApiKey,
                "Google Gemini" or "Gemini" => GeminiApiKey,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(key))
            {
                ShowSuccess($"Please enter a valid key for {provider} first.");
                return;
            }

            try
            {
                var providerObj = Providers.ProviderFactory.GetProvider(provider);
                var models = await providerObj.GetAvailableModelsAsync(key);
                if (models != null && models.Count > 0)
                {
                    ShowSuccess($"{provider} API Key validated successfully!");
                }
                else
                {
                    ShowSuccess($"Could not validate {provider} API key.");
                }
            }
            catch (Exception ex)
            {
                ShowSuccess($"Validation error: {ex.Message}");
            }
        }

        public void ChangeTheme(int index)
        {
            SelectedThemeIndex = index;
            ElementTheme theme = index switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            try
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppTheme"] = theme.ToString();
            }
            catch { }

            (App.Current as App)?.MainWindowRef?.SetTheme(theme);
        }

        private void ShowSuccess(string message)
        {
            StatusMessage = message;
            IsSaveSuccessOpen = true;
        }
    }
}
