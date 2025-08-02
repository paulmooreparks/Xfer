using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class TupleElement : ListElement {
    public static readonly string ElementName = "tuple";
    public const char OpeningSpecifier = '(';
    public const char ClosingSpecifier = ')';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    /// <summary>
    /// Semantic items only
    /// </summary>
    public new IEnumerable<Element> Values => _items;

    public TupleElement(ElementStyle style = ElementStyle.Compact)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style)) {
    }

    public TupleElement(IEnumerable<Element> values) : this() {
        foreach (var v in values) { Add(v); }
    }

    public TupleElement(params Element[] values) : this() {
        foreach (var v in values) { Add(v); }
    }

    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

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

        // Output all children (valid elements and metadata) in order
        for (var i = 0; i < Children.Count; ++i) {
            var item = Children[i];
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(item.ToXfer(formatting, indentChar, indentation, depth + 1));
            if (!isIndented && item.Delimiter.Style is ElementStyle.Implicit or ElementStyle.Compact && i + 1 < Children.Count) {
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
                sb.Append(Delimiter.Closing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinClosing);
                break;
        }

        return sb.ToString();
    }

    public override string ToString() {
        return ToXfer();
    }
}
