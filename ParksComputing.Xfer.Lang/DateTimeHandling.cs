using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang;

public enum DateTimeHandling {
    Unspecified, // Serialize without time zone (ISO format)
    Local,       // Serialize with local time zone
    Utc,         // Serialize with UTC time zone
    RoundTrip    // Serialize preserving the original offset
}
