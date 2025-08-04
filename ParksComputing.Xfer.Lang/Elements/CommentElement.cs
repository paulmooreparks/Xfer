using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a comment element in XferLang using forward slash (/) delimiters.
/// Comments are non-semantic elements that provide documentation or annotations
/// but are not included in the actual data output. They are preserved during parsing
/// but excluded from serialization.
/// </summary>
public class CommentElement : Element
{
    /// <summary>
    /// The element name used in XferLang serialization for comment elements.
    /// </summary>
    public static readonly string ElementName = "comment";

    /// <summary>
    /// The opening delimiter character (forward slash) for comment elements.
    /// </summary>
    public const char OpeningSpecifier = '/';

    /// <summary>
    /// The closing delimiter character (forward slash) for comment elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for comment elements using forward slash characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the CommentElement class.
    /// </summary>
    public CommentElement() : base(ElementName, new(OpeningSpecifier, ClosingSpecifier)) { }

    /// <summary>
    /// Serializes this comment element to its XferLang string representation.
    /// </summary>
    /// <returns>An empty string, as comments are excluded from serialization output</returns>
    public override string ToXfer() => string.Empty;

    /// <summary>
    /// Serializes this comment element to its XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style (ignored for comments)</param>
    /// <param name="indentChar">The indentation character (ignored for comments)</param>
    /// <param name="indentation">The indentation level (ignored for comments)</param>
    /// <param name="depth">The nesting depth (ignored for comments)</param>
    /// <returns>An empty string, as comments are excluded from serialization output</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) => string.Empty;

    /// <summary>
    /// Returns a string representation of this comment element.
    /// </summary>
    /// <returns>An empty string</returns>
    public override string ToString() => string.Empty;
}
