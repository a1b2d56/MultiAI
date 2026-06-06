#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiAI.Views;
using System;

namespace MultiAI
{
    public sealed partial class MainWindow : Window
    {
        private Services.DatabaseService _db = new Services.DatabaseService();

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue("AppTheme", out object? themeObj) && themeObj is string themeStr)
            {
                if (Enum.TryParse(themeStr, out ElementTheme theme))
                {
                    SetTheme(theme);
                }
            }
        }

        public void SetTheme(ElementTheme theme)
        {
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }
        }

        public async void RefreshSidebar()
        {
            while (NavView.MenuItems.Count > 2)
            {
                NavView.MenuItems.RemoveAt(2);
            }

            var sessions = await _db.GetAllSessionsAsync();
            foreach (var session in sessions)
            {
                string cleanTitle = session.Title?.Replace("\r", "")?.Replace("\n", " ")?.Trim() ?? "New Chat";
                var item = new NavigationViewItem 
                { 
                    Icon = new SymbolIcon(Symbol.Message),
                    Content = cleanTitle,
                    Tag = session.SessionId
                };
                NavView.MenuItems.Add(item);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshSidebar();
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(Views.ChatPage));
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Views.SettingsPage));
            }
            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                if (item.Tag?.ToString() == "NewChat")
                {
                    ContentFrame.Navigate(typeof(Views.ChatPage));
                }
                else if (item.Tag != null)
                {
                    ContentFrame.Navigate(typeof(Views.ChatPage));
                    if (ContentFrame.Content is Views.ChatPage chatPage)
                    {
                        chatPage.LoadSession(item.Tag.ToString()!);
                    }
                }
            }
        }
    }
}
