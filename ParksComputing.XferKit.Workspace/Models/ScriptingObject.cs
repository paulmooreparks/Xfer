using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;

namespace ParksComputing.XferKit.Workspace.Models;

public class ScriptingObject : DynamicObject, IDictionary<string, object?> {
    private readonly Dictionary<string, object?> _dynamicProperties = [];

    public ICollection<string> Keys => _dynamicProperties.Keys;
    public ICollection<object?> Values => _dynamicProperties.Values;
    public int Count => _dynamicProperties.Count;
    public bool IsReadOnly => false;

    public ScriptingObject() { }

    public bool ContainsKey(string key) {
        return _dynamicProperties.ContainsKey(key);
    }

    public void Add(string key, object? value) {
        _dynamicProperties.Add(key, value);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        return _dynamicProperties.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value) {
        _dynamicProperties[binder.Name] = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames() {
        return _dynamicProperties.Keys;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() {
        return _dynamicProperties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public bool Remove(string key) => _dynamicProperties.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) => _dynamicProperties.TryGetValue(key, out value);

    public void Add(KeyValuePair<string, object?> item) => _dynamicProperties.Add(item.Key, item.Value);

    public void Clear() => _dynamicProperties.Clear();

    public bool Contains(KeyValuePair<string, object?> item) => _dynamicProperties.ContainsKey(item.Key);

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) {
        if (array is null) {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index must be non-negative.");
        }

        if (array.Length - arrayIndex < _dynamicProperties.Count) {
            throw new ArgumentException("The target array does not have enough space to copy the elements.");
        }

        foreach (var kvp in _dynamicProperties) {
            array[arrayIndex++] = kvp;
        }
    }

    public bool Remove(KeyValuePair<string, object?> item) => _dynamicProperties.Remove(item.Key);

    public object? this[string propertyName] {
        get => _dynamicProperties.TryGetValue(propertyName, out var value) ? value : null;
        set => _dynamicProperties[propertyName] = value;
    }
}

public static class ScriptObjectExtensions {
    public static ScriptingObject FromObject(this object source) {
        var scriptObject = new ScriptingObject();

        if (source is null) {
            return scriptObject;
        }

        var properties = source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties) {
            var value = prop.GetValue(source);
            scriptObject[prop.Name] = value;
        }

        return scriptObject;
    }
}
