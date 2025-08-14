using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Represents a complete XferLang document with its root element, metadata, processing instructions,
/// and any parsing errors or warnings. This is the primary object model for working with XferLang content.
/// </summary>
public class XferDocument {
    /// <summary>
    /// Fast lookup map for element IDs to elements. Provides O(1) ID lookups.
    /// </summary>
    private readonly Dictionary<string, Element> _idIndex = new();

    /// <summary>
    /// Fast lookup map for tags to elements. Provides O(1) tag-based collection lookups.
    /// </summary>
    private readonly Dictionary<string, HashSet<Element>> _tagIndex = new();

    /// <summary>
    /// Flag indicating whether the ID index is current with the document structure.
    /// </summary>
    private bool _idIndexValid = false;

    /// <summary>
    /// Flag indicating whether the tag index is current with the document structure.
    /// </summary>
    private bool _tagIndexValid = false;

    /// <summary>
    /// Gets or sets the root element of the document. Defaults to an empty tuple element.
    /// All document content is contained within this root element.
    /// </summary>
    public CollectionElement Root { get; set; } = new TupleElement();

    /// <summary>
    /// Gets or sets the document metadata, if specified via document processing instructions.
    /// Contains information about the XferLang version, document version, and custom metadata.
    /// </summary>
    public XferMetadata? Metadata { get; set; }

    /// <summary>
    /// Document-level Processing Instructions that appear outside the root element.
    /// These are conceptually siblings to the root element.
    /// </summary>
    public List<ProcessingInstruction> ProcessingInstructions { get; set; } = new List<ProcessingInstruction>();

    /// <summary>
    /// The first fatal error encountered during parsing, if any.
    /// When an error is present, parsing stops and the document may be incomplete.
    /// </summary>
    public ParseError? Error { get; set; }

    /// <summary>
    /// Collection of non-fatal warnings encountered during parsing.
    /// Warnings indicate potential issues but don't prevent successful parsing.
    /// </summary>
    public List<ParseWarning> Warnings { get; set; } = new List<ParseWarning>();

    /// <summary>
    /// Gets a value indicating whether the document has a fatal error.
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// Gets a value indicating whether the document has any warnings.
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the document is valid (no fatal errors).
    /// Note: A document can be valid but still have warnings.
    /// </summary>
    public bool IsValid => !HasError;

    /// <summary>
    /// Initializes a new XferDocument with an empty root collection.
    /// </summary>
    public XferDocument() { }

    /// <summary>
    /// Initializes a new XferDocument with the specified root collection.
    /// </summary>
    /// <param name="root">The root collection element for the document.</param>
    public XferDocument(CollectionElement root) {
        Root = root;
        InvalidateIndexes();
    }

    /// <summary>
    /// Adds an element to the document's root collection.
    /// </summary>
    /// <param name="value">The element to add to the document.</param>
    public void Add(Element value) {
        Root.Add(value);
        InvalidateIndexes();
    }

    /// <summary>
    /// Builds or rebuilds the ID index by traversing the entire document tree.
    /// This provides fast O(1) lookups for GetElementById operations.
    /// </summary>
    private void BuildIdIndex() {
        _idIndex.Clear();
        BuildIdIndexRecursive(Root);
        _idIndexValid = true;
    }

    /// <summary>
    /// Builds or rebuilds the tag index by traversing the entire document tree.
    /// This provides fast O(1) lookups for GetElementsByTag operations.
    /// </summary>
    private void BuildTagIndex() {
        _tagIndex.Clear();
        BuildTagIndexRecursive(Root);
        _tagIndexValid = true;
    }

    /// <summary>
    /// Recursively builds the ID index for all elements in the tree.
    /// </summary>
    /// <param name="element">The element to process and recurse into.</param>
    private void BuildIdIndexRecursive(Element element) {
        // Add this element to the index if it has an ID
        if (!string.IsNullOrEmpty(element.Id)) {
            _idIndex[element.Id] = element;
        }

        // Recursively process all children
        foreach (var child in element.Children) {
            BuildIdIndexRecursive(child);
        }
    }

    /// <summary>
    /// Recursively builds the tag index for all elements in the tree.
    /// </summary>
    /// <param name="element">The element to process and recurse into.</param>
    private void BuildTagIndexRecursive(Element element) {
        // Add this element to the tag index for each tag
        if (element.Tags != null && element.Tags.Count > 0) {
            foreach (var tag in element.Tags) {
                if (string.IsNullOrEmpty(tag)) { continue; }
                if (!_tagIndex.TryGetValue(tag, out var elements)) {
                    elements = new HashSet<Element>();
                    _tagIndex[tag] = elements;
                }
                elements.Add(element);
            }
        }

        // Recursively process all children
        foreach (var child in element.Children) {
            BuildTagIndexRecursive(child);
        }
    }

    /// <summary>
    /// Marks both the ID and tag indexes as invalid, requiring a rebuild on next access.
    /// This should be called whenever the document structure changes.
    /// </summary>
    private void InvalidateIndexes() {
        _idIndexValid = false;
        _tagIndexValid = false;
    }

    /// <summary>
    /// Marks the ID index as invalid, requiring a rebuild on next access.
    /// This should be called whenever the document structure changes.
    /// </summary>
    private void InvalidateIdIndex() {
        _idIndexValid = false;
    }

    /// <summary>
    /// Marks the tag index as invalid, requiring a rebuild on next access.
    /// This should be called whenever the document structure changes.
    /// </summary>
    private void InvalidateTagIndex() {
        _tagIndexValid = false;
    }

    /// <summary>
    /// Ensures the ID index is current with the document structure.
    /// Rebuilds the index if it has been invalidated.
    /// </summary>
    private void EnsureIdIndexValid() {
        if (!_idIndexValid) {
            BuildIdIndex();
        }
    }

    /// <summary>
    /// Ensures the tag index is current with the document structure.
    /// Rebuilds the index if it has been invalidated.
    /// </summary>
    private void EnsureTagIndexValid() {
        if (!_tagIndexValid) {
            BuildTagIndex();
        }
    }

    /// <summary>
    /// Adds an element to the ID index. Used when adding elements with IDs.
    /// </summary>
    /// <param name="element">The element to add to the index.</param>
    internal void RegisterElementId(Element element) {
        if (!string.IsNullOrEmpty(element.Id)) {
            EnsureIdIndexValid();
            _idIndex[element.Id] = element;
        }
    }

    /// <summary>
    /// Removes an element from the ID index. Used when removing elements or changing IDs.
    /// </summary>
    /// <param name="id">The ID to remove from the index.</param>
    internal void UnregisterElementId(string id) {
        if (!string.IsNullOrEmpty(id)) {
            _idIndex.Remove(id);
        }
    }

    /// <summary>
    /// Finds an element by its ID within the document using fast O(1) lookup.
    /// </summary>
    /// <param name="id">The ID of the element to find.</param>
    /// <returns>The first element that matches the given ID; otherwise, null.</returns>
    public Element? GetElementById(string id) {
        if (string.IsNullOrEmpty(id)) {
            return null;
        }

        EnsureIdIndexValid();
        return _idIndex.TryGetValue(id, out Element? element) ? element : null;
    }

    /// <summary>
    /// Finds an element by its ID using the original tree traversal method.
    /// This method is provided for compatibility and verification purposes.
    /// Generally, GetElementById should be used instead for better performance.
    /// </summary>
    /// <param name="id">The ID of the element to find.</param>
    /// <returns>The first element that matches the given ID; otherwise, null.</returns>
    public Element? FindElementById(string id) {
        if (string.IsNullOrEmpty(id)) {
            return null;
        }
        return Root.FindElementById(id);
    }

    /// <summary>
    /// Gets all elements in the document that have IDs assigned.
    /// </summary>
    /// <returns>A read-only collection of all elements with IDs.</returns>
    public IReadOnlyDictionary<string, Element> GetAllElementsWithIds() {
        EnsureIdIndexValid();
        return _idIndex;
    }

    /// <summary>
    /// Checks if an element with the specified ID exists in the document.
    /// </summary>
    /// <param name="id">The ID to check for.</param>
    /// <returns>True if an element with the specified ID exists; otherwise, false.</returns>
    public bool ContainsElementId(string id) {
        if (string.IsNullOrEmpty(id)) {
            return false;
        }

        EnsureIdIndexValid();
        return _idIndex.ContainsKey(id);
    }

    /// <summary>
    /// Gets all elements in the document that have the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>A read-only collection of elements with the specified tag.</returns>
    public IReadOnlyCollection<Element> GetElementsByTag(string tag) {
        if (string.IsNullOrEmpty(tag)) {
            return Array.Empty<Element>();
        }

        EnsureTagIndexValid();
        return _tagIndex.TryGetValue(tag, out var elements) ? elements : Array.Empty<Element>();
    }

    /// <summary>
    /// Gets all elements in the document that have any of the specified tags.
    /// </summary>
    /// <param name="tags">The tags to search for.</param>
    /// <returns>A read-only collection of elements that have at least one of the specified tags.</returns>
    public IReadOnlyCollection<Element> GetElementsByAnyTag(params string[] tags) {
        if (tags == null || tags.Length == 0) {
            return Array.Empty<Element>();
        }

        EnsureTagIndexValid();
        var result = new HashSet<Element>();

        foreach (var tag in tags) {
            if (!string.IsNullOrEmpty(tag) && _tagIndex.TryGetValue(tag, out var elements)) {
                result.UnionWith(elements);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all elements in the document that have all of the specified tags.
    /// </summary>
    /// <param name="tags">The tags that elements must have.</param>
    /// <returns>A read-only collection of elements that have all of the specified tags.</returns>
    public IReadOnlyCollection<Element> GetElementsByAllTags(params string[] tags) {
        if (tags == null || tags.Length == 0) {
            return Array.Empty<Element>();
        }

        EnsureTagIndexValid();
        HashSet<Element>? result = null;

        foreach (var tag in tags) {
            if (string.IsNullOrEmpty(tag)) {
                continue;
            }

            if (!_tagIndex.TryGetValue(tag, out var elements)) {
                // If any tag is not found, no elements can have all tags
                return Array.Empty<Element>();
            }

            if (result == null) {
                result = new HashSet<Element>(elements);
            } else {
                result.IntersectWith(elements);
            }

            // Early exit if no elements remain
            if (result.Count == 0) {
                return Array.Empty<Element>();
            }
        }

        return result ?? (IReadOnlyCollection<Element>)Array.Empty<Element>();
    }

    /// <summary>
    /// Gets all unique tags used in the document.
    /// </summary>
    /// <returns>A read-only collection of all tags in the document.</returns>
    public IReadOnlyCollection<string> GetAllTags() {
        EnsureTagIndexValid();
        return _tagIndex.Keys;
    }

    /// <summary>
    /// Checks if any elements in the document have the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if any elements have the specified tag; otherwise, false.</returns>
    public bool ContainsTag(string tag) {
        if (string.IsNullOrEmpty(tag)) {
            return false;
        }

        EnsureTagIndexValid();
        return _tagIndex.ContainsKey(tag);
    }

    /// <summary>
    /// Gets the count of elements that have the specified tag.
    /// </summary>
    /// <param name="tag">The tag to count elements for.</param>
    /// <returns>The number of elements with the specified tag.</returns>
    public int GetTagElementCount(string tag) {
        if (string.IsNullOrEmpty(tag)) {
            return 0;
        }

        EnsureTagIndexValid();
        return _tagIndex.TryGetValue(tag, out var elements) ? elements.Count : 0;
    }

    /// <summary>
    /// Gets all elements of the specified type in the document.
    /// </summary>
    /// <typeparam name="T">The element type to search for.</typeparam>
    /// <returns>A collection of elements of the specified type.</returns>
    public IReadOnlyCollection<T> GetElementsByType<T>() where T : Element {
        return Root.FindElementsByType<T>();
    }

    /// <summary>
    /// Gets all elements of the specified type in the document.
    /// </summary>
    /// <param name="elementType">The element type to search for.</param>
    /// <returns>A collection of elements of the specified type.</returns>
    public IReadOnlyCollection<Element> GetElementsByType(Type elementType) {
        return Root.FindElementsByType(elementType);
    }

    /// <summary>
    /// Converts the document to a XferLang string representation without formatting.
    /// </summary>
    /// <returns>A compact XferLang string representation of the document.</returns>
    public virtual string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the document to a XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="indentChar">Character to use for indentation (default: space).</param>
    /// <param name="indentation">Number of indent characters per level (default: 2).</param>
    /// <param name="depth">Starting depth level for indentation (default: 0).</param>
    /// <returns>A formatted XferLang string representation of the document.</returns>
    public virtual string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        // Serialize document-level PIs first
        foreach (var pi in ProcessingInstructions) {
            sb.Append(pi.ToXfer(formatting, indentChar, indentation, depth));
            if (formatting.HasFlag(Formatting.Indented)) {
                sb.AppendLine();
            }
        }

        // Then serialize the root element
        sb.Append(Root.ToXfer(formatting, indentChar, indentation, depth));

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the document using default formatting.
    /// </summary>
    /// <returns>A XferLang string representation of the document.</returns>
    public override string ToString() {
        return ToXfer();
    }

    /// <summary>
    /// Converts the document to a UTF-8 encoded byte array.
    /// </summary>
    /// <returns>A byte array containing the UTF-8 encoded XferLang representation.</returns>
    public byte[] ToByteArray() {
        var stringRepresentation = ToString();
        // Use UTF-8 by default
        return Encoding.UTF8.GetBytes(stringRepresentation);
    }
}
