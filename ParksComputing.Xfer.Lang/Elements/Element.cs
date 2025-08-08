using System;
using System.Collections.Generic;
using System.Linq;

using ParksComputing.Xfer.Lang;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// The abstract base class for all XferLang elements. Provides common functionality
/// for element hierarchies, metadata, IDs, and serialization.
/// </summary>
public abstract class Element {
    /// <summary>
    /// Gets or sets the parent element of this element in the document hierarchy.
    /// </summary>
    public Element? Parent { get; set; }
    private readonly List<Element> children = [];

    /// <summary>
    /// The character prefix used to indicate hexadecimal numeric literals ('$').
    /// </summary>
    public const char HexadecimalPrefix = '$';

    /// <summary>
    /// The character prefix used to indicate binary numeric literals ('%').
    /// </summary>
    public const char BinaryPrefix = '%';

    /// <summary>
    /// The character that opens all XferLang elements ('&lt;').
    /// </summary>
    public const char ElementOpeningCharacter = '<';

    /// <summary>
    /// The character that closes all XferLang elements ('&gt;').
    /// </summary>
    public const char ElementClosingCharacter = '>';

    /// <summary>
    /// Gets the name of this element type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the delimiter information for this element, including opening/closing characters and style.
    /// </summary>
    public ElementDelimiter Delimiter { get; set; } = new ElementDelimiter('\0', '\0');

    /// <summary>
    /// Gets the collection of child elements contained within this element.
    /// </summary>
    public List<Element> Children => children;

    /// <summary>
    /// Optional ID for this element, settable via inline PI: &lt;! id "myId" !&gt;
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Optional tag for this element, settable via inline PI: &lt;! tag "tagName" !&gt;
    /// Tags allow non-unique categorization of elements for grouping and selection.
    /// Each element can have only one tag.
    /// </summary>
    public string? Tag { get; set; }

    public virtual object? ParsedValue { get; set; } = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    /// <param name="name">The name of the element type.</param>
    /// <param name="delimiter">The delimiter information for this element.</param>
    public Element(string name, ElementDelimiter delimiter) {
        Name = name;
        Delimiter = delimiter;
    }

    /// <summary>
    /// Recursively finds the first descendant element with the specified ID.
    /// </summary>
    /// <param name="id">The ID to search for.</param>
    /// <returns>The found element, or null if no element is found.</returns>
    public Element? FindElementById(string id) {
        if (string.Equals(Id, id, StringComparison.Ordinal)) {
            return this;
        }

        foreach (var child in Children) {
            var found = child.FindElementById(id);
            if (found != null) {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Recursively finds all descendant elements with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>A collection of elements that have the specified tag.</returns>
    public IReadOnlyCollection<Element> FindElementsByTag(string tag) {
        var result = new List<Element>();

        if (string.Equals(Tag, tag, StringComparison.Ordinal)) {
            result.Add(this);
        }

        foreach (var child in Children) {
            var childResults = child.FindElementsByTag(tag);
            result.AddRange(childResults);
        }

        return result;
    }

    /// <summary>
    /// Recursively finds all descendant elements of the specified type.
    /// </summary>
    /// <typeparam name="T">The element type to search for.</typeparam>
    /// <returns>A collection of elements of the specified type.</returns>
    public IReadOnlyCollection<T> FindElementsByType<T>() where T : Element {
        var result = new List<T>();

        if (this is T element) {
            result.Add(element);
        }

        foreach (var child in Children) {
            var childResults = child.FindElementsByType<T>();
            result.AddRange(childResults);
        }

        return result;
    }

    /// <summary>
    /// Recursively finds all descendant elements of the specified type.
    /// </summary>
    /// <param name="elementType">The element type to search for.</param>
    /// <returns>A collection of elements of the specified type.</returns>
    public IReadOnlyCollection<Element> FindElementsByType(Type elementType) {
        var result = new List<Element>();

        if (elementType.IsAssignableFrom(this.GetType())) {
            result.Add(this);
        }

        foreach (var child in Children) {
            var childResults = child.FindElementsByType(elementType);
            result.AddRange(childResults);
        }

        return result;
    }

    /// <summary>
    /// Gets the next sibling element in the parent's children collection.
    /// </summary>
    public Element? NextSibling {
        get {
            if (Parent == null) {
                return null;
            }

            var siblings = Parent.Children;
            var index = siblings.IndexOf(this);

            return index >= 0 && index < siblings.Count - 1 ? siblings[index + 1] : null;
        }
    }

    /// <summary>
    /// Gets the previous sibling element in the parent's children collection.
    /// </summary>
    public Element? PreviousSibling {
        get {
            if (Parent == null) {
                return null;
            }

            var siblings = Parent.Children;
            var index = siblings.IndexOf(this);

            return index > 0 ? siblings[index - 1] : null;
        }
    }

    /// <summary>
    /// Gets the first child element, or null if this element has no children.
    /// </summary>
    public Element? FirstChild => Children.Count > 0 ? Children[0] : null;

    /// <summary>
    /// Gets the last child element, or null if this element has no children.
    /// </summary>
    public Element? LastChild => Children.Count > 0 ? Children[Children.Count - 1] : null;

    /// <summary>
    /// Gets all sibling elements (excluding this element).
    /// </summary>
    /// <returns>A collection of sibling elements.</returns>
    public IEnumerable<Element> GetSiblings() {
        if (Parent == null) {
            return Enumerable.Empty<Element>();
        }

        return Parent.Children.Where(child => child != this);
    }

    /// <summary>
    /// Gets all ancestor elements from parent up to the root.
    /// </summary>
    /// <returns>A collection of ancestor elements, ordered from immediate parent to root.</returns>
    public IEnumerable<Element> GetAncestors() {
        var current = Parent;
        while (current != null) {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Gets all descendant elements in depth-first order.
    /// </summary>
    /// <returns>A collection of all descendant elements.</returns>
    public IEnumerable<Element> GetDescendants() {
        foreach (var child in Children) {
            yield return child;
            foreach (var descendant in child.GetDescendants()) {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Serializes this element to its XferLang string representation using default formatting.
    /// </summary>
    /// <returns>The XferLang string representation of this element</returns>
    public abstract string ToXfer();

    /// <summary>
    /// Serializes this element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this element</returns>
    public abstract string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0);

    /// <summary>
    /// Adds a child element to this element's children collection.
    /// Automatically sets the parent relationship and prevents duplicate additions.
    /// </summary>
    /// <param name="child">The child element to add</param>
    public virtual void AddChild(Element child) {
        if (!children.Contains(child)) {
            children.Add(child);
            child.Parent = this;
        }
    }

    /// <summary>
    /// Removes this element from its parent element.
    /// DOM-like removal that handles parent-child relationship cleanup.
    /// </summary>
    /// <returns>True if the element was successfully removed, false if it had no parent</returns>
    public virtual bool Remove() {
        if (Parent == null) {
            return false;
        }

        return Parent.RemoveChild(this);
    }

    /// <summary>
    /// Removes a specific child element from this element's children collection.
    /// Automatically clears the parent relationship.
    /// </summary>
    /// <param name="child">The child element to remove</param>
    /// <returns>True if the child was found and removed, false otherwise</returns>
    public virtual bool RemoveChild(Element child) {
        if (children.Remove(child)) {
            child.Parent = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a child element at the specified index from this element's children collection.
    /// Automatically clears the parent relationship.
    /// </summary>
    /// <param name="index">The zero-based index of the child to remove</param>
    /// <returns>True if the child was successfully removed, false if index was out of range</returns>
    public virtual bool RemoveChildAt(int index) {
        if (index < 0 || index >= children.Count) {
            return false;
        }

        var child = children[index];
        children.RemoveAt(index);
        child.Parent = null;
        return true;
    }

    /// <summary>
    /// Removes all child elements from this element's children collection.
    /// Automatically clears parent relationships for all removed children.
    /// </summary>
    /// <returns>The number of children that were removed</returns>
    public virtual int RemoveAllChildren() {
        var count = children.Count;

        // Clear parent relationships
        foreach (var child in children) {
            child.Parent = null;
        }

        children.Clear();
        return count;
    }

    /// <summary>
    /// Replaces a child element with a new element.
    /// Automatically manages parent-child relationships.
    /// </summary>
    /// <param name="oldChild">The existing child element to replace</param>
    /// <param name="newChild">The new child element to add in its place</param>
    /// <returns>True if the replacement was successful, false if oldChild was not found</returns>
    public virtual bool ReplaceChild(Element oldChild, Element newChild) {
        var index = children.IndexOf(oldChild);
        if (index == -1) {
            return false;
        }

        // Clear old parent relationship
        oldChild.Parent = null;

        // Set up new relationships
        children[index] = newChild;
        newChild.Parent = this;

        return true;
    }
}
