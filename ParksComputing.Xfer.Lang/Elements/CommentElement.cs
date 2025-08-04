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
    public static readonly string ElementName = "comment";
    public const char OpeningSpecifier = '/';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public CommentElement() : base(ElementName, new(OpeningSpecifier, ClosingSpecifier)) { }

    public override string ToXfer() => string.Empty;
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) => string.Empty;
    public override string ToString() => string.Empty;
}
