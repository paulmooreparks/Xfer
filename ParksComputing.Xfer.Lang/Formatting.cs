using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Specifies formatting options for XferLang document serialization.
/// These options control how the output text is structured and formatted.
/// </summary>
[Flags]
public enum Formatting {
    /// <summary>
    /// No special formatting applied. Output is compact with minimal whitespace.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Apply indentation to nested elements for improved readability.
    /// </summary>
    Indented = 0x01,

    /// <summary>
    /// Add spacing between elements for better visual separation.
    /// </summary>
    Spaced = 0x10,

    /// <summary>
    /// Combines indented and spaced formatting for maximum readability.
    /// Equivalent to <see cref="Indented"/> | <see cref="Spaced"/>.
    /// </summary>
    Pretty = Indented | Spaced
}
