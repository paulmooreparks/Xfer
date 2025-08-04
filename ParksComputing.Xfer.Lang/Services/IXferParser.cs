using System.Text;

namespace ParksComputing.Xfer.Lang.Services;

/// <summary>
/// Defines the contract for XferLang parsers that convert XferLang text into document objects.
/// </summary>
public interface IXferParser {
    /// <summary>
    /// Gets the text encoding used by this parser.
    /// </summary>
    Encoding Encoding { get; }

    /// <summary>
    /// Parses XferLang content from a byte array into an XferDocument.
    /// </summary>
    /// <param name="input">The byte array containing XferLang content to parse.</param>
    /// <returns>An XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    XferDocument Parse(byte[] input);

    /// <summary>
    /// Parses XferLang content from a string into an XferDocument.
    /// </summary>
    /// <param name="input">The string containing XferLang content to parse.</param>
    /// <returns>An XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    XferDocument Parse(string input);
}
