using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for document-level metadata and configuration in XferLang.
/// Contains document properties, schema information, versioning, and other metadata
/// that applies to the entire XferLang document. The instruction expects an object
/// containing document configuration parameters.
/// </summary>
public class DocumentProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword used to identify document processing instructions.
    /// </summary>
    public const string Keyword = "document";

    /// <summary>
    /// Gets a dictionary of custom character IDs defined for this document.
    /// Maps custom character names to their corresponding Unicode code points.
    /// </summary>
    public Dictionary<string, int> CustomCharIds { get; } = new Dictionary<string, int>();

    /// <summary>
    /// Initializes a new instance of the DocumentProcessingInstruction class with the specified object value.
    /// </summary>
    /// <param name="value">The object element containing document configuration parameters.</param>
    public DocumentProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    /// <summary>
    /// Initializes a new instance of the DocumentProcessingInstruction class with an empty object value.
    /// </summary>
    public DocumentProcessingInstruction() : base(new ObjectElement(), Keyword) { }
}
