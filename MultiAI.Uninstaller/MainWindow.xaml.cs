#nullable enable
using MultiAI.Uninstaller.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace MultiAI.Uninstaller
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            UninstallButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            UninstallProgressBar.Visibility = Visibility.Visible;
            StatusText.Visibility = Visibility.Visible;

            await UninstallEngine.ExecuteUninstallAsync(status =>
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = status;
                });
            });

            await Task.Delay(1000);
            PromptText.Text = "Multi.AI has been successfully removed from your computer.";
            UninstallProgressBar.Visibility = Visibility.Collapsed;
            StatusText.Text = "Completed";
            
            CancelButton.Content = "Close";
            CancelButton.IsEnabled = true;
            UninstallButton.Visibility = Visibility.Collapsed;
        }
    }
}
