using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents an empty element in XferLang that contains no value.
/// Used for representing null or void values in the serialization format.
/// </summary>
public class EmptyElement : TypedElement<string>
{
    /// <summary>
    /// The element name used in XferLang serialization for empty elements.
    /// </summary>
    public static readonly string ElementName = "empty";

    /// <summary>
    /// Gets the value of this empty element, which is always an empty string.
    /// </summary>
    public override string Value => string.Empty;

    /// <summary>
    /// Initializes a new instance of the EmptyElement class.
    /// </summary>
    public EmptyElement() : base(string.Empty, ElementName, new('\0', '\0')) { }

    /// <summary>
    /// Serializes this empty element to its XferLang string representation.
    /// </summary>
    /// <returns>An empty string, as empty elements have no serialized representation</returns>
    public override string ToXfer() => string.Empty;

    /// <summary>
    /// Serializes this empty element to its XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style (ignored for empty elements)</param>
    /// <param name="indentChar">The indentation character (ignored for empty elements)</param>
    /// <param name="indentation">The indentation level (ignored for empty elements)</param>
    /// <param name="depth">The nesting depth (ignored for empty elements)</param>
    /// <returns>An empty string, as empty elements have no serialized representation</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) => string.Empty;

    /// <summary>
    /// Returns a string representation of this empty element.
    /// </summary>
    /// <returns>An empty string</returns>
    public override string ToString() => string.Empty;
}
