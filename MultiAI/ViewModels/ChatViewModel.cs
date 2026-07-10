#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiAI.Models;
using MultiAI.Providers;
using MultiAI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiAI.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private readonly SecureStorageService _secureStorage;
        private CancellationTokenSource? _streamCts;

        [ObservableProperty]
        private string _sessionId = Guid.NewGuid().ToString();

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();

        [ObservableProperty]
        private ObservableCollection<string> _availableProviders = new ObservableCollection<string>(ProviderFactory.SupportedProviders);

        [ObservableProperty]
        private string _selectedProvider = "Google Gemini";

        [ObservableProperty]
        private ObservableCollection<string> _availableModels = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedModel = string.Empty;

        [ObservableProperty]
        private string _inputMessage = string.Empty;

        [ObservableProperty]
        private bool _isGenerating;

        [ObservableProperty]
        private bool _isDeleteVisible;

        [ObservableProperty]
        private string _statusText = string.Empty;

        public event Action? ScrollToBottomRequested;

        public ChatViewModel()
        {
            _db = new DatabaseService();
            _secureStorage = new SecureStorageService();
        }

        public async Task InitializeAsync()
        {
            await UpdateModelsForProviderAsync();
        }

        public async Task LoadSessionAsync(string sessionId)
        {
            SessionId = sessionId;
            Messages.Clear();
            var history = await _db.GetMessagesAsync(sessionId);
            foreach (var m in history)
            {
                Messages.Add(m);
            }

            var session = await _db.GetSessionAsync(sessionId);
            if (session != null)
            {
                if (!string.IsNullOrEmpty(session.Provider) && AvailableProviders.Contains(session.Provider))
                {
                    SelectedProvider = session.Provider;
                }

                await UpdateModelsForProviderAsync(session.Model);
                IsDeleteVisible = true;
            }

            ScrollToBottomRequested?.Invoke();
        }

        public async Task OnProviderChangedAsync()
        {
            await UpdateModelsForProviderAsync();
        }

        public async Task UpdateModelsForProviderAsync(string? targetModel = null)
        {
            AvailableModels.Clear();

            try
            {
                var providerObj = ProviderFactory.GetProvider(SelectedProvider);
                string key = _secureStorage.GetKey(SelectedProvider) ?? string.Empty;

                var models = await providerObj.GetAvailableModelsAsync(key);
                foreach (var m in models)
                {
                    AvailableModels.Add(m);
                }

                if (!string.IsNullOrEmpty(targetModel) && AvailableModels.Contains(targetModel))
                {
                    SelectedModel = targetModel;
                }
                else if (AvailableModels.Count > 0)
                {
                    SelectedModel = AvailableModels[0];
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error loading models: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputMessage) || IsGenerating) return;

            string userInput = InputMessage.Trim();
            InputMessage = string.Empty;
            bool isFirstMessage = Messages.Count == 0;

            var userMsg = new Message
            {
                SessionId = SessionId,
                Role = "You",
                Content = userInput,
                Timestamp = DateTime.Now
            };

            Messages.Add(userMsg);
            await _db.SaveMessageAsync(userMsg);
            ScrollToBottomRequested?.Invoke();

            string apiKey = _secureStorage.GetKey(SelectedProvider) ?? string.Empty;
            if (string.IsNullOrEmpty(apiKey))
            {
                var systemErr = new Message
                {
                    SessionId = SessionId,
                    Role = SelectedProvider,
                    Content = $"Please save your {SelectedProvider} API key in Settings first.",
                    Timestamp = DateTime.Now
                };
                Messages.Add(systemErr);
                await _db.SaveMessageAsync(systemErr);
                return;
            }

            IsGenerating = true;
            StatusText = $"{SelectedProvider} is thinking...";

            var aiMsg = new Message
            {
                SessionId = SessionId,
                Role = SelectedProvider,
                Content = "",
                Timestamp = DateTime.Now
            };

            Messages.Add(aiMsg);
            int aiMsgIndex = Messages.Count - 1;

            _streamCts = new CancellationTokenSource();
            var history = Messages.Where(m => m != userMsg && m != aiMsg).ToList();

            try
            {
                var providerObj = ProviderFactory.GetProvider(SelectedProvider);
                providerObj.Initialize(apiKey, SelectedModel);

                var tokenStream = providerObj.StreamMessageAsync(userInput, history, _streamCts.Token);

                bool receivedTokens = false;
                await foreach (var token in tokenStream)
                {
                    receivedTokens = true;
                    aiMsg.Content += token;
                    Messages[aiMsgIndex] = new Message
                    {
                        Id = aiMsg.Id,
                        SessionId = aiMsg.SessionId,
                        Role = aiMsg.Role,
                        Content = aiMsg.Content,
                        Timestamp = aiMsg.Timestamp
                    };
                    ScrollToBottomRequested?.Invoke();
                }

                if (!receivedTokens || string.IsNullOrWhiteSpace(aiMsg.Content))
                {
                    aiMsg.Content = await providerObj.SendMessageAsync(userInput, history);
                    Messages[aiMsgIndex] = aiMsg;
                }
            }
            catch (Exception ex)
            {
                aiMsg.Content = $"Error: {ex.Message}";
                Messages[aiMsgIndex] = aiMsg;
            }
            finally
            {
                IsGenerating = false;
                StatusText = string.Empty;
                await _db.SaveMessageAsync(aiMsg);
            }

            if (isFirstMessage)
            {
                await CreateSessionWithAutoTitleAsync(userInput, apiKey);
            }
            else
            {
                var currentSession = await _db.GetSessionAsync(SessionId);
                if (currentSession != null)
                {
                    currentSession.LastUpdatedAt = DateTime.Now;
                    await _db.SaveSessionAsync(currentSession);
                    (App.Current as App)?.MainWindowRef?.RefreshSidebar();
                }
            }

            IsDeleteVisible = true;
        }

        private async Task CreateSessionWithAutoTitleAsync(string firstPrompt, string apiKey)
        {
            var session = new ChatSession
            {
                SessionId = SessionId,
                Title = "New Chat",
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                Provider = SelectedProvider,
                Model = SelectedModel
            };

            try
            {
                var gemini = new GeminiProvider();
                string geminiKey = _secureStorage.GetKey("Google Gemini") ?? apiKey;
                if (!string.IsNullOrEmpty(geminiKey))
                {
                    gemini.Initialize(geminiKey, "gemini-1.5-flash");
                    string titlePrompt = $"Generate a very short 3-to-5 word title summarizing this message. Only return the title itself, no quotes, no extra text: {firstPrompt}";
                    string title = await gemini.SendMessageAsync(titlePrompt, new List<Message>());
                    if (!string.IsNullOrEmpty(title) && !title.StartsWith("Error"))
                    {
                        session.Title = title.Replace("\r", "").Replace("\n", " ").Trim('"', ' ');
                    }
                }
            }
            catch { }

            if (session.Title == "New Chat" && !string.IsNullOrWhiteSpace(firstPrompt))
            {
                var clean = firstPrompt.Replace("\r", "").Replace("\n", " ");
                var words = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                session.Title = string.Join(" ", words.Take(5));
                if (words.Length > 5) session.Title += "...";
            }

            await _db.SaveSessionAsync(session);
            (App.Current as App)?.MainWindowRef?.RefreshSidebar();
        }

        [RelayCommand]
        public async Task DeleteChatAsync()
        {
            await _db.DeleteSessionAsync(SessionId);
            (App.Current as App)?.MainWindowRef?.RefreshSidebar();

            SessionId = Guid.NewGuid().ToString();
            Messages.Clear();
            IsDeleteVisible = false;
        }

        [RelayCommand]
        public void CancelGeneration()
        {
            _streamCts?.Cancel();
            IsGenerating = false;
            StatusText = "Generation cancelled.";
        }
    }
}
