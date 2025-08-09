using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a tuple element in XferLang that contains an ordered collection of elements.
/// Tuples are delimited by parentheses () and preserve element order, making them suitable
/// for representing structured data with positional significance.
/// </summary>
public class TupleElement : ListElement {
    /// <summary>
    /// The element name used for tuple elements.
    /// </summary>
    public static readonly string ElementName = "tuple";

    /// <summary>
    /// The character used to open tuple elements ('(').
    /// </summary>
    public const char OpeningSpecifier = '(';

    /// <summary>
    /// The character used to close tuple elements (')').
    /// </summary>
    public const char ClosingSpecifier = ')';

    /// <summary>
    /// The element delimiter configuration for tuple elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    /// <summary>
    /// Initializes a new instance of the TupleElement class with the specified style.
    /// </summary>
    /// <param name="style">The element style to use (default is compact).</param>
    public TupleElement(ElementStyle style = ElementStyle.Compact)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style)) {
    }

    /// <summary>
    /// Initializes a new instance of the TupleElement class with the specified collection of elements.
    /// </summary>
    /// <param name="values">The elements to add to the tuple.</param>
    public TupleElement(IEnumerable<Element> values) : this() {
        foreach (var v in values) { Add(v); }
    }

    /// <summary>
    /// Initializes a new instance of the TupleElement class with the specified array of elements.
    /// </summary>
    /// <param name="values">The elements to add to the tuple.</param>
    public TupleElement(params Element[] values) : this() {
        foreach (var v in values) { Add(v); }
    }

    /// <summary>
    /// Converts the tuple element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the tuple element.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the tuple element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the tuple element.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        bool isIndented = (formatting & Formatting.Indented) == Formatting.Indented;
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        string rootIndent = string.Empty;
        string nestIndent = string.Empty;

        var sb = new StringBuilder();

        if (isIndented) {
            rootIndent = new string(indentChar, indentation * depth);
            nestIndent = new string(indentChar, indentation * (depth + 1));
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.ExplicitOpening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.CompactOpening);
                break;
        }

        if (isIndented) {
            sb.Append(Environment.NewLine);
        }

        // Output all children (valid elements and metadata) in order
        for (var i = 0; i < Children.Count; ++i) {
            var item = Children[i];
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(item.ToXfer(formatting, indentChar, indentation, depth + 1));
            if (!isIndented && item.Delimiter.Style is ElementStyle.Implicit || (i + 1 < Children.Count && item.Delimiter.Style is ElementStyle.Compact && item.Delimiter.CompactClosing == string.Empty)) {
                sb.Append(' ');
            }
            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
        }

        if (isIndented) {
            sb.Append(rootIndent);
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.ExplicitClosing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.CompactClosing);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the tuple element.
    /// </summary>
    /// <returns>The XferLang representation of the tuple element.</returns>
    public override string ToString() {
        return ToXfer();
    }
}
