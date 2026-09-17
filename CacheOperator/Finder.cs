namespace CacheOperator;

public class Finder
{
    public IFileSystemElement[] Find(string name, Folder startingFolder)
    {
        return RecursiveSearch(name, startingFolder).ToArray();
    }

    private List<IFileSystemElement> RecursiveSearch(string name, Folder start)
    {
        var result = new List<IFileSystemElement>();
        foreach (var child in start.GetChildren())
        {
            if (child.Name.Contains(name))
                result.Add(child);

            else if (child is Folder folder)
            {
                var down = RecursiveSearch(name, folder);
                if (down.Count > 0)
                    result.AddRange(down);
            }
        }
        
        return result;
    }
}