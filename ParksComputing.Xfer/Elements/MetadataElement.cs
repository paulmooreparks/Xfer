using System.Text;
using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xfer.Elements;

public class MetadataElement : Element
{
    public static readonly string ElementName = "metadata";
    public const char OpeningSpecifier = '!';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);
    public static readonly string DefaultVersion = Parser.Version;

    public static readonly string XferKeyword = "xfer";

    private static readonly SortedSet<string> Keywords = new() {
        XferKeyword
    };

    private Dictionary<string, KeyValuePairElement> _values = new();
    public IReadOnlyDictionary<string, KeyValuePairElement> Values => _values;

    public KeyValuePairElement this[string index]
    {
        get
        {
            return _values[index];
        }
        set
        {
            if (string.Equals(index, XferKeyword))
            {
                Xfer = CastOrThrow<TextElement>(value, index).Value ?? string.Empty;
                return;
            }

            SetElement(index, value.Value);
        }
    }

    private TElement CastOrThrow<TElement>(KeyValuePairElement value, string key) where TElement : Element
    {
        if (value.Value is TElement t)
        {
            return t;
        }
        throw new ArgumentException($"Invalid element type for '{key}': {value.GetType()}");
    }

    // We may do more with this in the future....
    public string Encoding => "UTF-8";

    private string _xfer = string.Empty;

    public string Xfer
    {
        get
        {
            return _xfer;
        }
        set
        {
            _xfer = value;
            SetElement(XferKeyword, new StringElement(value));
        }
    }

    private string _message_id = string.Empty;

    public MetadataElement(ElementStyle elementStyle = ElementStyle.Explicit) : this(DefaultVersion, elementStyle)
    {
    }

    public MetadataElement(string version, ElementStyle elementStyle = ElementStyle.Explicit)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, elementStyle))
    {
        Xfer = version;
    }

    private bool IsKeyword(string compare, out string? keyword)
    {
        return Keywords.TryGetValue(compare, out keyword);
    }

    private void SetElement<TElement>(string key, TElement element) where TElement : Element
    {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp))
        {
            _values[key] = new KeyValuePairElement(kvp.KeyElement, element);
        }
        else
        {
            TextElement keyElement;

            if (key.IsKeywordString())
            {
                keyElement = new KeywordElement(key, style: ElementStyle.Implicit);
            }
            else
            {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new KeyValuePairElement(keyElement, element));
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
                result = (TElement)Convert.ChangeType(convertible, typeof(TElement));
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
        return true;
    }

    public void AddOrUpdate(KeyValuePairElement value)
    {
        this[value.Key] = value;
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
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
        foreach (var value in _values.Values) {
            ++i;
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(value.ToXfer(formatting, indentChar, indentation, depth + 1));
            if ((value.Delimiter.Style == ElementStyle.Implicit || value.Delimiter.Style == ElementStyle.Compact) && i < _values.Values.Count()) {
                sb.Append(' ');
            }
            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
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

    public override string ToString()
    {
        return ToXfer();
    }
}
