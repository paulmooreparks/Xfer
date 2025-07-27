using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Deserialization {
    /// <summary>
    /// Default implementation of IDeserializationInstructionResolver.
    /// Applies global and inline deserialization PIs.
    /// </summary>
    public class DefaultDeserializationInstructionResolver : IDeserializationInstructionResolver {
        public virtual object? ResolveInstructions(Element element, XferDocument document) {
            // Example: Look for global 'deserialize' PI in document.MetadataCollection
            // and inline 'deserialize' PI attached to the element (if supported)
            // Return a merged instruction set or options
            var globalPI = document.MetadataCollection
                .OfType<ProcessingInstructionElement>()
                .FirstOrDefault(pi => pi.PIType == ProcessingInstructionElement.DeserializeKeyword);

            // Inline PI support: if element has attached PI, use/merge it
            // (Assume element.Metadata or similar property for inline PIs)
            ProcessingInstructionElement? inlinePI = null;
            if (element is MetadataElement meta && meta.ContainsKey(ProcessingInstructionElement.DeserializeKeyword)) {
                var kvp = meta[ProcessingInstructionElement.DeserializeKeyword];
                if (kvp is KeyValuePairElement piKvp && piKvp.Value is ProcessingInstructionElement piElem) {
                    inlinePI = piElem;
                }
            }

            // Merge logic: prioritize inline, fallback to global
            return inlinePI ?? globalPI;
        }
    }
}
