using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions {
    // Example: meta PI
    public class MetaProcessingInstruction : ProcessingInstruction {
        public const string Keyword = "meta";
        public new XferMetadata Metadata { get; set; } = new XferMetadata();
        public MetaProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    }
}
