using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class representing a dictionary-based collection element in XferLang.
/// Manages key-value pair collections with efficient key-based lookups.
/// Non-semantic elements (processing instructions, comments) are stored separately from key-value pairs.
/// </summary>
public abstract class DictionaryElement : CollectionElement {
    /// <summary>
    /// Holds only semantic key-value pairs (not PIs/comments)
    /// </summary>
    protected Dictionary<string, KeyValuePairElement> _values = [];

    /// <summary>
    /// Initializes a new instance of the DictionaryElement class.
    /// </summary>
    /// <param name="elementName">The name of this dictionary element type</param>
    /// <param name="delimiter">The delimiter configuration for this element</param>
    protected DictionaryElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    /// <summary>
    /// Gets the number of key-value pairs in this dictionary (excludes processing instructions and comments).
    /// </summary>
    public override int Count => _values.Count;

    /// <summary>
    /// Get a semantic value by key (returns null if not found)
    /// </summary>
    public Element? GetValue(string key) => _values.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Gets the key-value pair element at the specified index, or null if the index is out of bounds.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve</param>
    /// <returns>The key-value pair element at the specified index, or null if index is invalid</returns>
    public override Element? GetElementAt(int index) => index >= 0 && index < Count ? _values.Values.ElementAt(index) : null;

    /// <summary>
    /// Add a semantic key-value pair. Non-semantic elements (PIs/comments) should be added to Children only.
    /// </summary>
    public void Add(string key, KeyValuePairElement value) => _values[key] = value;

    /// <summary>
    /// Adds an element to this dictionary. Supports key-value pairs and non-semantic elements.
    /// Key-value pairs are added to the dictionary, while processing instructions and comments
    /// are added only to the children collection.
    /// </summary>
    /// <param name="element">The element to add</param>
    /// <returns>True if the element was added successfully, false if it was a duplicate key-value pair</returns>
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

    /// <summary>
    /// Returns a string representation of this dictionary by serializing all key-value pairs.
    /// </summary>
    /// <returns>A space-separated string of all key-value pairs in the dictionary</returns>
    public override string ToString() {
        // Serialize the dictionary
        return string.Join(" ", _values.Select(kvp => $"{kvp.Key}{kvp.Value}"));
    }
}
