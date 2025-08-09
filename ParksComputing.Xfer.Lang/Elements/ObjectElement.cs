using System.Text;
using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents an object element in XferLang that contains key-value pairs.
/// Objects are delimited by curly braces {} and store structured data as named properties.
/// Supports both semantic key-value pairs and metadata elements for round-trip preservation.
/// </summary>
public class ObjectElement : DictionaryElement {
    /// <summary>
    /// Stores all child elements in order, including KeyValuePairElement and MetadataElement.
    /// Only KeyValuePairElements are referenced in the dictionary; others are preserved for round-tripping.
    /// </summary>
    public static readonly string ElementName = "object";

    /// <summary>
    /// The character used to open object elements ('{').
    /// </summary>
    public const char OpeningSpecifier = '{';

    /// <summary>
    /// The character used to close object elements ('}').
    /// </summary>
    public const char ClosingSpecifier = '}';

    /// <summary>
    /// The element delimiter configuration for object elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    /// <summary>
    /// Only semantic key-value pairs (not PIs/comments)
    /// </summary>
    public IReadOnlyDictionary<string, KeyValuePairElement> Dictionary => _values;

    /// <summary>
    /// Gets or sets the element associated with the specified key.
    /// </summary>
    /// <param name="index">The key of the element to get or set.</param>
    /// <returns>The element associated with the specified key.</returns>
    public Element this[string index] {
        get {
            return _values[index].Value;
        }
        set {
            SetElement(index, value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the ObjectElement class.
    /// </summary>
    public ObjectElement() : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact)) { }

    private void SetElement<TElement>(string key, TElement element) where TElement : Element {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp)) {
            // Remove old KVP from Children collection
            Children.Remove(kvp);
            // Update with new KVP using keyword key
            var newKvp = new KeyValuePairElement(new KeywordElement(key), element);
            _values[key] = newKvp;
            // Add new KVP to Children collection
            Children.Add(newKvp);
            newKvp.Parent = this;
        }
        else {
            // Use KeywordElement for object keys - keywords are the correct key type
            var keyElement = new KeywordElement(key);
            var newKvp = new KeyValuePairElement(keyElement, element);
            _values.Add(key, newKvp);
            // Add new KVP to Children collection
            Children.Add(newKvp);
            newKvp.Parent = this;
        }
    }

    /// <summary>
    /// Determines whether the object contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the object.</param>
    /// <returns>True if the object contains an element with the key; otherwise, false.</returns>
    public bool ContainsKey(string key) {
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// Gets the element associated with the specified key, cast to the specified type.
    /// </summary>
    /// <typeparam name="TElement">The type of element to retrieve.</typeparam>
    /// <param name="key">The key of the element to get.</param>
    /// <param name="result">When this method returns, contains the element associated with the specified key, if found; otherwise, null.</param>
    /// <returns>True if the object contains an element with the key and it can be cast to the specified type; otherwise, false.</returns>
    public bool TryGetElement<TElement>(string key, out TElement? result) where TElement : Element {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp)) {
            if (kvp.Value is TElement element) {
                result = element;
                return true;
            }
            else if (kvp.Value is IConvertible convertible && typeof(TElement).IsAssignableFrom(convertible.GetType())) {
                result = (TElement) Convert.ChangeType(convertible, typeof(TElement));
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the element value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to retrieve</param>
    /// <returns>The element associated with the specified key</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found</exception>
    public Element GetElement(string key) {
        return _values[key].Value;
    }

    /// <summary>
    /// Removes the key-value pair with the specified key from the object.
    /// </summary>
    /// <param name="key">The key of the element to remove</param>
    /// <returns>True if the element was found and removed, false otherwise</returns>
    public bool Remove(string key) {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp)) {
            bool removedFromDict = _values.Remove(key);
            bool removedFromChildren = Children.Remove(kvp);

            if (removedFromDict && kvp != null) {
                kvp.Parent = null;
                // Also clear parent relationship for the KVP's value
                if (kvp.Value != null) {
                    kvp.Value.Parent = null;
                }
            }

            return removedFromDict;
        }
        return false;
    }

    /// <summary>
    /// Removes a specific child element from this object.
    /// Overrides base implementation to handle both _values dictionary and Children collection.
    /// </summary>
    /// <param name="child">The child element to remove</param>
    /// <returns>True if the child was found and removed, false otherwise</returns>
    public override bool RemoveChild(Element child) {
        if (child is KeyValuePairElement kvp) {
            return Remove(kvp.Key);
        }

        // For non-KVP children (like ProcessingInstructions), just remove from Children
        if (Children.Remove(child)) {
            child.Parent = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a child element at the specified index from the Children collection.
    /// Note: Objects are not naturally ordered, but this provides access to Children collection by index.
    /// </summary>
    /// <param name="index">The zero-based index of the child to remove</param>
    /// <returns>True if the child was successfully removed, false if index was out of range</returns>
    public override bool RemoveChildAt(int index) {
        if (index < 0 || index >= Children.Count) {
            return false;
        }

        var child = Children[index];
        return RemoveChild(child);
    }

    /// <summary>
    /// Removes all key-value pairs and children from this object.
    /// </summary>
    /// <returns>The number of children that were removed</returns>
    public override int RemoveAllChildren() {
        var count = Children.Count;

        // Clear parent relationships
        foreach (var child in Children) {
            child.Parent = null;
        }

        _values.Clear();
        Children.Clear();
        return count;
    }

    /// <summary>
    /// Replaces a child element with a new element.
    /// For KeyValuePairElements, replaces the value for the same key.
    /// </summary>
    /// <param name="oldChild">The existing child element to replace</param>
    /// <param name="newChild">The new child element to add in its place</param>
    /// <returns>True if the replacement was successful, false if oldChild was not found</returns>
    public override bool ReplaceChild(Element oldChild, Element newChild) {
        if (oldChild is KeyValuePairElement oldKvp && newChild is KeyValuePairElement newKvp) {
            // Replace KVP with same key
            if (newKvp.Key == oldKvp.Key && _values.ContainsKey(oldKvp.Key)) {
                AddOrUpdate(newKvp);
                return true;
            }
        }

        // For other types of children, use index-based replacement
        var index = Children.IndexOf(oldChild);
        if (index == -1) {
            return false;
        }

        // Clear old parent relationship
        oldChild.Parent = null;

        // Set up new relationships
        Children[index] = newChild;
        newChild.Parent = this;

        return true;
    }

/// <summary>
/// Adds a new key-value pair to the object or updates an existing one.
/// If the key already exists, the existing pair is replaced in both the dictionary and children collection.
/// </summary>
/// <param name="value">The key-value pair element to add or update</param>
public void AddOrUpdate(KeyValuePairElement value) {
    if (_values.TryGetValue(value.Key, out KeyValuePairElement? existing)) {
        _values[value.Key] = value;
        // Replace the first matching child in Children
        int idx = Children.FindIndex(e => e is KeyValuePairElement k && k.Key == value.Key);
        if (idx >= 0) {
            Children[idx] = value;
            value.Parent = this;
        } else {
            Children.Add(value);
            value.Parent = this;
        }
    }
    else {
        _values.Add(value.Key, value);
        Children.Add(value);
        value.Parent = this;
    }
}

/// <summary>
/// Adds an element to the object. Supports key-value pairs and processing instructions.
/// Key-value pairs are added to the object's dictionary, while processing instructions
/// are added only to the children collection.
/// </summary>
/// <param name="element">The element to add (must be KeyValuePairElement or ProcessingInstruction)</param>
/// <exception cref="InvalidOperationException">Thrown when the element type is not supported</exception>
public void AddOrUpdate(Element element) {
    switch (element) {
        case KeyValuePairElement kvp:
            AddOrUpdate(kvp);
            break;
        case ProcessingInstruction meta:
            // Remove from previous parent if it exists
            if (meta.Parent != null && meta.Parent != this) {
                meta.Parent.RemoveChild(meta);
            }

            if (!Children.Contains(meta)) {
                Children.Add(meta);
            }
            meta.Parent = this;
            break;
        default:
            throw new InvalidOperationException($"Only KeyValuePairElement and MetadataElement can be added to ObjectElement. Attempted: {element.GetType().Name}");
    }
}

    /// <summary>
    /// Gets a list of all key-value pair elements in this object.
    /// This provides access to all the object's properties as a typed collection.
    /// </summary>
    public List<KeyValuePairElement> TypedValue {
        get {
            List<KeyValuePairElement> values = [];
            foreach (var value in _values) {
                values.Add(value.Value);
            }
            return values;
        }
    }


    /// <summary>
    /// Converts the object element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the object element.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the object element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the object element.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        bool isIndented = (formatting & Formatting.Indented) == Formatting.Indented;
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        string rootIndent = string.Empty;
        string nestIndent = string.Empty;

        var sb = new StringBuilder();

        if (isIndented) {
            rootIndent = new string(indentChar, indentation * depth);
            nestIndent = new string(indentChar, indentation * (depth + 1));
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.ExplicitOpening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.CompactOpening);
                break;
        }

        if (isIndented) {
            sb.Append(Environment.NewLine);
        }

        int i = 0;
        foreach (var child in Children) {
            ++i;
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(child.ToXfer(formatting, indentChar, indentation, depth + 1));

            // Add space between KeyValuePair elements (but not after the last one)
            if (i < Children.Count && child is KeyValuePairElement item) {
                if (!isIndented && item.Value.Delimiter.Style is ElementStyle.Implicit || (item.Value.Delimiter.Style is ElementStyle.Compact && item.Value.Delimiter.CompactClosing == string.Empty)) {
                    sb.Append(' ');
                }
            }

            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
        }

        if (isIndented) {
            sb.Append(rootIndent);
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.ExplicitClosing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.CompactClosing);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the object element.
    /// </summary>
    /// <returns>The XferLang representation of the object element.</returns>
    public override string ToString() {
        return ToXfer();
    }
}
