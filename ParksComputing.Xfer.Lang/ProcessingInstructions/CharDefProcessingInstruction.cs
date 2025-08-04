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
    /// <summary>
    /// The keyword used to identify character definition processing instructions.
    /// </summary>
    public const string Keyword = "chardef";

    /// <summary>
    /// Gets a dictionary of custom character IDs defined by this processing instruction.
    /// Maps custom character names to their corresponding Unicode code points.
    /// </summary>
    public Dictionary<string, int> CustomCharIds { get; } = new Dictionary<string, int>();

    /// <summary>
    /// Initializes a new instance of the CharDefProcessingInstruction class with the specified character definitions.
    /// </summary>
    /// <param name="value">The object element containing character name-to-codepoint mappings.</param>
    public CharDefProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    /// <summary>
    /// Handles the processing of character definitions by registering the custom character mappings globally.
    /// Validates that all values are character elements and updates the CharacterIdRegistry.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value is not an ObjectElement or contains invalid character definitions.</exception>
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

    /// <summary>
    /// Handles element processing by assigning the processing instruction's value as the element's ID.
    /// This provides element identification functionality for character definition contexts.
    /// </summary>
    /// <param name="element">The element to assign the ID to.</param>
    public override void ElementHandler(Element element) {
        element.Id = Value.ToString();
    }
}
