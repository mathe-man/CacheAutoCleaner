using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CacheOperator;

namespace Desktop;

public partial class ViewModel : ObservableObject
{

    [ObservableProperty] 
    private string _newElementPath = string.Empty;
    
    
    [ObservableProperty] 
    private int _cleaningIntervalSeconds = 100;

    public ObservableCollection<FileSystemElement> Elements = new();


    [RelayCommand]
    private void AddElement()
    {
        if (string.IsNullOrWhiteSpace(NewElementPath))
            return;
        
        Elements.Add(new FileSystemElement(NewElementPath));
        
        Memory.Save(Elements.ToList());
    }

    [RelayCommand]
    private void SaveElements()
        => Memory.Save(Elements.ToList());
    
    [RelayCommand]
    private void StartCleaningStandby()
        => Operator.StartParallelStandby(CleaningIntervalSeconds);
    
    [RelayCommand]
    public void StopCleaningStandby()
        => Operator.StopParallelStandby();
}