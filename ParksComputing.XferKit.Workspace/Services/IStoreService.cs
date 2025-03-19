namespace ParksComputing.XferKit.Workspace.Services;

public interface IStoreService : IDictionary<string, object>
{
    void ClearStore();
    void Delete(string key);
    object? Get(string key);
    void Set(string key, object value);
}