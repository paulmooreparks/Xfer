using System;
using System.Collections.Generic;

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
    /// Optional metadata for this element, set via meta PI.
    /// </summary>
    public XferMetadata? Metadata { get; set; }

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
}
