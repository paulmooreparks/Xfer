using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang;

namespace ParksComputing.XferKit.Workspace.Services.Impl;

public class StoreService : IStoreService {
    private readonly string _storeFilePath;
    private Dictionary<string, object> _store;
    private FileSystemWatcher? _watcher;
    private DateTime _lastModified;
    private readonly object _lock = new();

    public StoreService(string storeFilePath) {
        _storeFilePath = storeFilePath;
        _store = LoadStore();
        _lastModified = File.Exists(_storeFilePath) ? File.GetLastWriteTimeUtc(_storeFilePath) : DateTime.MinValue;

        StartFileWatcher();
    }

    private Dictionary<string, object> LoadStore() {
        if (!File.Exists(_storeFilePath))
            return new Dictionary<string, object>();

        lock (_lock) {
            try {
                _lastModified = File.GetLastWriteTimeUtc(_storeFilePath);
                var xfer = File.ReadAllText(_storeFilePath);
                var table = XferConvert.Deserialize(xfer, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
                return table ?? new Dictionary<string, object>();
                // return XferConvert.Deserialize<Dictionary<string, object>>(xfer);
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"[STORE] Error loading store: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }
    }

    private void SaveStore() {
        lock (_lock) {
            try {
                var xfer = XferConvert.Serialize(_store, Formatting.Pretty);
                File.WriteAllText(_storeFilePath, xfer);
                _lastModified = File.GetLastWriteTimeUtc(_storeFilePath);
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"[STORE] Error saving store: {ex.Message}");
            }
        }
    }

    private void StartFileWatcher() {
        var directory = Path.GetDirectoryName(_storeFilePath);
        var fileName = Path.GetFileName(_storeFilePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            return;

        _watcher = new FileSystemWatcher(directory, fileName) {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };

        _watcher.Changed += (s, e) => ReloadIfChanged();
        _watcher.Renamed += (s, e) => ReloadIfChanged();
        _watcher.EnableRaisingEvents = true;
    }

    private void ReloadIfChanged() {
        lock (_lock) {
            var lastWriteTime = File.GetLastWriteTimeUtc(_storeFilePath);

            if (lastWriteTime > _lastModified) {
                Console.WriteLine("[STORE] Store file changed externally, reloading...");
                _store = LoadStore();
            }
        }
    }

    public object? Get(string key) {
        ReloadIfNeeded();
        return _store.TryGetValue(key, out var value) ? value : null;
    }

    public void Set(string key, object value) {
        _store[key] = value;
        SaveStore();
    }

    public void Delete(string key) {
        if (_store.Remove(key))
            SaveStore();
    }

    public void Clear() {
        _store.Clear();
        SaveStore();
    }

    private void ReloadIfNeeded() {
        var lastWriteTime = File.GetLastWriteTimeUtc(_storeFilePath);
        if (lastWriteTime > _lastModified) {
            Console.WriteLine("[STORE] Detected modification, reloading...");
            _store = LoadStore();
        }
    }
}
