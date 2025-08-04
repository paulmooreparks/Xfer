using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class for elements that contain collections of other elements.
/// Provides common functionality for array-like and object-like elements that store
/// multiple child elements with appropriate indexing and enumeration capabilities.
/// </summary>
public abstract class CollectionElement : Element {
    /// <summary>
    /// Initializes a new CollectionElement with the specified element name and delimiter configuration.
    /// </summary>
    /// <param name="elementName">The name of this collection element type.</param>
    /// <param name="delimiter">The delimiter configuration for this collection element.</param>
    protected CollectionElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    /// <summary>
    /// Gets the number of elements in this collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Internal storage for the collection elements.
    /// </summary>
    protected List<Element> _items = [];

    /// <summary>
    /// Gets an enumerable view of all elements in this collection.
    /// </summary>
    public virtual IEnumerable<Element> Values => _items;

    /// <summary>
    /// Retrieves the element at the specified index in the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The element at the specified index, or null if the index is out of range.</returns>
    public abstract Element? GetElementAt(int index);

    /// <summary>
    /// Adds an element to this collection.
    /// </summary>
    /// <param name="element">The element to add to the collection.</param>
    /// <returns>True if the element was successfully added; otherwise, false.</returns>
    public abstract bool Add(Element element);
}
