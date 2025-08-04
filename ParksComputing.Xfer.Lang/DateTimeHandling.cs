using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Specifies how DateTime values should be handled during serialization and deserialization.
/// Controls time zone information and formatting in the resulting XferLang output.
/// </summary>
public enum DateTimeHandling {
    /// <summary>
    /// Serialize without time zone information using ISO format.
    /// </summary>
    Unspecified, // Serialize without time zone (ISO format)

    /// <summary>
    /// Serialize with local time zone information.
    /// </summary>
    Local,       // Serialize with local time zone

    /// <summary>
    /// Serialize with UTC time zone information.
    /// </summary>
    Utc,         // Serialize with UTC time zone

    /// <summary>
    /// Serialize preserving the original DateTimeOffset information for round-trip fidelity.
    /// </summary>
    RoundTrip    // Serialize preserving the original offset
}
