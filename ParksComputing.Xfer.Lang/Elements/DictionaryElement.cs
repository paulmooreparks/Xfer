using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public abstract class DictionaryElement : CollectionElement {
    /// <summary>
    /// Holds only semantic key-value pairs (not PIs/comments)
    /// </summary>
    protected Dictionary<string, KeyValuePairElement> _values = [];

    protected DictionaryElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _values.Count;

    /// <summary>
    /// Get a semantic value by key (returns null if not found)
    /// </summary>
    public Element? GetValue(string key) => _values.TryGetValue(key, out var value) ? value : null;

    public override Element? GetElementAt(int index) => index >= 0 && index < Count ? _values.Values.ElementAt(index) : null;

    /// <summary>
    /// Add a semantic key-value pair. Non-semantic elements (PIs/comments) should be added to Children only.
    /// </summary>
    public void Add(string key, KeyValuePairElement value) => _values[key] = value;

    public override bool Add(Element element) {
        if (element is KeyValuePairElement kvp) {
            if (_values.ContainsKey(kvp.Key)) {
                return false;
            }
            _values.Add(kvp.Key, kvp);
            if (!Children.Contains(kvp)) {
                Children.Add(kvp);
                kvp.Parent = this;
            }
            return true;
        } else if (element is ParksComputing.Xfer.Lang.ProcessingInstructions.ProcessingInstruction || element is CommentElement) {
            // Non-semantic: add only to Children
            if (!Children.Contains(element)) {
                Children.Add(element);
                element.Parent = this;
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
