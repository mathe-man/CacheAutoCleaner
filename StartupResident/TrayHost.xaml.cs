using System.Diagnostics;
using System.Windows;
using CacheOperator;

namespace StartupResident;

public partial class TrayHost : Window
{
    public TrayHost() => InitializeComponent();

    private void Clean_Click(object s, RoutedEventArgs e) 
        => Operator.CleanCache();


    private void Quit_Click(object s, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}