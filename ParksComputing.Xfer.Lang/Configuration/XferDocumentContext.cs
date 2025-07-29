using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Configuration {
using ParksComputing.Xfer.Lang.Elements;
    /// <summary>
    /// Holds context for an XferLang document, including processing instructions and elements.
    /// </summary>
    public class XferDocumentContext {
        /// <summary>
        /// All processing instructions found during parsing, each referencing the element they apply to.
        /// </summary>
        // Now supports MetadataElement as PI
        public IList<Element> ProcessingInstructions { get; } = [];

        /// <summary>
        /// All top-level elements in the document.
        /// </summary>
        public IList<object> Elements { get; } = [];
    }
}
