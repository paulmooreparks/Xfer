using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public abstract class DictionaryElement : CollectionElement {
    protected Dictionary<string, KeyValuePairElement> _values = [];

    protected DictionaryElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _values.Count;

    public Element? GetValue(string key) => _values.TryGetValue(key, out var value) ? value : null;

    public override Element? GetElementAt(int index) => index >= 0 && index < Count ? _values.Values.ElementAt(index) : null;

    public void Add(string key, KeyValuePairElement value) => _values[key] = value;

    public override bool Add(Element element) {
        if (element is KeyValuePairElement kvp) {
            if (_values.ContainsKey(kvp.Key)) {
                return false;
            }

            _values.Add(kvp.Key, kvp);
            // Add to Children if not already present (for round-trip consistency)
            if (!Children.Contains(kvp)) {
                Children.Add(kvp);
                kvp.Parent = this;
            }
            return true;
        }
        return false;
    }

    public override string ToString() {
        // Serialize the dictionary
        return string.Join(" ", _values.Select(kvp => $"{kvp.Key}{kvp.Value}"));
    }
}
