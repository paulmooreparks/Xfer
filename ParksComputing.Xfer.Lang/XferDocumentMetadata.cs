using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Represents metadata information for an XferLang document.
/// Contains standard metadata fields and supports extensible custom metadata.
/// </summary>
public class XferMetadata
{
    /// <summary>
    /// The standard key name for the XferLang version metadata field.
    /// </summary>
    public const string XferKey = "xfer"; // Default version, can be overridden

    /// <summary>
    /// The standard key name for the document version metadata field.
    /// </summary>
    public const string VersionKey = "version"; // Default version, can be overridden

    /// <summary>
    /// Gets or sets the XferLang parser version used to create this document.
    /// Defaults to the current parser version.
    /// </summary>
    public string? Xfer { get; set; } = Parser.Version;

    /// <summary>
    /// Gets or sets the document version specified by the author.
    /// </summary>
    public string? Version { get; set; }
    // Add more known properties as needed

    /// <summary>
    /// Gets a dictionary containing user-defined or unknown metadata keys and their values.
    /// This allows for extensible metadata beyond the standard fields.
    /// </summary>
    public Dictionary<string, object?> Extensions { get; } = [];
}
