#nullable enable
using System;
using System.Windows;
using System.Windows.Controls;

namespace MultiAI.Installer.Views
{
    public partial class LicenseView : UserControl
    {
        public event Action<bool>? OnAcceptanceChanged;

        public bool IsAccepted => AcceptCheckBox.IsChecked == true;

        public LicenseView()
        {
            InitializeComponent();
        }

        private void AcceptCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            OnAcceptanceChanged?.Invoke(IsAccepted);
        }
    }
}
