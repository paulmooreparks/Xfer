using System.Text;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a dereference of a previously bound name using leading underscore(s): _name
/// Uses empty closing delimiter semantics similar to numeric and boolean elements.
/// </summary>
public class DereferenceElement : TypedElement<string> {
    public static readonly string ElementName = "deref";
    public const char OpeningSpecifier = '_';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public DereferenceElement(string name, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(name, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) { }

    // Note: Dereference elements are parsed via a lightweight opening predicate (DereferenceElementOpening)
    // that treats a leading '_' followed by identifier characters as a dereference token, similar to how
    // Keyword/Identifier implicit forms are handled. The parser immediately attempts binding resolution
    // and substitutes a clone when possible.

    public override string ToXfer() => ToXfer(Formatting.None);

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var underscores = new string(OpeningSpecifier, Delimiter.SpecifierCount);
        return underscores + Value;
    }
}
