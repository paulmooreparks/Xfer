using System.Collections.Generic;
using System.Text;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions {
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
