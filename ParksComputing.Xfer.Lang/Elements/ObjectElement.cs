using System.Text;
using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Elements;

public class ObjectElement : DictionaryElement
{
    public static readonly string ElementName = "object";
    public const char OpeningSpecifier = '{';
    public const char ClosingSpecifier = '}';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    public IReadOnlyDictionary<string, KeyValuePairElement> Values => _values;

    public Element this[string index]
    {
        get
        {
            return _values[index].Value;
        }
        set
        {
            SetElement(index, value);
        }
    }

    public ObjectElement() : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact)) { }

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
                keyElement = new IdentifierElement(key, style: ElementStyle.Implicit);
            }
            else
            {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new KeyValuePairElement(keyElement, element));
        }
    }

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public bool TryGetElement<TElement>(string key, out TElement? result) where TElement : Element
    {
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp))
        {
            if (kvp.Value is TElement element)
            {
                result = element;
                return true;
            }
            else if (kvp.Value is IConvertible convertible && typeof(TElement).IsAssignableFrom(convertible.GetType()))
            {
                result = (TElement)Convert.ChangeType(convertible, typeof(TElement));
                return true;
            }
        }

        result = default;
        return false;
    }

    public Element GetElement(string key)
    {
        return _values[key].Value;
    }

    public bool Remove(string key)
    {
        return _values.Remove(key);
    }

    public void AddOrUpdate(KeyValuePairElement value)
    {
        if (_values.TryGetValue(value.Key, out KeyValuePairElement? tuple)) {
            _values[value.Key] = value;
        }
        else {
            _values.Add(value.Key, value);
        }
    }

    public List<KeyValuePairElement> TypedValue
    {
        get
        {
            List<KeyValuePairElement> values = new();
            foreach (var value in _values)
            {
                values.Append(value.Value);
            }
            return values;
        }
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

    public override string ToString()
    {
        return ToXfer();
    }
}
