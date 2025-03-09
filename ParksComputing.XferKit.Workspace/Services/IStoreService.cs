namespace ParksComputing.XferKit.Workspace.Services;

public interface IStoreService
{
    void Clear();
    void Delete(string key);
    object? Get(string key);
    void Set(string key, object value);
}