#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiAI.ViewModels;
using System;

namespace MultiAI
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SetAppIcon();

            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue("AppTheme", out object? themeObj) && themeObj is string themeStr)
            {
                if (Enum.TryParse(themeStr, out ElementTheme theme))
                {
                    SetTheme(theme);
                }
            }
        }

        private void SetAppIcon()
        {
            try
            {
                string iconPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Assets", "AppIcon.png");
                if (System.IO.File.Exists(iconPath))
                {
                    this.AppWindow.SetIcon(iconPath);
                }
            }
            catch { }
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
            await ViewModel.LoadSessionsAsync();

            while (NavView.MenuItems.Count > 2)
            {
                NavView.MenuItems.RemoveAt(2);
            }

            foreach (var session in ViewModel.Sessions)
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
