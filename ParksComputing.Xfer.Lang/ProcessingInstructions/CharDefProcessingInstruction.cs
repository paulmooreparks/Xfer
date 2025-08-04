using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for defining custom character definitions in XferLang.
/// Allows mapping of custom character names to Unicode code points for use in character elements.
/// The instruction expects an object containing name-to-codepoint mappings.
/// </summary>
// Example: charDef PI
public class CharDefProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "chardef";
    public Dictionary<string, int> CustomCharIds { get; } = new Dictionary<string, int>();
    public CharDefProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    public override void ProcessingInstructionHandler() {
        var charDefRegistry = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (Value is not ObjectElement obj) {
            throw new InvalidOperationException($"{Keyword} processing instruction expects an object element. Found: {Value.GetType().Name}");
        }

        foreach (var kv in obj.Dictionary ) {
            var name = kv.Value.Key;

            if (kv.Value.Value is CharacterElement charElem) {
                charDefRegistry[name] = charElem.Value;
            }
            else {
                throw new InvalidOperationException($"{Keyword} processing instruction expects a character element for key '{name}'. Found: {kv.Value.GetType().Name}");
            }
        }

        CharacterIdRegistry.SetCustomIds(charDefRegistry);
    }

    public override void ElementHandler(Element element) {
        element.Id = Value.ToString();
    }
}
