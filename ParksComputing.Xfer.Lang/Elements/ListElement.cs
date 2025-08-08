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
    /// Removes the specified element from this list.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if the element was found and removed, false otherwise</returns>
    public virtual bool Remove(Element element) {
        if (element == null) {
            return false;
        }

        bool removedFromItems = _items.Remove(element);
        bool removedFromChildren = Children.Remove(element);

        if (removedFromItems || removedFromChildren) {
            element.Parent = null;
        }

        return removedFromItems || removedFromChildren;
    }

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove</param>
    /// <returns>True if the element was found and removed, false otherwise</returns>
    public virtual bool RemoveAt(int index) {
        if (index < 0 || index >= _items.Count) {
            return false;
        }

        var element = _items[index];
        _items.RemoveAt(index);
        Children.Remove(element);
        element.Parent = null;

        return true;
    }

    /// <summary>
    /// Removes a specific child element from this list.
    /// Overrides base implementation to handle both _items and Children collections.
    /// </summary>
    /// <param name="child">The child element to remove</param>
    /// <returns>True if the child was found and removed, false otherwise</returns>
    public override bool RemoveChild(Element child) {
        if (child == null) {
            return false;
        }

        bool removedFromItems = _items.Remove(child);
        bool removedFromChildren = Children.Remove(child);

        if (removedFromItems || removedFromChildren) {
            child.Parent = null;
        }

        return removedFromItems || removedFromChildren;
    }

    /// <summary>
    /// Removes a child element at the specified index.
    /// Overrides base implementation to handle both _items and Children collections.
    /// </summary>
    /// <param name="index">The zero-based index of the child to remove</param>
    /// <returns>True if the child was successfully removed, false if index was out of range</returns>
    public override bool RemoveChildAt(int index) {
        return RemoveAt(index);
    }

    /// <summary>
    /// Removes all child elements from this list.
    /// Overrides base implementation to handle both _items and Children collections.
    /// </summary>
    /// <returns>The number of children that were removed</returns>
    public override int RemoveAllChildren() {
        var count = _items.Count;

        // Clear parent relationships
        foreach (var child in _items) {
            child.Parent = null;
        }

        _items.Clear();
        Children.Clear();
        return count;
    }

    /// <summary>
    /// Replaces a child element with a new element.
    /// Overrides base implementation to handle both _items and Children collections.
    /// </summary>
    /// <param name="oldChild">The existing child element to replace</param>
    /// <param name="newChild">The new child element to add in its place</param>
    /// <returns>True if the replacement was successful, false if oldChild was not found</returns>
    public override bool ReplaceChild(Element oldChild, Element newChild) {
        var index = _items.IndexOf(oldChild);
        if (index == -1) {
            return false;
        }

        // Clear old parent relationship
        oldChild.Parent = null;

        // Set up new relationships
        _items[index] = newChild;
        newChild.Parent = this;

        // Also update Children collection if the old child was there
        var childIndex = Children.IndexOf(oldChild);
        if (childIndex != -1) {
            Children[childIndex] = newChild;
        }

        return true;
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
