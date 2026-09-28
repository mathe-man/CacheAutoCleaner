namespace CacheOperator;

public class Folder : IFileSystemElement
{
    public bool Exists { get; set; }
    public string Name { get; }
    public string FullPath { get; }
    
    
    public Folder(string fullPath)
    {
        FullPath = fullPath;
        Name = new DirectoryInfo(fullPath).Name;
        Exists = Directory.Exists(fullPath);
    }

    public bool Delete()
    {
        if (Exists)
        {
            try
            {
                Directory.Delete(FullPath, true);
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
        var children = new List<IFileSystemElement>();
        if (!Exists)
            return children.ToArray();
        
        try
        {
            foreach (var folder in Directory.EnumerateDirectories(FullPath))
            {
                children.Add(new Folder(folder));
            }

            foreach (var file in Directory.EnumerateFiles(FullPath))
            {
                children.Add(new FileItem(file));
            }
        }
        catch (UnauthorizedAccessException e) {}
        

        return children.ToArray();
    }
}