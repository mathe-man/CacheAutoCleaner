using System.Collections.ObjectModel;
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

    private ObservableCollection<IFileSystemElement> _elements = new();
    
    
    [RelayCommand]
    public void StartCleaningStandby()
        => Operator.StartParallelStandby(CleaningIntervalSeconds);
    
    [RelayCommand]
    public void StopCleaningStandby()
        => Operator.StopParallelStandby();
}