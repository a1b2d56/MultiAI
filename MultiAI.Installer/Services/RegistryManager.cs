#nullable enable
using Microsoft.Win32;
using System;
using System.IO;

namespace MultiAI.Installer.Services
{
    public static class RegistryManager
    {
        private const string UninstallRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Multi.AI";

        public static void RegisterInstallation(string installDir, string version, long sizeInBytes)
        {
            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(UninstallRegistryKey, true);
                if (key == null) return;

                string mainExe = Path.Combine(installDir, "MultiAI.exe");
                string uninstallerExe = Path.Combine(installDir, "Uninstall.exe");

                key.SetValue("DisplayName", "Multi.AI", RegistryValueKind.String);
                key.SetValue("DisplayVersion", version, RegistryValueKind.String);
                key.SetValue("Publisher", "Ananmay Jha", RegistryValueKind.String);
                key.SetValue("DisplayIcon", $"{mainExe},0", RegistryValueKind.String);
                key.SetValue("InstallLocation", installDir, RegistryValueKind.String);
                key.SetValue("UninstallString", $"\"{uninstallerExe}\"", RegistryValueKind.String);
                key.SetValue("QuietUninstallString", $"\"{uninstallerExe}\" /quiet", RegistryValueKind.String);
                key.SetValue("EstimatedSize", (int)(sizeInBytes / 1024), RegistryValueKind.DWord);
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"), RegistryValueKind.String);
                key.SetValue("URLInfoAbout", "https://github.com/a1b2d56/MultiAI", RegistryValueKind.String);
                key.SetValue("HelpLink", "https://github.com/a1b2d56/MultiAI/issues", RegistryValueKind.String);
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
            catch
            {
                // Fallback to CurrentUser if HKLM is unavailable
                try
                {
                    using var userKey = Registry.CurrentUser.CreateSubKey(UninstallRegistryKey, true);
                    if (userKey == null) return;

                    string mainExe = Path.Combine(installDir, "MultiAI.exe");
                    string uninstallerExe = Path.Combine(installDir, "Uninstall.exe");

                    userKey.SetValue("DisplayName", "Multi.AI", RegistryValueKind.String);
                    userKey.SetValue("DisplayVersion", version, RegistryValueKind.String);
                    userKey.SetValue("Publisher", "Ananmay Jha", RegistryValueKind.String);
                    userKey.SetValue("DisplayIcon", $"{mainExe},0", RegistryValueKind.String);
                    userKey.SetValue("InstallLocation", installDir, RegistryValueKind.String);
                    userKey.SetValue("UninstallString", $"\"{uninstallerExe}\"", RegistryValueKind.String);
                    userKey.SetValue("QuietUninstallString", $"\"{uninstallerExe}\" /quiet", RegistryValueKind.String);
                    userKey.SetValue("EstimatedSize", (int)(sizeInBytes / 1024), RegistryValueKind.DWord);
                    userKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"), RegistryValueKind.String);
                    userKey.SetValue("URLInfoAbout", "https://github.com/a1b2d56/MultiAI", RegistryValueKind.String);
                    userKey.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    userKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
        }
    }
}
