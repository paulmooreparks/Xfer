using System.Text;

using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xfer.Models.Elements;

public class MetadataElement : Element {
    public static readonly string ElementName = "metadata";
    public const char OpeningMarker = '!';
    public const char ClosingMarker = OpeningMarker;
    public static readonly string DefaultVersion = "1.0.0";

    private Dictionary<string, KeyValuePairElement> _values = new ();
    public IReadOnlyDictionary<string, KeyValuePairElement> Values => _values;

    public KeyValuePairElement this[string index] {
        get {
            return _values[index];
        }
        set {
            switch (index) {
                case "version":
                    SetOrThrow<TextElement>(value, index);
                    Version = value.Value.ToString() ?? string.Empty;
                    break;

                case "message_id":
                    SetOrThrow<TextElement>(value, index);
                    MessageId = value.Value.ToString() ?? string.Empty;
                    break;

                case "ttl":
                    SetOrThrow<IntegerElement>(value, index);
                    Ttl = Convert.ToInt32(value.Value);
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
        if (_values.TryGetValue(key, out KeyValuePairElement? kvp)) {
            _values[key] = new KeyValuePairElement(kvp.KeyElement, element);
        }
        else {
            TextElement keyElement;

            if (key.IsKeywordString()) {
                keyElement = new KeywordElement(key);
            }
            else {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new KeyValuePairElement(keyElement, element));
        }
    }

    private void SetOrThrow<TElement>(KeyValuePairElement value, string key) where TElement : Element {
        if (value.Value is not TElement) {
            throw new ArgumentException($"Invalid element type for '{key}': {value.GetType()}");
        }
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        this[value.Key] = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var value in _values.Values) {
            sb.Append(value.ToString());
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
