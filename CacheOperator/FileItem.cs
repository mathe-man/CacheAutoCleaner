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
        Name = new FileInfo(fullPath).Name;
        Exists = File.Exists(fullPath);
    }
    
    public bool Delete()
    {
        if (Exists)
        {
            try
            {
                File.Delete(FullPath);
                Exists = false;
                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        return false;
    }

    public IFileSystemElement[] GetChildren()
    {
        // File don't have any child
        return [];
    }
}