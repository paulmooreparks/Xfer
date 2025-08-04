using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Extensions;

/// <summary>
/// Provides extension methods for converting .NET objects to XferLang format.
/// Simplifies serialization by adding convenient ToXfer() method to all objects.
/// </summary>
public static class ObjectExtensions {
    /// <summary>
    /// Converts any .NET object to its XferLang string representation.
    /// Uses XferConvert.Serialize internally with default settings.
    /// </summary>
    /// <param name="obj">The object to serialize to XferLang format.</param>
    /// <returns>The XferLang string representation of the object.</returns>
    public static string ToXfer(this object obj) {
        string xfer = XferConvert.Serialize(obj);
        return xfer;
    }
}
