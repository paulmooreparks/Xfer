using System.Collections.Generic;
using System.Text;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Represents a complete XferLang document with its root element, metadata, processing instructions,
/// and any parsing errors or warnings. This is the primary object model for working with XferLang content.
/// </summary>
public class XferDocument {
    /// <summary>
    /// Gets or sets the root element of the document. Defaults to an empty tuple element.
    /// All document content is contained within this root element.
    /// </summary>
    public CollectionElement Root { get; set; } = new TupleElement();

    /// <summary>
    /// Gets or sets the document metadata, if specified via document processing instructions.
    /// Contains information about the XferLang version, document version, and custom metadata.
    /// </summary>
    public XferMetadata? Metadata { get; set; }

    /// <summary>
    /// Document-level Processing Instructions that appear outside the root element.
    /// These are conceptually siblings to the root element.
    /// </summary>
    public List<ProcessingInstruction> ProcessingInstructions { get; set; } = new List<ProcessingInstruction>();

    /// <summary>
    /// The first fatal error encountered during parsing, if any.
    /// When an error is present, parsing stops and the document may be incomplete.
    /// </summary>
    public ParseError? Error { get; set; }

    /// <summary>
    /// Collection of non-fatal warnings encountered during parsing.
    /// Warnings indicate potential issues but don't prevent successful parsing.
    /// </summary>
    public List<ParseWarning> Warnings { get; set; } = new List<ParseWarning>();

    /// <summary>
    /// Gets a value indicating whether the document has a fatal error.
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// Gets a value indicating whether the document has any warnings.
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the document is valid (no fatal errors).
    /// Note: A document can be valid but still have warnings.
    /// </summary>
    public bool IsValid => !HasError;

    /// <summary>
    /// Initializes a new XferDocument with an empty root collection.
    /// </summary>
    public XferDocument() { }

    /// <summary>
    /// Initializes a new XferDocument with the specified root collection.
    /// </summary>
    /// <param name="root">The root collection element for the document.</param>
    public XferDocument(CollectionElement root) {
        Root = root;
    }

    /// <summary>
    /// Adds an element to the document's root collection.
    /// </summary>
    /// <param name="value">The element to add to the document.</param>
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

    /// <summary>
    /// Converts the document to a XferLang string representation without formatting.
    /// </summary>
    /// <returns>A compact XferLang string representation of the document.</returns>
    public virtual string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the document to a XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="indentChar">Character to use for indentation (default: space).</param>
    /// <param name="indentation">Number of indent characters per level (default: 2).</param>
    /// <param name="depth">Starting depth level for indentation (default: 0).</param>
    /// <returns>A formatted XferLang string representation of the document.</returns>
    public virtual string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        // Serialize document-level PIs first
        foreach (var pi in ProcessingInstructions) {
            sb.Append(pi.ToXfer(formatting, indentChar, indentation, depth));
            if (formatting.HasFlag(Formatting.Indented)) {
                sb.AppendLine();
            }
        }

        // Then serialize the root element
        sb.Append(Root.ToXfer(formatting, indentChar, indentation, depth));

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the document using default formatting.
    /// </summary>
    /// <returns>A XferLang string representation of the document.</returns>
    public override string ToString() {
        return ToXfer();
    }

    /// <summary>
    /// Converts the document to a UTF-8 encoded byte array.
    /// </summary>
    /// <returns>A byte array containing the UTF-8 encoded XferLang representation.</returns>
    public byte[] ToByteArray() {
        var stringRepresentation = ToString();
        // Use UTF-8 by default
        return Encoding.UTF8.GetBytes(stringRepresentation);
    }
}
