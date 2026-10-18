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
        
        Operator.CleanCache();
        
        if (Memory.GetParam("StayUpAfterStartupCleaning") != "true") {
            Shutdown();
        }
    }
}