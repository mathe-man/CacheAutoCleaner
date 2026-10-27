using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Setup;

public static class InstallPaths
{
    public const string AppName = "AutoCacheCleaner";
    public const string ShortcutFileName = "AutoCacheCleaner.lnk";

    public static readonly string StartMenuShortcutPath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
        InstallPaths.ShortcutFileName);

    public const string UninstallKeyPath =
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppName;

    
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


public record InstallOptions(string InstallDir, bool StartWithWindows);

public record InstallProgress(double Progress, string Message);

public class InstallException(string message) : Exception(message);

public class DirectProgress<T>(Action<T> handler) : IProgress<T>
{
    public void Report(T value) => handler(value);
}

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
    
    public void CreateShortcut(string installDir)
    {
        string target = Path.Combine(installDir, InstallPaths.MainExeName);

        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                         ?? throw new InvalidOperationException("Unable to find shell type");

        dynamic shell = Activator.CreateInstance(shellType)!;

        try
        {
            dynamic link = shell.CreateShortcut(InstallPaths.StartMenuShortcutPath);
            link.TargetPath = target;
            link.WorkingDirectory = installDir;
            link.IconLocation = target + ",0";
            link.Description = InstallPaths.AppName;
            link.Save();
        }
        finally
        {
            Marshal.FinalReleaseComObject(shell);
        }
    }
    
    public void DeleteShortcut()
    {
        File.Delete(InstallPaths.StartMenuShortcutPath);
    }
    
    private void RemoveRegistry(string installDir)
    {
        SetStartup(installDir, false);
        Registry.CurrentUser.DeleteSubKeyTree(InstallPaths.UninstallKeyPath, throwOnMissingSubKey: false);
    }

    public void Install(InstallOptions options, IProgress<InstallProgress>? progress = null)
    {
        string dir = options.InstallDir;
        bool freshInstall = DetectExistingInstall() is null;
        bool dirExisted = Directory.Exists(dir);

        // Stack of actions to undo in case of abortion
        var undo = new Stack<Action>();
        
        
        void Report(double report, string message) =>
            progress?.Report(new InstallProgress(report, message));

        try
        {
            Report(0.05, "Closing running applications...");
            if (!CloseRunningApps(dir))
                throw new InstallException("AutoCacheCleaner is still running. Please close it and try again.");


            Report(0.10, "Copying files...");
            // Push the undo action before starting it => preventing an error during this action
            undo.Push(() =>
            {
                if (!dirExisted && Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
            });

            ExtractPayload(dir, new DirectProgress<double>(f => Report(0.10 + f * 0.70, "Copying files...")));

            Report(0.85, "Creating the uninstaller...");
            CopyUninstaller(dir);

            Report(0.90, "Registering the application...");
            undo.Push(() => RemoveRegistry(dir));
            WriteRegistry(dir, options.StartWithWindows);

            Report(0.97, "Creating the Start menu shortcut...");
            undo.Push(DeleteShortcut);
            CreateShortcut(dir);
            

            Report(1.0, "Installation complete.");
        }
        
        catch
        {
            if (freshInstall)
            {
                while (undo.Count > 0)
                {
                    try { undo.Pop()(); }
                    catch { /* Cleaning shouldn't hide the crash error */ }
                }
            }
            throw;
        }
    }
    
    public bool Uninstall()
    {
        // Get the installation directory from the register, otherwise use the local directory of the current exe
        string dir = DetectExistingInstall()?.InstallDir
            ?? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        if (!CloseRunningApps(dir))
            return false;

        DeleteShortcut();         // Remove the shortcut in the start menu
        RemoveRegistry(dir);
        
        if (IsSafeToDelete(dir))
            ScheduleDirectoryDeletion(dir);

        return true;
    }

    // Prevent from deleting from directory such as the root
    private static bool IsSafeToDelete(string dir)
    {
        string full = Path.GetFullPath(dir);

        bool isRoot = string.Equals(Path.GetPathRoot(full), full + Path.DirectorySeparatorChar,
                                    StringComparison.OrdinalIgnoreCase);

        return !isRoot
            && Path.GetFileName(full).Equals(InstallPaths.AppName, StringComparison.OrdinalIgnoreCase)
            && File.Exists(Path.Combine(full, InstallPaths.UninstallerFileName));
    }

    private static void ScheduleDirectoryDeletion(string dir)
    {
        // Wait a couple second then delete
        var info = new ProcessStartInfo("cmd.exe",
            $"/c ping 127.0.0.1 -n 4 > nul & rmdir /s /q \"{dir}\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process.Start(info);
    }
}