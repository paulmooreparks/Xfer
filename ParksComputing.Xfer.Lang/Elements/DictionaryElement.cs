using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public abstract class DictionaryElement : CollectionElement<KeyValuePairElement> {
    protected Dictionary<string, KeyValuePairElement> _values = [];

    protected DictionaryElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _values.Count;

    public Element? GetValue(string key) => _values.TryGetValue(key, out var value) ? value : null;

    public override KeyValuePairElement? GetElementAt(int index) => index < Count ? _values.Values.ElementAt(index) : null;

    public void Add(string key, KeyValuePairElement value) => _values[key] = value;

    public override bool Add(KeyValuePairElement value) {
        if (_values.ContainsKey(value.Key)) {
            return false;
        }

        _values.Add(value.Key, value);
        // Add to Children if not already present (for round-trip consistency)
        if (this is Element e && !e.Children.Contains(value)) {
            e.Children.Add(value);
        }
        return true;
    }

    public override string ToString() {
        // Serialize the dictionary
        return string.Join(" ", _values.Select(kvp => $"{kvp.Key}{kvp.Value}"));
    }
}
