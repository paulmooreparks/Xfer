using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for document-level metadata and configuration in XferLang.
/// Contains document properties, schema information, versioning, and other metadata
/// that applies to the entire XferLang document. The instruction expects an object
/// containing document configuration parameters.
/// </summary>
public class DocumentProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "document";
    public Dictionary<string, int> CustomCharIds { get; } = new Dictionary<string, int>();
    public DocumentProcessingInstruction(ObjectElement value) : base(value, Keyword) { }
    public DocumentProcessingInstruction() : base(new ObjectElement(), Keyword) { }
}
