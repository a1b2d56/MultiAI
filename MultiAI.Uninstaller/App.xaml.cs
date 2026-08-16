#nullable enable
using MultiAI.Uninstaller.Services;
using System;
using System.Linq;
using System.Windows;

namespace MultiAI.Uninstaller
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Contains("/quiet", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("/silent", StringComparer.OrdinalIgnoreCase))
            {
                await UninstallEngine.ExecuteUninstallAsync(_ => { });
                Shutdown(0);
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
