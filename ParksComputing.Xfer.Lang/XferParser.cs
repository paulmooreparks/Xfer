using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Provides static convenience methods for parsing XferLang content into document objects.
/// This is a simplified interface to the underlying Parser class for common parsing scenarios.
/// </summary>
public class XferParser {
    /// <summary>
    /// Parses XferLang content from a byte array using a new parser instance.
    /// </summary>
    /// <param name="input">The byte array containing XferLang content to parse.</param>
    /// <returns>An XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    public static XferDocument Parse(byte[] input) => new Parser().Parse(input);

    /// <summary>
    /// Parses XferLang content from a string using a new parser instance.
    /// </summary>
    /// <param name="input">The string containing XferLang content to parse.</param>
    /// <returns>An XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    public static XferDocument Parse(string input) => new Parser().Parse(input);
}
