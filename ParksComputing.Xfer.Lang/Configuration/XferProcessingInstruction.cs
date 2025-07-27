using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Configuration
{
    /// <summary>
    /// Represents a processing instruction in an XferLang document.
    /// </summary>
    public class XferProcessingInstruction
    {
        /// <summary>
        /// The type or name of the processing instruction (e.g., "processor", "id", "culture").
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Parameters for the processing instruction, as parsed from the document.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// Reference to the element this PI applies to (if any).
        /// </summary>
        public Elements.Element? AppliesTo { get; set; }

        /// <summary>
        /// Optional: position of the PI in the document.
        /// </summary>
        public int DocumentIndex { get; set; }
    }
}
