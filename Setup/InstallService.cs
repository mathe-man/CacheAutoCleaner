using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Win32;

namespace Setup;

public static class InstallPaths
{
    public const string AppName = "AutoCacheCleaner";
    public const string ShortcutFileName = "AutoCacheCleaner.lnk";
    
    public const string UninstallKeyPath =
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppName;

    //
    public static readonly string[] ProcessNames = ["Desktop", "StartupResident"];

    
    public static string DefaultInstallDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs", AppName);
    
    
    public const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string UninstallerFileName = "Uninstall.exe";

    // Files names
    public const string MainExeName = "Desktop.exe";
    public const string ResidentExeName = "StartupResident.exe";

    public const string Version = "1.0.0";
    public const string Publisher = "mathe-man";
}


public record ExistingInstall(string InstallDir, string? Version);


public class InstallService
{
    public void ExtractPayload(string installDir, IProgress<double>? progress = null)
    {
        Directory.CreateDirectory(installDir);

        using Stream stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("payload.zip")
            ?? throw new InvalidOperationException("Cannot find payload.zip");

        
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

        
        string root = Path.GetFullPath(installDir) + Path.DirectorySeparatorChar;
        int done = 0;

        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            string dest = Path.GetFullPath(Path.Combine(installDir, entry.FullName));

            // Zip slip protection
            if (!dest.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Invalid entry : {entry.FullName}");

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(dest);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

                entry.ExtractToFile(dest, overwrite: true);
            }

            progress?.Report(++done / (double)zip.Entries.Count);
        }
    }
    
    public ExistingInstall? DetectExistingInstall()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(InstallPaths.UninstallKeyPath);

        
        if (key?.GetValue("InstallLocation") is not string dir || !Directory.Exists(dir))
            return null;

        return new ExistingInstall(dir, key.GetValue("DisplayVersion") as string);
    }
    
    public bool CloseRunningApps(string installDir)
    {
        foreach (string name in InstallPaths.ProcessNames)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    if (!IsRunningFrom(process, installDir)) continue;

                    try
                    {
                        if (process.CloseMainWindow() && process.WaitForExit(3000)) continue;

                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(3000);
                    }
                    catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
                    {
                    }
                }
            }
        }

        return !IsAnyAppRunning(installDir);
    }
    
    private static bool IsAnyAppRunning(string installDir)
    {
        foreach (string name in InstallPaths.ProcessNames)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    if (IsRunningFrom(process, installDir)) return true;
                }
            }
        }
        return false;
    }

    private static bool IsRunningFrom(Process process, string installDir)
    {
        try
        {
            string? exePath = process.MainModule?.FileName;
            if (exePath is null) return false;

            string root = Path.GetFullPath(installDir) + Path.DirectorySeparatorChar;
            return exePath.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            return false;
        }
    }
    
    public void CopyUninstaller(string installDir)
    {
        string source = Environment.ProcessPath
                        ?? throw new InvalidOperationException("Cannot find the process path");
        string dest = Path.Combine(installDir, InstallPaths.UninstallerFileName);

        // When an update is launched from the uninstaller itself we don't (and can't) copy 
        if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(dest), StringComparison.OrdinalIgnoreCase))
            return;
        
        
        File.Copy(source, dest, overwrite: true);
    }
    
    
    public void WriteRegistry(string installDir, bool startWithWindows)
    {
        string uninstaller = Path.Combine(installDir, InstallPaths.UninstallerFileName);

        // Estimated size of the installation
        long bytes = new DirectoryInfo(installDir)
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Sum(f => f.Length);

        
        
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(InstallPaths.UninstallKeyPath))
        {
            key.SetValue("DisplayName", InstallPaths.AppName);
            key.SetValue("DisplayVersion", InstallPaths.Version);
            key.SetValue("Publisher", InstallPaths.Publisher);
            key.SetValue("InstallLocation", installDir);
            key.SetValue("DisplayIcon", Path.Combine(installDir, InstallPaths.MainExeName));
            key.SetValue("UninstallString", $"\"{uninstaller}\" --uninstall");
            key.SetValue("EstimatedSize", (int)(bytes / 1024), RegistryValueKind.DWord);
            key.SetValue("NoModify", 1, RegistryValueKind.DWord);
            key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        }

        SetStartup(installDir, startWithWindows);
    }
    
    public void SetStartup(string installDir, bool enabled)
    {
        using RegistryKey run = Registry.CurrentUser.CreateSubKey(InstallPaths.RunKeyPath);

        if (enabled)
        {
            string resident = Path.Combine(installDir, InstallPaths.ResidentExeName);
            run.SetValue(InstallPaths.AppName, $"\"{resident}\"");
        }
        else
        {
            run.DeleteValue(InstallPaths.AppName, throwOnMissingValue: false);
        }
    }
}