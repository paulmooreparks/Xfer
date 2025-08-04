using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class representing a list-based collection element in XferLang.
/// Manages ordered collections of semantic items with proper parent-child relationships.
/// Non-semantic elements (processing instructions, comments) are stored separately from semantic items.
/// </summary>
public abstract class ListElement : CollectionElement {
    /// <summary>
    /// Holds only semantic items (not PIs/comments)
    /// </summary>

    protected ListElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    /// <summary>
    /// Gets the number of semantic items in this list (excludes processing instructions and comments).
    /// </summary>
    public override int Count => _items.Count;

    /// <summary>
    /// Gets the semantic element at the specified index, or null if the index is out of bounds.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve</param>
    /// <returns>The element at the specified index, or null if index is invalid</returns>
    public override Element? GetElementAt(int index) => index >= 0 && index < Count ? _items[index] : null;

    /// <summary>
    /// Add a semantic item. Non-semantic elements (PIs/comments) should be added to Children only.
    /// </summary>
    public override bool Add(Element element) {
        if (element is ParksComputing.Xfer.Lang.ProcessingInstructions.ProcessingInstruction || element is CommentElement) {
            // Non-semantic: add only to Children
            if (!Children.Contains(element)) {
                Children.Add(element);
                element.Parent = this;
            }
            return true;
        }
        _items.Add(element);
        if (!Children.Contains(element)) {
            Children.Add(element);
            element.Parent = this;
        }
        return true;
    }

    /// <summary>
    /// Returns a string representation of this list by joining all semantic items with spaces.
    /// </summary>
    /// <returns>A space-separated string of all semantic items in the list</returns>
    public override string ToString() {
        return string.Join(" ", _items);
    }

    /// <summary>
    /// Gets or sets the semantic element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set</param>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of bounds</exception>
    public Element this[int index] {
        get {
            return _items[index];
        }
        set {
            _items[index] = value;
        }
    }
}
