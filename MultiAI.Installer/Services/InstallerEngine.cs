#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace MultiAI.Installer.Services
{
    public class InstallerEngine
    {
        public static async Task InstallAsync(
            string targetDir,
            bool createDesktopShortcut,
            bool createStartMenuShortcut,
            Action<string, double> onProgress)
        {
            await Task.Run(() =>
            {
                onProgress("Preparing installation directory...", 5);
                Directory.CreateDirectory(targetDir);

                onProgress("Extracting application files...", 10);
                long totalBytes = ExtractPayload(targetDir, onProgress);

                onProgress("Creating application shortcuts...", 85);
                ShortcutManager.CreateShortcuts(targetDir, createDesktopShortcut, createStartMenuShortcut);

                onProgress("Registering program in Windows Settings & Control Panel...", 95);
                RegistryManager.RegisterInstallation(targetDir, "1.0.0", totalBytes);

                onProgress("Installation completed successfully!", 100);
            });
        }

        private static long ExtractPayload(string targetDir, Action<string, double> onProgress)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // Try loading embedded resource payload.zip
            using Stream? stream = assembly.GetManifestResourceStream("MultiAI.Installer.Resources.payload.zip") 
                ?? (File.Exists("payload.zip") ? File.OpenRead("payload.zip") : null);

            if (stream == null)
            {
                throw new FileNotFoundException("Installer payload package not found inside installer.");
            }

            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            int totalEntries = archive.Entries.Count;
            int extracted = 0;
            long totalBytes = 0;

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    // Directory entry
                    string dirPath = Path.Combine(targetDir, entry.FullName);
                    Directory.CreateDirectory(dirPath);
                    continue;
                }

                string destinationPath = Path.Combine(targetDir, entry.FullName);
                string? destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                entry.ExtractToFile(destinationPath, true);
                totalBytes += entry.Length;
                extracted++;

                double progress = 10 + ((double)extracted / totalEntries * 70);
                if (extracted % 5 == 0 || extracted == totalEntries)
                {
                    onProgress($"Extracting: {entry.Name}", progress);
                }
            }

            return totalBytes;
        }

        public static void LaunchApp(string installDir)
        {
            try
            {
                string exePath = Path.Combine(installDir, "MultiAI.exe");
                if (File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = installDir,
                        UseShellExecute = true
                    });
                }
            }
            catch { }
        }
    }
}
