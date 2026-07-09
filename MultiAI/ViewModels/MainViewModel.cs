#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiAI.Models;
using MultiAI.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MultiAI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        [ObservableProperty]
        private ObservableCollection<ChatSession> _sessions = new ObservableCollection<ChatSession>();

        [ObservableProperty]
        private ChatSession? _selectedSession;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public MainViewModel()
        {
            _db = new DatabaseService();
        }

        [RelayCommand]
        public async Task LoadSessionsAsync()
        {
            var sessions = await _db.GetAllSessionsAsync();
            Sessions.Clear();
            foreach (var session in sessions)
            {
                if (string.IsNullOrWhiteSpace(SearchQuery) || 
                    session.Title.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase))
                {
                    Sessions.Add(session);
                }
            }
        }
    }
}
