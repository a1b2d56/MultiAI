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
        private string _groqApiKey = string.Empty;

        [ObservableProperty]
        private string _deepSeekApiKey = string.Empty;

        [ObservableProperty]
        private string _mistralApiKey = string.Empty;

        [ObservableProperty]
        private string _openRouterApiKey = string.Empty;

        [ObservableProperty]
        private string _xAIApiKey = string.Empty;

        [ObservableProperty]
        private string _ollamaHost = "http://localhost:11434";

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
        private bool _hasGroqKey;

        [ObservableProperty]
        private bool _hasDeepSeekKey;

        [ObservableProperty]
        private bool _hasMistralKey;

        [ObservableProperty]
        private bool _hasOpenRouterKey;

        [ObservableProperty]
        private bool _hasXAIKey;

        [ObservableProperty]
        private bool _hasOllamaKey;

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
            GroqApiKey = _storageService.GetKey("Groq") ?? string.Empty;
            DeepSeekApiKey = _storageService.GetKey("DeepSeek") ?? string.Empty;
            MistralApiKey = _storageService.GetKey("Mistral AI") ?? string.Empty;
            OpenRouterApiKey = _storageService.GetKey("OpenRouter") ?? string.Empty;
            XAIApiKey = _storageService.GetKey("xAI (Grok)") ?? string.Empty;
            OllamaHost = _storageService.GetKey("Ollama (Local)") ?? "http://localhost:11434";

            HasOpenAIKey = !string.IsNullOrEmpty(OpenAIApiKey);
            HasAnthropicKey = !string.IsNullOrEmpty(AnthropicApiKey);
            HasGeminiKey = !string.IsNullOrEmpty(GeminiApiKey);
            HasGroqKey = !string.IsNullOrEmpty(GroqApiKey);
            HasDeepSeekKey = !string.IsNullOrEmpty(DeepSeekApiKey);
            HasMistralKey = !string.IsNullOrEmpty(MistralApiKey);
            HasOpenRouterKey = !string.IsNullOrEmpty(OpenRouterApiKey);
            HasXAIKey = !string.IsNullOrEmpty(XAIApiKey);
            HasOllamaKey = !string.IsNullOrEmpty(OllamaHost);
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
        public void SaveGroqKey(string key)
        {
            _storageService.SaveKey("Groq", key);
            GroqApiKey = key;
            HasGroqKey = !string.IsNullOrEmpty(key);
            ShowSuccess("Groq API Key saved securely.");
        }

        [RelayCommand]
        public void SaveDeepSeekKey(string key)
        {
            _storageService.SaveKey("DeepSeek", key);
            DeepSeekApiKey = key;
            HasDeepSeekKey = !string.IsNullOrEmpty(key);
            ShowSuccess("DeepSeek API Key saved securely.");
        }

        [RelayCommand]
        public void SaveMistralKey(string key)
        {
            _storageService.SaveKey("Mistral AI", key);
            MistralApiKey = key;
            HasMistralKey = !string.IsNullOrEmpty(key);
            ShowSuccess("Mistral AI API Key saved securely.");
        }

        [RelayCommand]
        public void SaveOpenRouterKey(string key)
        {
            _storageService.SaveKey("OpenRouter", key);
            OpenRouterApiKey = key;
            HasOpenRouterKey = !string.IsNullOrEmpty(key);
            ShowSuccess("OpenRouter API Key saved securely.");
        }

        [RelayCommand]
        public void SaveXAIKey(string key)
        {
            _storageService.SaveKey("xAI (Grok)", key);
            XAIApiKey = key;
            HasXAIKey = !string.IsNullOrEmpty(key);
            ShowSuccess("xAI (Grok) API Key saved securely.");
        }

        [RelayCommand]
        public void SaveOllamaHost(string host)
        {
            string cleanHost = string.IsNullOrWhiteSpace(host) ? "http://localhost:11434" : host;
            _storageService.SaveKey("Ollama (Local)", cleanHost);
            OllamaHost = cleanHost;
            HasOllamaKey = true;
            ShowSuccess("Ollama Host URL saved securely.");
        }

        [RelayCommand]
        public async Task ValidateKeyAsync(string provider)
        {
            string key = provider switch
            {
                "OpenAI" => OpenAIApiKey,
                "Anthropic" => AnthropicApiKey,
                "Google Gemini" or "Gemini" => GeminiApiKey,
                "Groq" => GroqApiKey,
                "DeepSeek" => DeepSeekApiKey,
                "Mistral AI" or "Mistral" => MistralApiKey,
                "OpenRouter" => OpenRouterApiKey,
                "xAI (Grok)" or "xAI" or "Grok" => XAIApiKey,
                "Ollama (Local)" or "Ollama" => OllamaHost,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(key) && provider != "Ollama (Local)" && provider != "Ollama")
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
                    ShowSuccess($"{provider} connection validated successfully! Found {models.Count} model(s).");
                }
                else
                {
                    ShowSuccess($"Could not validate {provider} connection.");
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
