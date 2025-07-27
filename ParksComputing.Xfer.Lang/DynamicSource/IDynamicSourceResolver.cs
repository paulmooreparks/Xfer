using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.DynamicSource
{
    public interface IDynamicSourceResolver
    {
        /// <summary>
        /// Resolves a dynamic value for the given key using PI directives in the document.
        /// </summary>
        /// <param name="key">The dynamic key to resolve.</param>
        /// <param name="document">The parsed XferDocument.</param>
        /// <returns>The resolved value, or null if not found.</returns>
        string? Resolve(string key, XferDocument document);
    }
}
