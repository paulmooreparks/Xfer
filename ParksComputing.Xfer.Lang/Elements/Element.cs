using System;
using System.Collections.Generic;

using ParksComputing.Xfer.Lang;

namespace ParksComputing.Xfer.Lang.Elements;

public abstract class Element {
    public Element? Parent { get; set; }
    private readonly List<Element> children = [];

    public const char HexadecimalPrefix = '$';
    public const char BinaryPrefix = '%';
    public const char ElementOpeningCharacter = '<';
    public const char ElementClosingCharacter = '>';
    public string Name { get; }
    public ElementDelimiter Delimiter { get; set; } = new ElementDelimiter('\0', '\0');
    public List<Element> Children => children;

    /// <summary>
    /// Optional ID for this element, settable via inline PI: <! id "myId" !>
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Optional metadata for this element, set via meta PI.
    /// </summary>
    public XferMetadata? Metadata { get; set; }

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

    public abstract string ToXfer();

    public abstract string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0);
    public virtual void AddChild(Element child) {
        if (!children.Contains(child)) {
            children.Add(child);
            child.Parent = this;
        }
    }
}
