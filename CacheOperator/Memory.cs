namespace CacheOperator;


public static class Memory
{
    public static string DataDirectory
    {
        get
        {
            string path = Path.Combine
            (
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "AutoCacheCleaner"
            );

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            return path;
        }
    }
    
    
    private static string FilePathInDataDirectory(string relativefilePath)
        => Path.Combine(DataDirectory, relativefilePath);
    
    private static string _itemsSavingFile = FilePathInDataDirectory("items");
    
    public static void Save(List<FileSystemElement> elements)
    {

        List<string> stringPaths = new();

        foreach (var elem in elements)
            stringPaths.Add(elem.FullPath);
        
            
        
        File.WriteAllLines(_itemsSavingFile,  stringPaths);
    }

    public static List<FileSystemElement> Load()
    {
        var result = new List<FileSystemElement>();
        
        if (File.Exists(_itemsSavingFile))
            foreach (var path in File.ReadAllLines(_itemsSavingFile))
                result.Add(new FileSystemElement(path));

        return result;
    }
}