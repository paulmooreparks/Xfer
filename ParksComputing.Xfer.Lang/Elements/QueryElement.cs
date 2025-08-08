using System.Text;

namespace ParksComputing.Xfer.Lang.Elements;

public class QueryElement : TypedElement<KeyValuePairElement> {
    /// <summary>
    /// The element name used for query elements.
    /// </summary>
    public const string ElementName = "query";

    /// <summary>
    /// The character used to open and close query elements (';').
    /// </summary>
    public const char OpeningSpecifier = ';';

    /// <summary>
    /// The character used to close query elements (same as opening).
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The element delimiter configuration for query elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the QueryElement class with the specified value and name.
    /// </summary>
    /// <param name="value">The element value for the query element.</param>
    public QueryElement(KeyValuePairElement value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
        Kvp = value;
    }

    /// <summary>
    /// Virtual method for handling query-specific logic.
    /// Override this method in derived classes to implement custom query behavior.
    /// </summary>
    public virtual void QueryHandler() {
    }

    /// <summary>
    /// Virtual method for handling element-specific processing.
    /// Override this method in derived classes to implement custom element handling logic.
    /// </summary>
    /// <param name="element">The element to be processed.</param>
    public virtual void ElementHandler(Element element) {
    }

    /// <summary>
    /// Converts the processing instruction to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the processing instruction.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the processing instruction to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the processing instruction.</returns>
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
                sb.Append(Delimiter.Opening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinOpening);
                break;
        }

        if (isIndented) {
            sb.Append(Environment.NewLine);
        }

        if (isIndented) {
            sb.Append(nestIndent);
        }

        sb.Append(Kvp?.ToXfer(formatting, indentChar, indentation, depth + 1));

        if (isIndented) {
            sb.Append(Environment.NewLine);
            sb.Append(rootIndent);
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Closing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinClosing);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets or sets the target element that this processing instruction applies to.
    /// Can be null if the processing instruction applies globally or has no specific target.
    /// </summary>
    public Element? Target { get; set; }

    /// <summary>
    /// Gets or sets the key-value pair element that contains the processing instruction's name and value.
    /// This represents the structured data of the processing instruction.
    /// </summary>
    public KeyValuePairElement Kvp { get; set; }
}
