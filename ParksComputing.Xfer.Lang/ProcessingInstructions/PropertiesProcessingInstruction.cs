using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for defining custom properties in XferLang documents.
/// Allows specification of additional property-value pairs that can be used for
/// application-specific configuration, validation rules, or other custom behaviors.
/// </summary>
public class PropertiesProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword used to identify properties processing instructions.
    /// </summary>
    public const string Keyword = "properties";

    /// <summary>
    /// Gets a dictionary of custom properties defined in this processing instruction.
    /// Maps property names to their corresponding string values.
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; } = new();

    /// <summary>
    /// Initializes a new instance of the PropertiesProcessingInstruction class with the specified properties object.
    /// </summary>
    /// <param name="value">The object element containing custom property definitions.</param>
    public PropertiesProcessingInstruction(ObjectElement value) : base(value, Keyword) { }
}
