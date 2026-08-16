#nullable enable
using MultiAI.Installer.Services;
using MultiAI.Installer.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace MultiAI.Installer
{
    public partial class MainWindow : FluentWindow
    {
        private int _currentStep = 0;

        private readonly WelcomeView _welcomeView = new();
        private readonly LicenseView _licenseView = new();
        private readonly LocationView _locationView = new();
        private readonly ProgressView _progressView = new();
        private readonly CompleteView _completeView = new();

        public MainWindow()
        {
            InitializeComponent();
            _licenseView.OnAcceptanceChanged += accepted =>
            {
                if (_currentStep == 1)
                {
                    NextButton.IsEnabled = accepted;
                }
            };
            ShowStep(0);
        }

        private void ShowStep(int step)
        {
            _currentStep = step;

            // Reset breadcrumb styles
            var activeBrush = (Brush)FindResource("AccentTextFillColorPrimaryBrush");
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
            var doneBrush = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));

            Step1Text.Foreground = step == 0 ? activeBrush : (step > 0 ? doneBrush : inactiveBrush);
            Step2Text.Foreground = step == 1 ? activeBrush : (step > 1 ? doneBrush : inactiveBrush);
            Step3Text.Foreground = step == 2 ? activeBrush : (step > 2 ? doneBrush : inactiveBrush);
            Step4Text.Foreground = step >= 3 ? activeBrush : inactiveBrush;

            switch (step)
            {
                case 0: // Welcome
                    MainContainer.Content = _welcomeView;
                    BackButton.IsEnabled = false;
                    NextButton.Content = "Next";
                    NextButton.IsEnabled = true;
                    CancelButton.IsEnabled = true;
                    break;

                case 1: // License
                    MainContainer.Content = _licenseView;
                    BackButton.IsEnabled = true;
                    NextButton.Content = "Next";
                    NextButton.IsEnabled = _licenseView.IsAccepted;
                    CancelButton.IsEnabled = true;
                    break;

                case 2: // Location & Options
                    MainContainer.Content = _locationView;
                    BackButton.IsEnabled = true;
                    NextButton.Content = "Install";
                    NextButton.IsEnabled = true;
                    CancelButton.IsEnabled = true;
                    break;

                case 3: // Progress
                    MainContainer.Content = _progressView;
                    BackButton.IsEnabled = false;
                    NextButton.IsEnabled = false;
                    CancelButton.IsEnabled = false;
                    StartInstallation();
                    break;

                case 4: // Complete
                    MainContainer.Content = _completeView;
                    BackButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    NextButton.Content = "Finish";
                    NextButton.IsEnabled = true;
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 0 && _currentStep < 3)
            {
                ShowStep(_currentStep - 1);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < 2)
            {
                ShowStep(_currentStep + 1);
            }
            else if (_currentStep == 2)
            {
                ShowStep(3); // Start Install
            }
            else if (_currentStep == 4)
            {
                if (_completeView.ShouldLaunch)
                {
                    InstallerEngine.LaunchApp(_locationView.InstallPath);
                }
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void StartInstallation()
        {
            string installPath = _locationView.InstallPath;
            bool desktopShortcut = _locationView.CreateDesktopShortcut;
            bool startMenuShortcut = _locationView.CreateStartMenuShortcut;

            try
            {
                await InstallerEngine.InstallAsync(
                    installPath,
                    desktopShortcut,
                    startMenuShortcut,
                    (status, progress) =>
                    {
                        _progressView.UpdateProgress(status, progress);
                    });

                await Task.Delay(800);
                ShowStep(4); // Move to complete view
            }
            catch (Exception ex)
            {
                _progressView.UpdateProgress($"Installation error: {ex.Message}", 0);
                CancelButton.IsEnabled = true;
                CancelButton.Content = "Close";
            }
        }
    }
}
