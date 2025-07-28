using System.Text;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang;


public class XferDocument {
    public Configuration.XferDocumentContext Context { get; set; } = new Configuration.XferDocumentContext();
    public TupleElement Root { get; set; } = new();
    public XferDocumentMetadata? Metadata { get; set; }

    public XferDocument() { }

    public XferDocument(TupleElement root) {
        Root = root;
    }

    public void Add(KeyValuePairElement value) {
        Root.Add(value);
    }


    public virtual string ToXfer() {
        return Root.ToXfer();
    }

    public virtual string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        return Root.ToXfer(formatting, indentChar, indentation, depth);
    }

    public override string ToString() {
        return ToXfer();
    }

    public byte[] ToByteArray() {
        var stringRepresentation = ToString();
        // Use UTF-8 by default
        return Encoding.UTF8.GetBytes(stringRepresentation);
    }
}
