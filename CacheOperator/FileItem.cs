namespace CacheOperator;

public class FileItem : IFileSystemElement
{
    public bool Exists { get; set; }
    public string Name { get; }
    public string FullPath { get; }
    
    public FileItem(string fullPath, bool ignoreExisting = false)
    {
        // Create the object even if the given directory doesn't exist if needed
        if (!ignoreExisting)
        {
            if  (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath));
        
            if (!File.Exists(fullPath))
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
            File.Delete(FullPath);
            Exists = false;
        }
    }

    public IFileSystemElement[] GetChildren()
    {
        // File don't have any child
        return [];
    }
}