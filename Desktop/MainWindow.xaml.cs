using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
}