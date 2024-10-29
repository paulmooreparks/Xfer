using System.Globalization;
using System.Text;

using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Models;
using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xfer.Services;

public class Parser {
    public Parser() : this(Encoding.UTF8) { }

    public Parser(Encoding encoding) { 
        Encoding = encoding;
    }

    public const char ElementOpeningMarker = '<';
    public const char ElementClosingMarker = '>';

    public Encoding Encoding { get; private set; } = Encoding.UTF8;

    private string _scanString = string.Empty;

    private int Start { get; set; } = 0;

    private int Position { get; set; } = 0;

    private string ScanString {
        get {
            return _scanString;
        }
        set {
            Start = Position = 0;
            _scanString = value;
        }
    }

    private char CurrentChar {
        get {
            if (Position >= ScanString.Length) { return '\0'; }
            return ScanString[Position];
        }
    }

    private string CurrentString {
        get {
            if (Start >= ScanString.Length || Position >= ScanString.Length) { return string.Empty; }
            return ScanString.Substring(Start, Position - (Start - 1));
        }
    }

    private char Peek {
        get {
            if (Position + 1 >= ScanString.Length) { return '\0'; }
            return ScanString[Position + 1];
        }
    }

    private string Remaining {
        get {
            if (Position >= ScanString.Length) { return string.Empty; }
            return ScanString[Position..];
        }
    }

    private int CurrentRow { get; set; } = 1;
    private int CurrentColumn { get; set; } = 1;

    private void UpdateRowColumn() {
        if (CurrentChar == '\n') // Unix/Linux and macOS line ending (LF)
        {
            CurrentRow++;
            CurrentColumn = 1;
        }
        else if (CurrentChar == '\r') // Windows line ending (CR)
        {
            // Check for the next character to handle \r\n correctly (Windows CRLF)
            if (Peek == '\n') {
                ++Position;
                ++Position;
            }
            CurrentRow++;
            CurrentColumn = 1;
        }
        else {
            ++CurrentColumn;
        }
    }

    private char Advance() {
        ++Position;
        UpdateRowColumn();

        if (CurrentChar == ElementOpeningMarker && Peek == CommentElement.OpeningMarker) {
            ++Position;
            ++CurrentColumn;
            ++Position;
            ++CurrentColumn;

            while (IsCharAvailable()) {
                if (CurrentChar == CommentElement.OpeningMarker && Peek == ElementClosingMarker) {
                    ++Position;
                    ++CurrentColumn;
                    ++Position;
                    ++CurrentColumn;
                    break;
                }

                ++Position;
                UpdateRowColumn();
            }
        }

        Start = Position;
        return CurrentChar;
    }

    private void SkipWhitespace() {
        while (char.IsWhiteSpace(CurrentChar)) {
            Advance();
        }
    }

    private string Expand() {
        ++Position;
        UpdateRowColumn();

        return CurrentString;
    }

    private bool IsCharAvailable() => CurrentChar != '\0';

    private bool IsKeywordChar(char c) {
        return char.IsLetterOrDigit(c) | c == '_' | c == '-';
    }

    public XferDocument Parse(byte[] input) {
        if (input == null || input.Length == 0) {
            throw new ArgumentNullException(nameof(input));
        }

        // Convert byte array to string using the specified encoding
        ScanString = Encoding.GetString(input);

        if (CurrentChar == '\0') {
            return new XferDocument();
        }

        return ParseDocument();
    }

    internal XferDocument ParseDocument() {
        var document = new XferDocument();
        var element = ParseElement();

        if (element is MetadataElement metadataElement) {
            document.Metadata = metadataElement;
        }

        while (IsCharAvailable()) {
            var content = ParseElement();
            /* I'm still debating whether or not to allow metadata elements here. */
            document.Root.Add(content);
        }

        return document;
    }

    internal Element ParseElement() {
        SkipWhitespace();
        while (IsCharAvailable()) {
            if (ElementOpening(StringElement.OpeningMarker)) {
                string value = CurrentString;

                while (IsCharAvailable()) {
                    if (ElementClosing(StringElement.ClosingMarker)) {
                        return new StringElement(value);
                    }

                    value = CurrentString;
                    Expand();
                }
            }

            if (ElementOpening(KeyValuePairElement.OpeningMarker)) {
                SkipWhitespace();
                while (Peek.IsKeywordChar()) {
                    Expand();
                }

                if (string.IsNullOrEmpty(CurrentString)) {
                    throw new InvalidOperationException("Key must be a non-empty string.");
                }

                var keyElement = new KeywordElement(CurrentString);
                Advance();
                SkipWhitespace();

                var keyValuePairElement = new KeyValuePairElement(keyElement);

                while (IsCharAvailable()) {
                    if (ElementClosing(KeyValuePairElement.ClosingMarker)) {
                        return keyValuePairElement;
                    }

                    Element valueElement = ParseElement();
                    keyValuePairElement.Value = valueElement;
                }
            }
            
            if (ElementOpening(PropertyBagElement.OpeningMarker)) {
                SkipWhitespace();
                var propBagElement = new PropertyBagElement();

                while (IsCharAvailable()) {
                    if (ElementClosing(PropertyBagElement.ClosingMarker)) {
                        return propBagElement;
                    }

                    var element = ParseElement();
                    propBagElement.Add(element);
                }
            }
            
            if (ElementOpening(MetadataElement.OpeningMarker)) {
                SkipWhitespace();
                var metadataElement = new MetadataElement();

                while (IsCharAvailable()) {
                    if (ElementClosing(MetadataElement.ClosingMarker)) {
                        return metadataElement;
                    }

                    var element = ParseElement();

                    if (element is KeyValuePairElement kvp) {
                        metadataElement.AddOrUpdate(kvp);
                    }
                    else {
                        throw new InvalidOperationException("Unexpected element type");
                    }
                }
            }
            
            if (ElementOpening(ObjectElement.OpeningMarker)) {
                SkipWhitespace();
                var objectElement = new ObjectElement();

                while (IsCharAvailable()) {
                    if (ElementClosing(ObjectElement.ClosingMarker)) {
                        return objectElement;
                    }

                    var element = ParseElement();

                    if (element is KeyValuePairElement kvp) {
                        objectElement.AddOrUpdate(kvp);
                    }
                    else {
                        throw new InvalidOperationException("Unexpected element type");
                    }
                }
            }
            
            if (ElementOpening(ArrayElement.OpeningMarker)) {
                SkipWhitespace();
                var arrayElement = new ArrayElement();

                while (IsCharAvailable()) {
                    if (ElementClosing(ArrayElement.ClosingMarker)) {
                        return arrayElement;
                    }

                    var element = ParseElement();
                    arrayElement.Add(element);
                }
            }

            if (ElementOpening(CommentElement.OpeningMarker)) {
                SkipWhitespace();

                while (IsCharAvailable()) {
                    if (ElementClosing(CommentElement.ClosingMarker)) {
                        break;
                    }

                    Advance();
                }

                continue;
            }

            throw new InvalidOperationException($"Expected element at row {CurrentRow}, column {CurrentColumn}.");
        }

        return new EmptyElement();

#if false
        if (char.IsDigit(CurrentChar) || CurrentChar.ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) {
            while (char.IsDigit(Peek) || Peek.ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) {
                Expand();
            }

            if (double.TryParse(CurrentString, out double doubleValue)) {
                var element = new DoubleElement(doubleValue);
                return element;
            }
            else if (int.TryParse(CurrentString, out int intValue)) {
                var element = new IntegerElement(intValue);
                return element;
            }
        }
        else if (IsIdentifierChar(CurrentChar)) {
            while (IsIdentifierChar(Peek)) {
                Expand();
            }

            TokenType tokenType;

            if (!IsKeyword(CurrentString, out tokenType)) {
                tokenType = TokenType.Identifier;
            }

            var element = new KeywordElement(CurrentString, tokenType);
            return element;
        }
#endif
    }

    internal bool ElementOpening(char openingMarker) {
        if (CurrentChar == ElementOpeningMarker && Peek == openingMarker) {
            Advance();
            Advance();
            return true;
        }

        return false;
    }

    internal bool ElementClosing(char closingMarker) {
        if (CurrentChar == closingMarker && Peek == ElementClosingMarker) {
            Advance();
            Advance();
            SkipWhitespace();
            return true;
        }

        return false;
    }

}
