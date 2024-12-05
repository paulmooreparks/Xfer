using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;
public abstract class DictionaryElement : CollectionElement {
    protected Dictionary<string, Element> _values = new();

    protected DictionaryElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _values.Count;

    public Element? GetValue(string key) => _values.TryGetValue(key, out var value) ? value : null;

    public override Element? GetElementAt(int index) => index < Count ? _values.Values.ElementAt(index) : null;

    public void Add(string key, Element value) => _values[key] = value;

    public override void Add(Element element) {
        if (element is KeyValuePairElement kvp) {
            _values[kvp.Key] = kvp.Value;
        }
        else {
            throw new InvalidOperationException($"Only KeyValuePairElement is allowed in {GetType().Name}");
        }
    }

    public override string ToString() {
        // Serialize the dictionary
        return string.Join(" ", _values.Select(kvp => $"{kvp.Key}{kvp.Value}"));
    }
}
