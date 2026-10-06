namespace CacheOperator;


public static class Memory
{
    private static string _itemsSavingFile = "items";
    
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