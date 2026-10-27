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
        
        if (e.Args.Contains("--uninstall"))
        {
            // MessageBox.Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
            // -> MessageBoxResult  (nombreux autres overloads)
            var answer = MessageBox.Show("Désinstaller AutoCacheCleaner ?", "Désinstallation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                bool ok = new InstallService().Uninstall();
                if (!ok)
                    MessageBox.Show("Ferme AutoCacheCleaner puis réessaie.", "Désinstallation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Application.Shutdown()
            // (overload : Shutdown(int exitCode))
            Shutdown();
            return;
        }
        

        try
        {
            var options = new InstallOptions(InstallPaths.DefaultInstallDir, StartWithWindows: true);
            new InstallService().Install(options);

            MessageBox.Show("Installed.", "Setup", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Setup failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        Shutdown();
    }
}