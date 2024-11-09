using System.Text;

using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xfer.Models.Elements;

public class MetadataElement : Element {
    public static readonly string ElementName = "metadata";
    public const char OpeningMarker = '!';
    public const char ClosingMarker = OpeningMarker;
    public static readonly string DefaultVersion = "1.0.0";

    private Dictionary<string, Tuple<Element, Element>> _values = new ();
    public IReadOnlyDictionary<string, Tuple<Element, Element>> Values => _values;

    public Element this[string index] {
        get {
            return _values[index].Item2;
        }
        set {
            switch (index) {
                case "version":
                    SetOrThrow<StringElement>(value, index);
                    Version = ((StringElement)value).Value;
                    break;

                case "message_id":
                    SetOrThrow<StringElement>(value, index);
                    MessageId = ((StringElement)value).Value;
                    break;

                case "ttl":
                    SetOrThrow<IntegerElement>(value, index);
                    Ttl = ((IntegerElement)value).TypedValue;
                    break;

                default:
                    SetOrUpdateValue(index, value);
                    break;
            }
        }
    }

    // We may do more with this in the future....
    public string Encoding => "UTF-8";

    private string _version = string.Empty;

    public string Version {
        get {
            return _version;
        }
        set {
            _version = value;
            SetOrUpdateValue("version", new StringElement(value));
        }
    }

    private string _message_id = string.Empty;

    public string MessageId {
        get {
            return _message_id;
        }
        set {
            _message_id = value;
            SetOrUpdateValue("message_id", new StringElement(value));
        }
    }

    private int _ttl = 0;

    public int Ttl {
        get {
            return _ttl;
        }
        set {
            _ttl = value;
            SetOrUpdateValue("ttl", new IntegerElement(value));
        }
    }

    public MetadataElement() : this(DefaultVersion) {
    }

    public MetadataElement(string version) : base(ElementName, new(OpeningMarker, ClosingMarker)) {
        Version = version;
    }

    private static readonly SortedSet<string> Keywords = new () {
        "message_id",
        "version",
        "ttl"
    };

    private bool IsKeyword(string compare, out string? keyword) {
        return Keywords.TryGetValue(compare, out keyword);
    }

    private void SetOrUpdateValue<TElement>(string key, TElement element) where TElement : Element {
        if (_values.TryGetValue(key, out Tuple<Element, Element>? tuple)) {
            _values[key] = new Tuple<Element, Element>(tuple.Item1, element);
        }
        else {
            Element keyElement;

            if (key.IsKeywordString()) {
                keyElement = new KeywordElement(key);
            }
            else {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new Tuple<Element, Element>(keyElement, element));
        }
    }

    private void SetOrThrow<TElement>(Element value, string key) where TElement : Element {
        if (value is not TElement) {
            throw new ArgumentException($"Invalid element type for '{key}': {value.GetType()}");
        }
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        if (_values.TryGetValue(value.Key, out Tuple<Element, Element>? tuple)) {
            _values[value.Key] = new Tuple<Element, Element>(value.KeyElement, value.TypedValue);
        }
        else {
            _values.Add(value.Key, new Tuple<Element, Element>(value.KeyElement, value.TypedValue));
        }
    }

    public override string Value {
        get {
            var sb = new StringBuilder();
            foreach (var value in _values.Values) {
                sb.Append($"{Parser.ElementOpeningMarker}{KeyValuePairElement.OpeningMarker}{value.Item1}{value.Item2}{KeyValuePairElement.ClosingMarker}{Parser.ElementClosingMarker}");
            }
            return sb.ToString();
        }
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
