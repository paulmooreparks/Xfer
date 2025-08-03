using System.Collections.Generic;
using System.Text;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions {
    // Base ProcessingInstruction: always contains a single KVP and (optionally) a Target
    public class ProcessingInstruction : TypedElement<Element> {
        public const string ElementName = "processingInstruction";
        public const char OpeningSpecifier = '!';
        public const char ClosingSpecifier = OpeningSpecifier;
        public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

        public ProcessingInstruction(Element value, string name) : base(value, name, ElementDelimiter) {
            Kvp = new KeyValuePairElement(new KeywordElement(name), value);
        }

        public virtual void ProcessingInstructionHandler() {
        }

        public virtual void ElementHandler(Element element) {
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
        public Element? Target { get; set; }
        public KeyValuePairElement Kvp { get; set; }
    }
}
