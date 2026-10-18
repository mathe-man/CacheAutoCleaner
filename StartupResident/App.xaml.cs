using System.Windows;
using CacheOperator;

namespace StartupResident;

public partial class App : Application
{
    public App()
    {
        
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        if (Memory.GetParam("StartupCleanAndClose") == "true")
        {
            Operator.CleanCache();
            Shutdown();
        }
    }
}