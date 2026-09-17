namespace CacheOperator;

public class Folder : IFileSystemElement
{
    public bool Exists { get; set; }
    public string Name { get; }
    public string FullPath { get; }
    
    
    public Folder(string fullPath, bool ignoreExisting = false)
    {
        // Create the object even if the given directory doesn't exist if needed
        if (!ignoreExisting)
        {
            if  (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath));
        
            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException();
        }
        
        FullPath = fullPath;
        Name = new DirectoryInfo(fullPath).Name;
        Exists = Directory.Exists(fullPath);
    }

    public void Delete()
    {
        if (Exists)
        {
            Directory.Delete(FullPath, true);
            Exists = false;
        }
    }

    public IFileSystemElement[] GetChildren()
    {
        var children = new List<IFileSystemElement>();

        foreach (var folder in Directory.EnumerateDirectories(FullPath))
        {
            children.Add(new Folder(folder));
        }

        foreach (var file in Directory.EnumerateFiles(FullPath))
        {
            children.Add(new FileItem(file));
        }

        return children.ToArray();
    }
}