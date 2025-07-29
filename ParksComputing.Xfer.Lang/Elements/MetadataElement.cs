using System.Text;
using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Elements;

public class MetadataElement : Element {
    /// <summary>
    /// The element this metadata is associated with (e.g., PI target).
    /// </summary>
    public Element? AnnotatedElement { get; set; }
    public static readonly string ElementName = "metadata";
    public const char OpeningSpecifier = '!';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);
    public static readonly string DefaultVersion = Parser.Version;

    public static readonly string XferKeyword = "xfer";
    public static readonly string SchemaKeyword = "schema";

    private static readonly SortedSet<string> Keywords = [
        XferKeyword
    ];

    private Dictionary<string, KeyValuePairElement> _values = [];
    public IReadOnlyDictionary<string, KeyValuePairElement> Values => _values;

    public KeyValuePairElement this[string index] {
        get {
            return _values[index];
        }
        set {
            SetElement(index, value.Value);
        }
    }

    private TElement CastOrThrow<TElement>(KeyValuePairElement value, string key) where TElement : Element {
        if (value.Value is TElement t) {
            return t;
        }
        throw new ArgumentException($"Invalid element type for '{key}': {value.GetType()}");
    }

    // We may do more with this in the future....
    public string Encoding => "UTF-8";

    private string _xfer = string.Empty;

    private string _message_id = string.Empty;

    public MetadataElement(ElementStyle elementStyle = ElementStyle.Explicit)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, elementStyle)) {
    }

    private bool IsKeyword(string compare, out string? keyword) {
        return Keywords.TryGetValue(compare, out keyword);
    }

    private void SetElement<TElement>(string key, TElement element) where TElement : Element {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp)) {
            var newKvp = new KeyValuePairElement(kvp.KeyElement, element);
            _values[key] = newKvp;
            // Replace in Children
            int idx = Children.FindIndex(e => e is KeyValuePairElement k && k.Key == key);
            if (idx >= 0) {
                Children[idx] = newKvp;
            } else {
                Children.Add(newKvp);
            }
        }
        else {
            TextElement keyElement;
            if (key.IsKeywordString()) {
                keyElement = new IdentifierElement(key, style: ElementStyle.Implicit);
            }
            else {
                keyElement = new StringElement(key);
            }
            var newKvp = new KeyValuePairElement(keyElement, element);
            _values.Add(key, newKvp);
            Children.Add(newKvp);
        }
    }

    public bool ContainsKey(string key) {
        return _values.ContainsKey(key);
    }

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

    public bool Add(KeyValuePairElement value) {
        if (_values.ContainsKey(value.Key)) {
            if (string.Equals(value.Key, XferKeyword)) {
                AddOrUpdate(value);
                return true;
            }
            return false;
        }
        _values.Add(value.Key, value);
        Children.Add(value);
        return true;
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        this[value.Key] = value;
    }

    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

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

        for (var i = 0; i < Children.Count; ++i) {
            var value = Children[i];
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(value.ToXfer(formatting, indentChar, indentation, depth + 1));
            if (!isIndented && value is KeyValuePairElement kvp && (kvp.Delimiter.Style == ElementStyle.Implicit || kvp.Delimiter.Style == ElementStyle.Compact) && i + 1 < Children.Count) {
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

    public override string ToString() {
        return ToXfer();
    }
}
