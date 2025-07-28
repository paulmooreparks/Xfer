using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Deserialization {
    /// <summary>
    /// Default implementation of IDeserializationInstructionResolver.
    /// Applies global and inline deserialization PIs.
    /// </summary>
    public class DefaultDeserializationInstructionResolver : IDeserializationInstructionResolver {
        public virtual object? ResolveInstructions(Element element, XferDocument document) {
            // Look for global 'deserialize' PI in document.Root.Values
            var globalPI = document.Root.Values
                .OfType<ProcessingInstructionElement>()
                .FirstOrDefault(pi => pi.PIType == ProcessingInstructionElement.DeserializeKeyword);

            // Inline PI support: check element.AttachedMetadata for a matching PI
            ProcessingInstructionElement? inlinePI = null;
            if (element.AttachedMetadata != null) {
                inlinePI = element.AttachedMetadata
                    .OfType<ProcessingInstructionElement>()
                    .FirstOrDefault(pi => pi.PIType == ProcessingInstructionElement.DeserializeKeyword);
            }

            // Merge logic: prioritize inline, fallback to global
            return inlinePI ?? globalPI;
        }
    }
}
