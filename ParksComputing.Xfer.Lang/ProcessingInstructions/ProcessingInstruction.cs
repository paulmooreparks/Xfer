using System.Collections.Generic;
using System.Text;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Represents a processing instruction in XferLang that provides metadata or processing directives
/// for elements in the document. Processing instructions are enclosed in &lt;! ... !&gt; delimiters
/// and contain key-value pairs that affect parsing or element behavior.
/// </summary>
// Base ProcessingInstruction: always contains a single KVP and (optionally) a Target
public class ProcessingInstruction : TypedElement<Element> {
    /// <summary>
    /// The element name used for processing instructions.
    /// </summary>
    public const string ElementName = "processingInstruction";

    /// <summary>
    /// The character used to open and close processing instruction elements ('!').
    /// </summary>
    public const char OpeningSpecifier = '!';

    /// <summary>
    /// The character used to close processing instruction elements (same as opening).
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The element delimiter configuration for processing instructions.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, ElementStyle.Explicit);

    /// <summary>
    /// Initializes a new instance of the ProcessingInstruction class with the specified value and name.
    /// </summary>
    /// <param name="value">The element value for the processing instruction.</param>
    /// <param name="name">The name/keyword for the processing instruction.</param>
    public ProcessingInstruction(Element value, string name) : base(value, name, ElementDelimiter) {
        Kvp = new KeyValuePairElement(new KeywordElement(name), value);
    }

    /// <summary>
    /// Virtual method for handling processing instruction-specific logic.
    /// Override this method in derived classes to implement custom processing instruction behavior.
    /// </summary>
    public virtual void ProcessingInstructionHandler() {
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
                sb.Append(Delimiter.ExplicitOpening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.CompactOpening);
                break;
        }

        if (isIndented) {
            sb.Append(Environment.NewLine);
        }

        if (isIndented) {
            sb.Append(nestIndent);
        }

        sb.Append(Kvp?.ToXfer(formatting, indentChar, indentation, depth + 1));

        // ProcessingInstructions only contain their core KVP - no additional children
        // Target is not included in serialization as it's a reference, not content

        if (isIndented) {
            sb.Append(Environment.NewLine);
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
