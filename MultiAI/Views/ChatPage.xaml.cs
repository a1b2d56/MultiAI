#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MultiAI.ViewModels;
using System.Threading.Tasks;

namespace MultiAI.Views
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; } = new ChatViewModel();

        public ChatPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            ViewModel.ScrollToBottomRequested += ViewModel_ScrollToBottomRequested;
            this.Loaded += ChatPage_Loaded;
        }

        private async void ChatPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        public async void LoadSession(string sessionId)
        {
            await ViewModel.LoadSessionAsync(sessionId);
        }

        private async void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.OnProviderChangedAsync();
        }

        private async void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                e.Handled = true;
                await ViewModel.SendMessageAsync();
            }
        }

        private void ViewModel_ScrollToBottomRequested()
        {
            if (ChatListView.Items.Count > 0)
            {
                ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]);
            }
        }
    }
}
