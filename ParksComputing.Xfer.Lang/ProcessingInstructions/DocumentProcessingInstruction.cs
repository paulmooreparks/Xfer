using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;
public class DocumentProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "document";
    public Dictionary<string, int> CustomCharIds { get; } = new Dictionary<string, int>();
    public DocumentProcessingInstruction(ObjectElement value) : base(value, Keyword) { }
    public DocumentProcessingInstruction() : base(new ObjectElement(), Keyword) { }
}
