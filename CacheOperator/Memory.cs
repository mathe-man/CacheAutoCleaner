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
    private static string _paramsSavingFile = FilePathInDataDirectory("params");
    
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

    
    
    private static Dictionary<string, string> GetParams()
    {
        Dictionary<string, string> parameters = new();

        if (File.Exists(_paramsSavingFile))
        {
            string existing = File.ReadAllText(_paramsSavingFile);

            foreach (var pair in existing.Split(";", StringSplitOptions.RemoveEmptyEntries))
                parameters[pair.Split('=')[0]] = pair.Split('=')[1]; ;

        }

        return parameters;
    }
    
    
    public static void SetParam(string key, string value)
    {
        var parameters = GetParams();
        
        parameters[key] = value;

        string result = string.Empty;
        foreach (var pair in parameters)
            result += $"{pair.Key}={pair.Value};";
        
        File.WriteAllText(_paramsSavingFile, result);
    }

    
    public static string GetParam(string key)
    {
        var parameters = GetParams();

        if (!parameters.ContainsKey(key))
            return string.Empty;
        
        return parameters[key]; 
    }    
}
