using System.Globalization;
using System.Text;
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

    private char Advance() {
        ++Position;

        while (char.IsWhiteSpace(CurrentChar)) {
            ++Position;
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
        return CurrentString;
    }

    private bool IsIdentifierChar(char c) {
        return char.IsLetterOrDigit(c) | c == '_' | c == '-';
    }

    private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>() {
        {"encoding", TokenType.EncodingKey},
        {"version", TokenType.VersionKey},
        {"urn", TokenType.UrnKey}
    };

    private bool IsKeyword(string compare, out TokenType tokenType) {
        return Keywords.TryGetValue(compare.ToLower(), out tokenType);
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

        var element = ParseMetadata();

        if (element is MetadataElement metadataElement) {
            document.Metadata = metadataElement;
        }

        var encodingName = document.Metadata.Encoding;

        Encoding = encodingName switch {
            "UTF-8" => Encoding.UTF8,
            "UTF-16" => Encoding.Unicode,
            "UTF-32" => Encoding.UTF32,
            "Unicode" => Encoding.Unicode,
            "ASCII" => Encoding.ASCII,
            _ => throw new NotSupportedException($"Encoding '{encodingName}' is not supported.")
        };

        while (CurrentChar != '\0') {
            var content = ParseElement();

            if (content is not CommentElement) {
                document.Root.Add(content);
            }
        }

        return document;
    }

    internal Element ParseMetadata() {
        if (ElementOpening(MetadataElement.OpeningMarker)) {
            var metadataElement = new MetadataElement();

            while (CurrentChar != MetadataElement.ClosingMarker && Peek != ElementClosingMarker) {
                var element = ParseKeyValuePair();

                if (element is KeyValuePairElement keyValuePairElement) {
                    metadataElement.AddOrUpdate(keyValuePairElement);
                }
                else {
                    throw new InvalidOperationException($"Unexpected element type: {element.GetType()}");
                }
            }

            Advance();
            Advance();

            return metadataElement;
        }

        return ParseElement();
    }

    internal Element ParseKeyValuePair() {
        var element = ParseKeyword();

        if (element is KeywordElement keywordElement) {
            var valueElement = ParseElement();
            element = new KeyValuePairElement(keywordElement.Value, valueElement);
        }
        else if (element is StringElement stringElement) {
            var valueElement = ParseElement();
            element = new KeyValuePairElement(stringElement.Value, valueElement);
        }
        else {
            throw new InvalidOperationException($"Unexpected key type: {element.GetType()}");
        }

        return element;
    }

    internal Element ParseKeyword() {
        if (IsIdentifierChar(Peek)) {
            while (IsIdentifierChar(Peek)) {
                Expand();
            }

            var element = new KeywordElement(CurrentString);
            Advance();
            return element;
        }

        return ParseElement();
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
            return true;
        }

        return false;
    }

    internal Element ParseElement() {
        while (CurrentChar != '\0') {
            switch (CurrentChar) {
                case ElementOpeningMarker:
                    Advance();

                    switch (CurrentChar) {
                        case CommentElement.OpeningMarker: {
                                Advance();

                                while (CurrentChar != '\0') {
                                    if (ElementClosing(CommentElement.ClosingMarker)) {
                                        return new CommentElement();
                                    }

                                    Expand();
                                }
                            }
                            break;

                        case PropertyBagElement.OpeningMarker: {
                                Advance();
                                var propBagElement = new PropertyBagElement();

                                while (CurrentChar != '\0') {
                                    if (ElementClosing(PropertyBagElement.ClosingMarker)) {
                                        return propBagElement;
                                    }

                                    var element = ParseElement();
                                    propBagElement.Add(element);
                                }
                            }
                            break;

                        case ObjectElement.OpeningMarker: {
                                Advance();
                                var objectElement = new ObjectElement();

                                while (CurrentChar != '\0') {
                                    if (ElementClosing(ObjectElement.ClosingMarker)) {
                                        return objectElement;
                                    }


                                    var element = ParseKeyValuePair();

                                    if (element is KeyValuePairElement kvp) {
                                        objectElement.Add(kvp);
                                    }
                                    else {
                                        throw new InvalidOperationException("Unexpected element type");
                                    }
                                }
                            }
                            break;

                        case ArrayElement.OpeningMarker: {
                                Advance();
                                var arrayElement = new ArrayElement();

                                while (CurrentChar != '\0') {
                                    if (ElementClosing(ArrayElement.ClosingMarker)) {
                                        return arrayElement;
                                    }

                                    var element = ParseElement();
                                    arrayElement.Add(element);
                                }
                            }
                            break;

                        case StringElement.OpeningMarker: {
                                Advance();
                                string value = CurrentString;

                                while (CurrentChar != '\0') {
                                    if (ElementClosing(StringElement.ClosingMarker)) {
                                        return new StringElement(value);
                                    }

                                    value = CurrentString;
                                    Expand();
                                }
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown element delimiter: {CurrentChar}");
                    }

                    if (CurrentChar == '\0') {
                        throw new InvalidOperationException("Expected closing delimiter");
                    }

                    break;

                default:
                    Advance();
                    break;
            }

            Advance();
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
}
