using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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

    /// <summary>
    /// Asynchronously parses XferLang content from a file using a new parser instance.
    /// </summary>
    /// <param name="filePath">The path to the file containing XferLang content to parse.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous parse operation. The task result contains an XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    public static async Task<XferDocument> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        return await ParseStreamAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously parses XferLang content from a stream using a new parser instance.
    /// Assumes UTF-8 encoding.
    /// </summary>
    /// <param name="stream">The stream containing XferLang content to parse.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous parse operation. The task result contains an XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    public static async Task<XferDocument> ParseStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
        return await ParseTextReaderAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously parses XferLang content from a TextReader using a new parser instance.
    /// </summary>
    /// <param name="reader">The TextReader containing XferLang content to parse.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous parse operation. The task result contains an XferDocument representing the parsed content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reader is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid XferLang syntax.</exception>
    public static async Task<XferDocument> ParseTextReaderAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        // For now, use the synchronous Parse method
        // In Phase 2, we can add async parsing that respects cancellation during parsing
        return Parse(content);
    }
}
