using System.Text;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang;

public class XferDocument {
    public Configuration.XferDocumentContext Context { get; set; } = new Configuration.XferDocumentContext();
    public List<Element> MetadataCollection { get; set; } = new();
    public TupleElement Root { get; set; } = new();

    public XferDocument() { }

    public XferDocument(List<Element> metadataCollection, TupleElement root) {
        MetadataCollection = metadataCollection;
        Root = root;
    }

    public XferDocument(TupleElement root) {
        Root = root;
    }

    public void Add(KeyValuePairElement value) {
        Root.Add(value);
    }

    public virtual string ToXfer() {
        return ToXfer(Formatting.None);
    }

    public virtual string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        bool isIndented = (formatting & Formatting.Indented) == Formatting.Indented;
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        string rootIndent = string.Empty;
        string nestIndent = string.Empty;

        var sb = new StringBuilder();
        foreach (var meta in MetadataCollection) {
            sb.Append(meta.ToXfer(formatting, indentChar, indentation, depth));
            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
        }

        int i = 0;
        foreach (var value in Root.Values) {
            ++i;
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(value.ToXfer(formatting, indentChar, indentation, depth));
            if ((value.Delimiter.Style == ElementStyle.Implicit || value.Delimiter.Style == ElementStyle.Compact) && i < Root.Values.Count()) {
                sb.Append(' ');
            }
            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
        }

        return sb.ToString();
    }

    public override string ToString() {
        return ToXfer();
    }

    public byte[] ToByteArray() {
        var stringRepresentation = ToString();
        // Use UTF-8 by default, or try to get encoding from first metadata element if available
        string encodingName = "UTF-8";
        if (MetadataCollection.Count > 0 && MetadataCollection[0] is MetadataElement meta && !string.IsNullOrEmpty(meta.Encoding)) {
            encodingName = meta.Encoding;
        }
        return encodingName switch {
            "UTF-8" => Encoding.UTF8.GetBytes(stringRepresentation),
            "UTF-16" => Encoding.Unicode.GetBytes(stringRepresentation),
            "UTF-32" => Encoding.UTF32.GetBytes(stringRepresentation),
            "Unicode" => Encoding.Unicode.GetBytes(stringRepresentation),
            "ASCII" => Encoding.ASCII.GetBytes(stringRepresentation),
            _ => throw new NotSupportedException($"Encoding '{encodingName}' is not supported.")
        };
    }
}
