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
            _values[key] = new KeyValuePairElement(kvp.KeyElement, element);
        }
        else {
            TextElement keyElement;

            if (key.IsKeywordString()) {
                keyElement = new IdentifierElement(key, style: ElementStyle.Implicit);
            }
            else {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new KeyValuePairElement(keyElement, element));
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

    public Element GetElement(string key) {
        return _values[key].Value;
    }

    public bool Remove(string key) {
        return _values.Remove(key);
    }

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

public void AddOrUpdate(Element element) {
    switch (element) {
        case KeyValuePairElement kvp:
            AddOrUpdate(kvp);
            break;
        case ProcessingInstruction meta:
            Children.Add(meta);
            meta.Parent = this;
            break;
        default:
            throw new InvalidOperationException($"Only KeyValuePairElement and MetadataElement can be added to ObjectElement. Attempted: {element.GetType().Name}");
    }
}

    public List<KeyValuePairElement> TypedValue {
        get {
            List<KeyValuePairElement> values = [];
            foreach (var value in _values) {
                values.Append(value.Value);
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
                sb.Append(Delimiter.Opening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinOpening);
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
            if ((child.Delimiter.Style == ElementStyle.Implicit || child.Delimiter.Style == ElementStyle.Compact) && i < Children.Count) {
                sb.Append(' ');
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
                sb.Append(Delimiter.Closing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinClosing);
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
