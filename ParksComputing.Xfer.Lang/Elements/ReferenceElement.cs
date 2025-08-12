using System.Text;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a dereference of a previously bound name using leading underscore(s): _name
/// Uses empty closing delimiter semantics similar to numeric and boolean elements.
/// </summary>
public class ReferenceElement : TypedElement<string> {
    /// <summary>Name used in serialization for reference elements.</summary>
    public static readonly string ElementName = "reference";
    /// <summary>Leading underscore character introducing a reference.</summary>
    public const char OpeningSpecifier = '_';
    /// <summary>Closing specifier (same underscore; empty closing semantics).</summary>
    public const char ClosingSpecifier = OpeningSpecifier;
    /// <summary>Element delimiter with empty closing portion for reference.</summary>
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Creates a reference element for a previously bound name.
    /// </summary>
    /// <param name="name">The binding name to resolve when evaluated.</param>
    /// <param name="specifierCount">Number of leading underscores (>=1) for stylistic grouping.</param>
    /// <param name="style">Delimiter style controlling implicit/compact/explicit emission.</param>
    public ReferenceElement(string name, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(name, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) { }

    /// <summary>
    /// Indicates this dereference element was produced as a clone during parse (for diagnostics).
    /// </summary>
    public bool IsClone { get; init; }

    /// <inheritdoc />
    public override string ToXfer() => ToXfer(Formatting.None);

    /// <inheritdoc />
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.CompactOpening}{Value}{Delimiter.CompactClosing}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{Value}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }
}
