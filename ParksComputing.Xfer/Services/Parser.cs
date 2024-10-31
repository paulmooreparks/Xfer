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
            if (Position >= ScanString.Length) { 
                return '\0'; 
            }
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

    private char PreviousChar {
        get {
            if (Position - 1 < 0) { return '\0'; }
            return ScanString[Position - 1];
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
        if (CurrentChar == '\n') {
            if (PreviousChar != '\r') {
                CurrentRow++;
            }

            CurrentColumn = 1;
        }
        else if (CurrentChar == '\r') {
            CurrentRow++;
            CurrentColumn = 1;
        }
        else {
            CurrentColumn++;
        }
    }

    private char Advance() {
        ++Position;
        UpdateRowColumn();

        if (CurrentChar == ElementOpeningMarker && Peek == CommentElement.OpeningMarker) {
            int markerCount = 1;
            ++Position;
            ++CurrentColumn;
            ++Position;
            ++CurrentColumn;

            while (CurrentChar == CommentElement.OpeningMarker) {
                ++markerCount;
                ++Position;
                ++CurrentColumn;
            }

            while (IsCharAvailable()) {
                if (CurrentChar == CommentElement.ClosingMarker) {
                    int tmpMarkerCount = markerCount;
                    --markerCount;

                    if (Peek == ElementClosingMarker && markerCount == 0) {
                        ++Position;
                        ++CurrentColumn;
                        ++Position;
                        ++CurrentColumn;
                        break;
                    }

                    bool commentClosed = false;

                    while (markerCount > 0 && Peek == CommentElement.ClosingMarker) {
                        ++Position;
                        ++CurrentColumn;
                        --markerCount;

                        if (Peek == ElementClosingMarker && markerCount == 0) {
                            ++Position;
                            ++CurrentColumn;
                            ++Position;
                            ++CurrentColumn;
                            commentClosed = true;
                            break;
                        }
                    }

                    if (commentClosed) {
                        break;
                    }
                    else {
                        markerCount = tmpMarkerCount;
                    }
                }

                ++Position;
                UpdateRowColumn();
            }
        }

        Start = Position;
        return CurrentChar;
    }

    internal bool ElementOpening(char openingMarker, ref int markerCount) {
        if (CurrentChar == ElementOpeningMarker && Peek == openingMarker) {
            markerCount = 1;
            Advance();
            Advance();

            while (CurrentChar == openingMarker) {
                ++markerCount;
                Advance();
            }

            return true;
        }

        return false;
    }

    internal bool ElementClosing(char closingMarker, int markerCount) {
        if (CurrentChar == closingMarker) {
            --markerCount;

            if (Peek == ElementClosingMarker && markerCount == 0) {
                Advance();
                Advance();
                return true;
            }

            int tmpPosition = Position;

            while (markerCount > 0 && Peek == closingMarker) {
                Advance();
                --markerCount;

                if (Peek == ElementClosingMarker && markerCount == 0) {
                    Advance();
                    Advance();
                    return true;
                }
            }

            Position = tmpPosition;
        }

        return false;
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
        /* We may want to add more characters to this list. */
        return char.IsLetterOrDigit(c) | c == '_' | c == '-';
    }

    public XferDocument Parse(byte[] input) {
        if (input == null || input.Length == 0) {
            throw new ArgumentNullException(nameof(input));
        }

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
        else {
            document.Root.Add(element);
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
            int markerCount = 1;

            if (ElementOpening(StringElement.OpeningMarker, ref markerCount)) {
                var stringElement = ParseStringElement(markerCount);
                SkipWhitespace();
                return stringElement;
            }

            if (ElementOpening(CharacterElement.OpeningMarker, ref markerCount)) {
                var characterElement = ParseCharacterElement(markerCount);
                SkipWhitespace();
                return characterElement;
            }

            if (ElementOpening(KeyValuePairElement.OpeningMarker, ref markerCount)) {
                var keyValuePairElement = ParseKeyValuePairElement(markerCount);
                SkipWhitespace();
                return keyValuePairElement;
            }

            if (ElementOpening(PropertyBagElement.OpeningMarker, ref markerCount)) {
                var propertyBagElement = ParsePropertyBagElement(markerCount);
                SkipWhitespace();
                return propertyBagElement;
            }

            if (ElementOpening(MetadataElement.OpeningMarker, ref markerCount)) {
                var metadataElement = ParseMetadataElement(markerCount);
                SkipWhitespace();
                return metadataElement;
            }

            if (ElementOpening(ObjectElement.OpeningMarker, ref markerCount)) {
                var objectElement = ParseObjectElement(markerCount);
                SkipWhitespace();
                return objectElement;
            }

            if (ElementOpening(ArrayElement.OpeningMarker, ref markerCount)) {
                var arrayElement = ParseArrayElement(markerCount);
                SkipWhitespace();
                return arrayElement;
            }

            if (ElementOpening(CommentElement.OpeningMarker, ref markerCount)) {
                // Parse comment but don't return it, as comments are not part of the logical output.
                ParseCommentElement(markerCount);
                SkipWhitespace();
                continue;
            }

            throw new InvalidOperationException($"Expected element at row {CurrentRow}, column {CurrentColumn}.");
        }

        return new EmptyElement();
    }

    private void ParseCommentElement(int markerCount) {
        while (IsCharAvailable()) {
            if (ElementClosing(CommentElement.ClosingMarker, markerCount)) {
                break;
            }

            Advance();
        }
    }

    private ArrayElement ParseArrayElement(int markerCount) {
        SkipWhitespace();
        var arrayElement = new ArrayElement();

        while (IsCharAvailable()) {
            if (ElementClosing(ArrayElement.ClosingMarker, markerCount)) {
                return arrayElement;
            }

            var element = ParseElement();
            arrayElement.Add(element);
        }

        throw new InvalidOperationException($"Unexpected end of {ArrayElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private ObjectElement ParseObjectElement(int markerCount) {
        SkipWhitespace();
        var objectElement = new ObjectElement();

        while (IsCharAvailable()) {
            if (ElementClosing(ObjectElement.ClosingMarker, markerCount)) {
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

        throw new InvalidOperationException($"Unexpected end of {ObjectElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private MetadataElement ParseMetadataElement(int markerCount) {
        SkipWhitespace();
        var metadataElement = new MetadataElement();

        while (IsCharAvailable()) {
            if (ElementClosing(MetadataElement.ClosingMarker, markerCount)) {
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

        throw new InvalidOperationException($"Unexpected end of {MetadataElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private PropertyBagElement ParsePropertyBagElement(int markerCount) {
        SkipWhitespace();
        var propBagElement = new PropertyBagElement();

        while (IsCharAvailable()) {
            if (ElementClosing(PropertyBagElement.ClosingMarker, markerCount)) {
                return propBagElement;
            }

            var element = ParseElement();
            propBagElement.Add(element);
        }

        throw new InvalidOperationException($"Unexpected end of {PropertyBagElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private KeyValuePairElement ParseKeyValuePairElement(int markerCount) {
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
            if (ElementClosing(KeyValuePairElement.ClosingMarker, markerCount)) {
                return keyValuePairElement;
            }

            Element valueElement = ParseElement();
            keyValuePairElement.Value = valueElement;
        }

        throw new InvalidOperationException($"Unexpected end of {KeywordElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    /* 
    The following two methods need a LOT of work to handle the vagaries of Unicode. 
    I'm just trying to get through the basic scenarios first.
    */

    private CharacterElement ParseCharacterElement(int markerCount) {
        StringBuilder charContent = new();

        while (IsCharAvailable() && !ElementClosing(CharacterElement.ClosingMarker, markerCount)) {
            charContent.Append(CurrentChar);
            Advance();
        }

        string charString = charContent.ToString();

        // Determine if it's a keyword or a hexadecimal value
        if (charString.StartsWith("$")) {
            // Hexadecimal value, e.g., <\$2764\>
            string hexValue = charString.Substring(1);
            if (int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int codePoint)) {
                char character = (char)codePoint;
                return new CharacterElement(character);
            }
            else {
                throw new InvalidOperationException($"Invalid hexadecimal value '{hexValue}' at row {CurrentRow}, column {CurrentColumn}.");
            }
        }
        else {
            return charString switch {
                "nl" => new CharacterElement('\n'),
                "cr" => new CharacterElement('\r'),
                "tab" => new CharacterElement('\t'),
                "vtab" => new CharacterElement('\v'),
                "bksp" => new CharacterElement('\b'),
                "ff" => new CharacterElement('\f'),
                "nul" => new CharacterElement('\0'),
                "bel" => new CharacterElement('\a'),
                "quote" => new CharacterElement('"'),
                "apos" => new CharacterElement('\''),
                "backslash" => new CharacterElement('\\'),
                "lt" => new CharacterElement('<'),
                "gt" => new CharacterElement('>'),
                _ => throw new InvalidOperationException($"Unknown character keyword '{charString}' at row {CurrentRow}, column {CurrentColumn}.")
            };
        }
    }

    private StringElement ParseStringElement(int markerCount) {
        StringBuilder valueBuilder = new StringBuilder();
        int embeddedMarkerCount = 1;

        while (IsCharAvailable()) {
            if (ElementOpening(CharacterElement.OpeningMarker, ref embeddedMarkerCount)) {
                CharacterElement characterElement = ParseCharacterElement(embeddedMarkerCount);
                valueBuilder.Append(characterElement.Value);
                continue;
            }

            if (ElementClosing(StringElement.ClosingMarker, markerCount)) {
                return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        // Handle the case where the closing marker is not found (error)
        throw new InvalidOperationException($"Unexpected end of string element at row {CurrentRow}, column {CurrentColumn}.");
    }
}
