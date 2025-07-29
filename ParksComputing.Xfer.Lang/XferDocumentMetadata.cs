using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang;

public class XferDocumentMetadata
{
    public string? Version { get; set; }
    public string? DocumentVersion { get; set; }
    // Add more known properties as needed

    // For user-defined/unknown keys:
    public Dictionary<string, object?> Extensions { get; } = [];
}
