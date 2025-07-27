using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Deserialization {
    /// <summary>
    /// Interface for resolving deserialization instructions from Processing Instructions (PIs).
    /// Supports both document-level and inline PI-driven customization.
    /// </summary>
    public interface IDeserializationInstructionResolver {
        /// <summary>
        /// Resolves deserialization instructions for a given element, considering global and inline PIs.
        /// </summary>
        /// <param name="element">The element to be deserialized.</param>
        /// <param name="document">The XferDocument containing global PIs.</param>
        /// <returns>Deserialization options or instructions (implementation-defined).</returns>
        object? ResolveInstructions(Element element, XferDocument document);
    }
}
