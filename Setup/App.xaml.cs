using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace Setup;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var service = new InstallService();
        string dir = InstallPaths.DefaultInstallDir;

        ExistingInstall? before = service.DetectExistingInstall();

        service.CloseRunningApps(dir);
        service.ExtractPayload(dir);
        service.CopyUninstaller(dir);
        service.WriteRegistry(dir, startWithWindows: true);

        ExistingInstall? after = service.DetectExistingInstall();

        // MessageBox.Show(string messageBoxText)
        MessageBox.Show($"Avant : {(before is null ? "aucune" : before.InstallDir)}\n" +
                        $"Après : {(after is null ? "aucune" : after.InstallDir)}");
        Shutdown();
    }
}