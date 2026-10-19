using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Win32;

namespace Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModel();
    }

    // Open the choice menu
    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var menu = BtnBrowse.ContextMenu;
        menu.PlacementTarget = BtnBrowse;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private void MenuFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a file",
            Filter = "Any (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
            PathInput.Text = dialog.FileName;
    }

    private void MenuFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder"
        };
        
        if (dialog.ShowDialog(this) == true)
            PathInput.Text = dialog.FolderName;
    }

    private void PeriodSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (e.NewValue == 0)
            PeriodText.Text = "Disabled";
        
        else if (e.NewValue == 1)
            PeriodText.Text = "30 minutes";
        
        else if (e.NewValue == 2)
            PeriodText.Text = "1 hour";
        
        else if (e.NewValue == 3)
            PeriodText.Text = "2 hours";
        
        else if (e.NewValue == 4)
            PeriodText.Text = "4 hours";
        
        else if (e.NewValue == 5)
            PeriodText.Text = "12 hours";
    }
}