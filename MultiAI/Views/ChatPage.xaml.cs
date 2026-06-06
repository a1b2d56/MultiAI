#nullable enable
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MultiAI.Views
{
    public sealed partial class ChatPage : Page
    {
        private Dictionary<string, List<string>> _modelsByProvider = new Dictionary<string, List<string>>()
        {
            // Hardcoded lists for now. Ideally we'd hit their respective /models endpoints dynamically,
            // but OpenAI and Anthropic API keys aren't always set up initially by the user.
            { "OpenAI", new List<string> { "gpt-4o", "gpt-4o-mini", "o1-mini" } },
            { "Anthropic", new List<string> { "claude-3-7-sonnet-latest", "claude-3-5-haiku-latest" } },
            { "Google Gemini", new List<string> { "gemini-2.5-pro", "gemini-2.0-flash", "gemini-1.5-pro" } }
        };

        private ObservableCollection<Models.Message> _messages = new ObservableCollection<Models.Message>();
        private Services.DatabaseService _db = new Services.DatabaseService();
        private string _sessionId = System.Guid.NewGuid().ToString();

        public ChatPage()
        {
            this.InitializeComponent();
            ChatListView.ItemsSource = _messages;
        }

        public async void LoadSession(string sessionId)
        {
            _sessionId = sessionId;
            _messages.Clear();
            var msgs = await _db.GetMessagesAsync(sessionId);
            foreach (var m in msgs) _messages.Add(m);

            var sessions = await _db.GetAllSessionsAsync();
            var current = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (current != null && !string.IsNullOrEmpty(current.Provider))
            {
                // Temporarily unhook this event so we don't accidentally trigger a background fetch
                // and violently overwrite the UI state we are trying to restore.
                ProviderComboBox.SelectionChanged -= ProviderComboBox_SelectionChanged;

                foreach (ComboBoxItem item in ProviderComboBox.Items)
                {
                    if (item.Content?.ToString() == current.Provider)
                    {
                        ProviderComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                await UpdateModelsAsync(current.Model);

                ProviderComboBox.SelectionChanged += ProviderComboBox_SelectionChanged;
                DeleteChatButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }

        private async void ProviderComboBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await UpdateModelsAsync();
        }

        private async void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateModelsAsync();
        }

        private async System.Threading.Tasks.Task UpdateModelsAsync(string? targetModel = null)
        {
            if (ProviderComboBox?.SelectedItem is ComboBoxItem selectedItem && ModelComboBox != null)
            {
                string provider = selectedItem.Content?.ToString() ?? "";
                
                ModelComboBox.Items.Clear();

                if (provider == "Google Gemini")
                {
                    var secureStorage = new Services.SecureStorageService();
                    var key = secureStorage.GetKey("Gemini");
                    if (!string.IsNullOrEmpty(key))
                    {
                        var gemini = new Providers.GeminiProvider();
                        var dynamicModels = await gemini.GetAvailableModelsAsync(key);
                        foreach(var m in dynamicModels)
                        {
                            ModelComboBox.Items.Add(new ComboBoxItem { Content = m });
                        }
                        
                        SetModelSelection(targetModel);
                        return;
                    }
                }

                if (_modelsByProvider.TryGetValue(provider, out var models))
                {
                    foreach(var model in models)
                    {
                        ModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                    }
                    SetModelSelection(targetModel);
                }
            }
        }

        private void SetModelSelection(string? targetModel)
        {
            if (ModelComboBox == null || ModelComboBox.Items.Count == 0) return;
            
            if (!string.IsNullOrEmpty(targetModel))
            {
                foreach (ComboBoxItem item in ModelComboBox.Items)
                {
                    if (item.Content?.ToString() == targetModel)
                    {
                        ModelComboBox.SelectedItem = item;
                        return;
                    }
                }
            }
            ModelComboBox.SelectedIndex = 0;
        }

        private async void SendButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            string userInput = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(userInput)) return;

            bool isFirstMessage = _messages.Count == 0;

            // Immediately clear the input box so the app feels snappy and responsive
            InputTextBox.Text = "";

            var userMsg = new Models.Message { SessionId = _sessionId, Role = "You", Content = userInput, Timestamp = System.DateTime.Now };
            _messages.Add(userMsg);
            await _db.SaveMessageAsync(userMsg);

            string providerName = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string modelName = (ModelComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            
            string responseText = "";
            string? keyToUse = null;

            if (providerName == "Google Gemini")
            {
                var secureStorage = new Services.SecureStorageService();
                keyToUse = secureStorage.GetKey("Gemini");
                if (string.IsNullOrEmpty(keyToUse)) 
                {
                    responseText = "Please save your Gemini API key in Settings first.";
                }
                else
                {
                    var gemini = new Providers.GeminiProvider();
                    gemini.Initialize(keyToUse, modelName);
                    var history = _messages.Where(m => m != userMsg).ToList();
                    responseText = await gemini.SendMessageAsync(userInput, history);
                }
            }
            else
            {
                // TODO: Wire up the official SDKs for OpenAI and Anthropic!
                // For now, we just politely decline to do anything.
                responseText = $"The {providerName} provider logic is not fully wired up yet. Try Google Gemini!";
            }

            var aiMsg = new Models.Message { SessionId = _sessionId, Role = providerName, Content = responseText, Timestamp = System.DateTime.Now };
            _messages.Add(aiMsg);
            await _db.SaveMessageAsync(aiMsg);

            if (isFirstMessage)
            {
                var session = new Models.ChatSession { 
                    SessionId = _sessionId, 
                    Title = "New Chat", 
                    CreatedAt = System.DateTime.Now, 
                    LastUpdatedAt = System.DateTime.Now,
                    Provider = providerName,
                    Model = modelName
                };
                
                if (providerName == "Google Gemini" && !string.IsNullOrEmpty(keyToUse))
                {
                    try {
                        // Fire off a silent background request to summarize the chat.
                        // We use flash here because it's super fast, cheap, and gets the job done.
                        var gemini = new Providers.GeminiProvider();
                        gemini.Initialize(keyToUse, "gemini-1.5-flash");
                        string prompt = $"Generate a very short 3-to-5 word title summarizing this message. Only return the title itself, no quotes, no extra text: {userInput}";
                        string title = await gemini.SendMessageAsync(prompt, new List<Models.Message>());
                        if (!string.IsNullOrEmpty(title) && !title.StartsWith("Error"))
                        {
                            session.Title = title.Replace("\r", "").Replace("\n", " ").Trim('"', ' ');
                        }
                    } catch {}
                }
                
                if (session.Title == "New Chat" && !string.IsNullOrWhiteSpace(userInput))
                {
                    // Fallback: If the API failed (or they used an unwired provider), 
                    // just grab the first few words so the sidebar isn't completely useless.
                    var cleanInput = userInput.Replace("\r", "").Replace("\n", " ");
                    var words = cleanInput.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    session.Title = string.Join(" ", words.Take(5));
                    if (words.Length > 5) session.Title += "...";
                }

                await _db.SaveSessionAsync(session);
                (App.Current as App)?.MainWindowRef?.RefreshSidebar();
                DeleteChatButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                var sessions = await _db.GetAllSessionsAsync();
                var current = sessions.FirstOrDefault(s => s.SessionId == _sessionId);
                if (current != null)
                {
                    current.LastUpdatedAt = System.DateTime.Now;
                    await _db.SaveSessionAsync(current);
                    (App.Current as App)?.MainWindowRef?.RefreshSidebar();
                }
            }
        }

        private async void DeleteChat_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await _db.DeleteSessionAsync(_sessionId);
            (App.Current as App)?.MainWindowRef?.RefreshSidebar();
            
            _sessionId = System.Guid.NewGuid().ToString();
            _messages.Clear();
        }
    }
}
