#nullable enable
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MultiAI.Installer.Views
{
    public partial class LocationView : UserControl
    {
        public string InstallPath => PathTextBox.Text.Trim();
        public bool CreateDesktopShortcut => DesktopShortcutCheckBox.IsChecked == true;
        public bool CreateStartMenuShortcut => StartMenuShortcutCheckBox.IsChecked == true;
        public bool LaunchAfterInstall => LaunchAfterCheckBox.IsChecked == true;

        public LocationView()
        {
            InitializeComponent();
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            PathTextBox.Text = Path.Combine(programFiles, "Multi.AI");
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Multi.AI Installation Directory",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (dialog.ShowDialog() == true)
            {
                string selected = dialog.FolderName;
                if (!selected.EndsWith("Multi.AI", StringComparison.OrdinalIgnoreCase))
                {
                    selected = Path.Combine(selected, "Multi.AI");
                }
                PathTextBox.Text = selected;
            }
        }
    }
}
