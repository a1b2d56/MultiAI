#nullable enable
using System.Windows.Controls;

namespace MultiAI.Installer.Views
{
    public partial class ProgressView : UserControl
    {
        public ProgressView()
        {
            InitializeComponent();
        }

        public void UpdateProgress(string status, double percent)
        {
            Dispatcher.Invoke(() =>
            {
                StatusTextBlock.Text = status;
                InstallProgressBar.Value = percent;
                PercentTextBlock.Text = $"{(int)percent}%";
            });
        }
    }
}
