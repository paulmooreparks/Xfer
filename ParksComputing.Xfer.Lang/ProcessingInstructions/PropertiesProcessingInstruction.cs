using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

public class PropertiesProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "properties";
    public Dictionary<string, string> CustomProperties { get; } = new();
    public PropertiesProcessingInstruction(ObjectElement value) : base(value, Keyword) { }
}
