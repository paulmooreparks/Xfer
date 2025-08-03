using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

public class XferMetadata
{
    public const string XferKey = "xfer"; // Default version, can be overridden
    public const string VersionKey = "version"; // Default version, can be overridden

    public string? Xfer { get; set; } = Parser.Version;
    public string? Version { get; set; }
    // Add more known properties as needed

    // For user-defined/unknown keys:
    public Dictionary<string, object?> Extensions { get; } = [];
}
