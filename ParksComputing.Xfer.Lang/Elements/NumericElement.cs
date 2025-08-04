using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class for numeric elements in XferLang.
/// Provides common functionality for serializing numeric types with configurable delimiters.
/// </summary>
/// <typeparam name="T">The numeric type this element represents</typeparam>
public abstract class NumericElement<T> : TypedElement<T>
{
    /// <summary>
    /// Initializes a new instance of the NumericElement class.
    /// </summary>
    /// <param name="value">The numeric value to represent</param>
    /// <param name="name">The element name for XferLang serialization</param>
    /// <param name="delimiter">The delimiter configuration for this element</param>
    public NumericElement(T value, string name, ElementDelimiter delimiter) : base(value, name, delimiter)
    {
    }

    /// <summary>
    /// Serializes this numeric element to its XferLang string representation using default formatting.
    /// </summary>
    /// <returns>The XferLang string representation of this numeric element</returns>
    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Serializes this numeric element to its XferLang string representation with specified formatting.
    /// Supports implicit, compact, and explicit delimiter styles.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this numeric element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit)
        {
            sb.Append($"{Value} ");
        }
        else if (Delimiter.Style == ElementStyle.Compact)
        {
            sb.Append($"{Delimiter.OpeningSpecifier}{Value} ");
        }
        else
        {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of this numeric element using its value.
    /// </summary>
    /// <returns>The string representation of the numeric value</returns>
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}
