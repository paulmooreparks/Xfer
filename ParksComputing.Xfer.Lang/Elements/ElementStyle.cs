using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Specifies the different styles in which XferLang elements can be written and parsed.
/// </summary>
public enum ElementStyle
{
    /// <summary>
    /// Explicit style with full element delimiters: &lt;elementType content elementType&gt;
    /// </summary>
    Explicit = 0,

    /// <summary>
    /// Compact style with simplified delimiters and minimal whitespace.
    /// </summary>
    Compact = 1,

    /// <summary>
    /// Implicit style where element type is inferred from content format.
    /// </summary>
    Implicit = 2,
}
