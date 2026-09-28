namespace CacheOperator;


public static class Memory
{
    private static string _directoriesSavingFile = "directories";
    private static string _fileSavingFile = "files";
    
    public static void Save(List<IFileSystemElement> elements)
    {

        List<string> foldersPaths = new();
        List<string> filesPaths = new();

        foreach (var elem in elements)
        {
            if (elem is Folder folder)
                foldersPaths.Add(folder.FullPath);
            
            else if (elem is FileItem  file)
                filesPaths.Add(file.FullPath);
        }
            
        
        File.WriteAllLines(_directoriesSavingFile,  foldersPaths);
        File.WriteAllLines(_fileSavingFile,  filesPaths);
    }

    public static List<IFileSystemElement> Load()
    {
        var result = new List<IFileSystemElement>();
        
        if (File.Exists(_directoriesSavingFile))
            foreach (var path in File.ReadAllLines(_directoriesSavingFile))
                result.Add(new Folder(path, true));
        
        if (File.Exists(_fileSavingFile))
            foreach (var path in File.ReadAllLines(_fileSavingFile))
                result.Add(new FileItem(path, true));

        return result;
    }
}