#nullable enable
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MultiAI.Uninstaller.Services
{
    public class UninstallEngine
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Multi.AI";

        public static async Task ExecuteUninstallAsync(Action<string> onStatusUpdate)
        {
            await Task.Run(() =>
            {
                onStatusUpdate("Stopping any running Multi.AI processes...");
                KillRunningInstances();

                onStatusUpdate("Removing Start Menu and Desktop shortcuts...");
                RemoveShortcuts();

                onStatusUpdate("Removing Windows Registry entries...");
                RemoveRegistryEntries();

                onStatusUpdate("Scheduling removal of application files...");
                ScheduleSelfDeletion();
            });
        }

        private static void KillRunningInstances()
        {
            try
            {
                var processes = Process.GetProcessesByName("MultiAI");
                foreach (var p in processes)
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch { }
                }

                var dotProcesses = Process.GetProcessesByName("Multi.AI");
                foreach (var p in dotProcesses)
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch { }
                }
            }
            catch { }
        }

        private static void RemoveShortcuts()
        {
            try
            {
                string publicDesktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Multi.AI.lnk");
                if (File.Exists(publicDesktop)) File.Delete(publicDesktop);

                string userDesktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Multi.AI.lnk");
                if (File.Exists(userDesktop)) File.Delete(userDesktop);

                string commonStartMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "Multi.AI.lnk");
                if (File.Exists(commonStartMenu)) File.Delete(commonStartMenu);

                string userStartMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Multi.AI.lnk");
                if (File.Exists(userStartMenu)) File.Delete(userStartMenu);
            }
            catch { }
        }

        private static void RemoveRegistryEntries()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
                key?.DeleteSubKeyTree("Multi.AI", false);
            }
            catch { }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
                key?.DeleteSubKeyTree("Multi.AI", false);
            }
            catch { }
        }

        private static void ScheduleSelfDeletion()
        {
            try
            {
                string installDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                string cmd = $"/C ping 127.0.0.1 -n 3 > nul & rmdir /S /Q \"{installDir}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmd,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(psi);
            }
            catch { }
        }
    }
}
