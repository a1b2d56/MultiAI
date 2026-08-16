#nullable enable
using System;
using System.IO;

namespace MultiAI.Installer.Services
{
    public static class ShortcutManager
    {
        public static void CreateShortcuts(string installDir, bool createDesktop, bool createStartMenu)
        {
            string targetExe = Path.Combine(installDir, "MultiAI.exe");
            if (!File.Exists(targetExe)) return;

            if (createDesktop)
            {
                try
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                    if (string.IsNullOrEmpty(desktopPath) || !Directory.Exists(desktopPath))
                    {
                        desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    }
                    string shortcutLocation = Path.Combine(desktopPath, "Multi.AI.lnk");
                    CreateShellLink(shortcutLocation, targetExe, installDir, "Multi.AI - Unified Multi-Provider AI Workspace");
                }
                catch { }
            }

            if (createStartMenu)
            {
                try
                {
                    string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
                    if (string.IsNullOrEmpty(startMenuPath) || !Directory.Exists(startMenuPath))
                    {
                        startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    }
                    string shortcutLocation = Path.Combine(startMenuPath, "Multi.AI.lnk");
                    CreateShellLink(shortcutLocation, targetExe, installDir, "Multi.AI - Unified Multi-Provider AI Workspace");
                }
                catch { }
            }
        }

        private static void CreateShellLink(string shortcutPath, string targetPath, string workingDir, string description)
        {
            try
            {
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) return;

                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDir;
                shortcut.Description = description;
                shortcut.IconLocation = $"{targetPath},0";
                shortcut.Save();
            }
            catch { }
        }
    }
}
