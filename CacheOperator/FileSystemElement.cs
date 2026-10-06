using CommunityToolkit.Mvvm.ComponentModel;

namespace CacheOperator;

public partial class FileSystemElement : ObservableObject
{
    
    public bool Exists { get; protected set;  }

    public string Name
    { get
        { return new FileInfo(FullPath).Name; } }

    
    [ObservableProperty] private string _fullPath; 

    public bool IsFolder
    {
        get { return Directory.Exists(FullPath); }
    }

    public bool IsFile
    {
        get { return File.Exists(FullPath); }
    }
    

    public FileSystemElement(string fullPath)
    {
        FullPath = fullPath;
    }


    public bool Delete()
    {
        try
        {
            if (IsFile) {
                File.Delete(FullPath);
                return true;
            }

            if (IsFolder) {
                Directory.Delete(FullPath);
                return true;
            }

            return false;
        }
        
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e);
        }
        return false;
    }

    public FileSystemElement[] GetChildren()
    {
        if (!IsFolder)
            return [];

        List<FileSystemElement> children = new ();
        var dir = new DirectoryInfo(FullPath);
        
        foreach (var child in dir.EnumerateDirectories())
            children.Add(new FileSystemElement(child.FullName));
        
        foreach (var child in dir.EnumerateFiles())
            children.Add(new FileSystemElement(child.FullName));


        return children.ToArray();
    }
    

    public bool Contains(string name)
    {
        return Name.Contains(name, StringComparison.OrdinalIgnoreCase);
    }
}