using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public ObservableCollection<FileSystemElement> Elements { get; } = new();


    public ViewModel()
    {
        Elements = new ObservableCollection<FileSystemElement>(Memory.Load());

        foreach (var e in Elements)
            e.PropertyChanged += OnElementPropertyChanged;


        Elements.CollectionChanged += (_, args) =>
        {
            // Subscribe for changes in new elements
            if (args.NewItems != null)
                foreach (FileSystemElement i in args.NewItems)
                    i.PropertyChanged += OnElementPropertyChanged;

            // Unsubscribe from removed elements
            if (args.OldItems != null)
                foreach (FileSystemElement i in args.OldItems)
                    i.PropertyChanged -= OnElementPropertyChanged;

            SaveElements();
        };
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(FileSystemElement.FullPath))
            SaveElements();
    }
    
    
    [RelayCommand]
    private void AddElement()
    {
        if (string.IsNullOrWhiteSpace(NewElementPath))
            return;
        
        Elements.Add(new FileSystemElement(NewElementPath));
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