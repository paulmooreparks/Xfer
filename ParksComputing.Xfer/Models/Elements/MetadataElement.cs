using System.Text;

using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xfer.Models.Elements;

public class MetadataElement : Element {
    public static readonly string ElementName = "metadata";
    public const char OpeningSpecifier = '!';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);
    public static readonly string DefaultVersion = Parser.Version;

    public static readonly string VersionKeyword = "version";
    public static readonly string MessageIdKeyword = "message_id";
    public static readonly string TtlKeyword = "ttl";

    private static readonly SortedSet<string> Keywords = new () {
        MessageIdKeyword,
        VersionKeyword,
        TtlKeyword
    };

    private Dictionary<string, KeyValuePairElement> _values = new ();
    public IReadOnlyDictionary<string, KeyValuePairElement> Values => _values;

    public KeyValuePairElement this[string index] {
        get {
            return _values[index];
        }
        set {
            if (string.Equals(index, VersionKeyword)) {
                Version = CastOrThrow<TextElement>(value, index).Value ?? string.Empty;
                return;
            }

            if (string.Equals(index, MessageIdKeyword)) {
                MessageId = CastOrThrow<TextElement>(value, index).Value ?? string.Empty;
                return;
            }

            if (string.Equals(index, TtlKeyword)) {
                Ttl = CastOrThrow<IntegerElement>(value, index).Value;
                return;
            }

            SetOrUpdateValue(index, value);
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

    private string _version = string.Empty;

    public string Version {
        get {
            return _version;
        }
        set {
            _version = value;
            SetOrUpdateValue(VersionKeyword, new StringElement(value));
        }
    }

    private string _message_id = string.Empty;

    public string MessageId {
        get {
            return _message_id;
        }
        set {
            _message_id = value;
            SetOrUpdateValue(MessageIdKeyword, new StringElement(value));
        }
    }

    private int _ttl = 0;

    public int Ttl {
        get {
            return _ttl;
        }
        set {
            _ttl = value;
            SetOrUpdateValue(TtlKeyword, new IntegerElement(value));
        }
    }

    public MetadataElement(ElementStyle elementStyle = ElementStyle.Normal) : this(DefaultVersion, elementStyle) {
    }

    public MetadataElement(string version, ElementStyle elementStyle = ElementStyle.Normal) 
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, elementStyle)) 
    {
        Version = version;
    }

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
                keyElement = new KeywordElement(key, style: ElementStyle.Bare);
            }
            else {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new KeyValuePairElement(keyElement, element));
        }
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        this[value.Key] = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var value in _values.Values) {
            sb.Append(value);
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
