#nullable enable
using System.Windows.Controls;

namespace MultiAI.Installer.Views
{
    public partial class CompleteView : UserControl
    {
        public bool ShouldLaunch => LaunchCheckBox.IsChecked == true;

        public CompleteView()
        {
            InitializeComponent();
        }
    }
}
