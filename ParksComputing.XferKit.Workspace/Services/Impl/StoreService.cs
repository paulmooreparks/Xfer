using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using ParksComputing.XferKit.DataStore.Services;

namespace ParksComputing.XferKit.Workspace.Services.Impl;

internal class SqliteStoreService : IStoreService {
    private readonly IKeyValueStore _store;

    public SqliteStoreService(IKeyValueStore store) {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public object this[string key] {
        get => _store[key];
        set => _store[key] = value;
    }

    public ICollection<string> Keys => _store.Keys;
    public ICollection<object> Values => _store.Values.Cast<object>().ToList();
    public int Count => _store.Count;
    public bool IsReadOnly => _store.IsReadOnly;

    public void Add(string key, object value) => _store.Add(key, value);
    public bool ContainsKey(string key) => _store.ContainsKey(key);
    public bool Remove(string key) => _store.Remove(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _store.TryGetValue(key, out value);
    public void Add(KeyValuePair<string, object> item) => _store.Add(item);
    public void Clear() => _store.Clear();
    public bool Contains(KeyValuePair<string, object> item) => _store.Contains(item);
    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _store.CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<string, object> item) => _store.Remove(item);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _store.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void ClearStore() => _store.Clear();
    public void Delete(string key) => _store.Remove(key);
    public object? Get(string key) => _store.TryGetValue(key, out var value) ? value : null;
    public void Set(string key, object value) => _store[key] = value;
}
