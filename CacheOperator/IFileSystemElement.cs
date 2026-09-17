namespace CacheOperator;

public interface IFileSystemElement
{
    public bool Exists { get; protected set;  }
    public string Name { get; }
    public string FullPath { get; }
    
    public void Delete();
    public IFileSystemElement[] GetChildren();

    public bool Contains(string name)
    {
        return Name.Contains(name);
    }
}