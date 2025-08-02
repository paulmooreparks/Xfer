using System.Text;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang;


public class XferDocument {
    public CollectionElement Root { get; set; } = new TupleElement();

    public XferMetadata? Metadata { get; set; }

    public XferDocument() { }

    public XferDocument(CollectionElement root) {
        Root = root;
    }

    public void Add(Element value) {
        Root.Add(value);
    }

    /// <summary>
    /// Finds an element by its ID within the document.
    /// </summary>
    /// <param name="id">The ID of the element to find.</param>
    /// <returns>The first element that matches the given ID; otherwise, null.</returns>
    public Element? GetElementById(string id) {
        if (string.IsNullOrEmpty(id)) {
            return null;
        }
        return Root.FindElementById(id);
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
