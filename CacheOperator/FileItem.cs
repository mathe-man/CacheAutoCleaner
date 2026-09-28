namespace CacheOperator;

public class FileItem : IFileSystemElement
{
    public bool Exists { get; set; }
    public string Name { get; }
    public string FullPath { get; }
    
    public FileItem(string fullPath)
    {
        if  (string.IsNullOrEmpty(fullPath))
            throw new ArgumentNullException(nameof(fullPath));

        
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