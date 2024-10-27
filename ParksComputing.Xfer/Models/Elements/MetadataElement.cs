using System.Text;

namespace ParksComputing.Xfer.Models.Elements;

public class MetadataElement : Element {
    private Dictionary<string, Element> _values = new ();
    public IReadOnlyDictionary<string, Element> Values => _values;

    public Element this[string index] {
        get {
            return _values[index];
        }
        set {
            if (_values.ContainsKey(index)) {
                _values[index] = value;
            }
            else {
                _values.Add(index, value);
            }

            if (index == "encoding") {
                if (value is StringElement element) {
                    Encoding = element.Value;
                    return;
                }

                throw new ArgumentException($"Invalid element type for '{index}': {value.GetType()}");
            }

            if (index == "version") {
                if (value is StringElement element) {
                    Version = element.Value;
                    return;
                }

                throw new ArgumentException($"Invalid element type for '{index}': {value.GetType()}");
            }
        }
    }

    public const char OpeningMarker = '@';
    public const char ClosingMarker = OpeningMarker;

    private string _encoding = "UTF-8";

    public string Encoding {
        get {
            return _encoding;
        }
        set {
            _encoding = value;
            var index = "encoding";

            if (_values.ContainsKey(index)) {
                _values[index] = new StringElement(value);
            }
            else {
                _values.Add(index, new StringElement(value));
            }
        }
    }

    private string _version = string.Empty;

    public string Version {
        get {
            return _version;
        }
        set {
            _version = value;
            var index = "version";

            if (_values.ContainsKey(index)) {
                _values[index] = new StringElement(value);
            }
            else {
                _values.Add(index, new StringElement(value));
            }
        }
    }

    public MetadataElement() : this("UTF-8", string.Empty) {
    }

    public MetadataElement(string encoding) : this(encoding, string.Empty) {
    }

    public MetadataElement(string encoding, string version) : base("metadata", new(OpeningMarker, ClosingMarker)) {
        Encoding = encoding;
        Version = version;
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        this[value.Key.Value] = value.Value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var kvp in _values) {
            sb.Append($"{Services.Parser.ElementOpeningMarker}{StringElement.OpeningMarker}{kvp.Key}{StringElement.ClosingMarker}{Services.Parser.ElementClosingMarker}{kvp.Value}");
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
