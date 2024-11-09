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

            CurrentColumn = 0;
        }
        else if (CurrentChar == '\r') {
            CurrentRow++;
            CurrentColumn = 0;
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
            UpdateRowColumn();
            ++Position;
            UpdateRowColumn();

            while (CurrentChar == CommentElement.OpeningMarker) {
                ++markerCount;
                ++Position;
                UpdateRowColumn();
            }

            while (IsCharAvailable()) {
                if (CurrentChar == CommentElement.ClosingMarker) {
                    int tmpMarkerCount = markerCount;
                    --markerCount;

                    if (Peek == ElementClosingMarker && markerCount == 0) {
                        ++Position;
                        UpdateRowColumn();
                        ++Position;
                        UpdateRowColumn();
                        break;
                    }

                    bool commentClosed = false;

                    while (markerCount > 0 && Peek == CommentElement.ClosingMarker) {
                        ++Position;
                        UpdateRowColumn();
                        --markerCount;

                        if (Peek == ElementClosingMarker && markerCount == 0) {
                            ++Position;
                            UpdateRowColumn();
                            ++Position;
                            UpdateRowColumn();
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

    private void SkipBOM() {
        // If the current character is the BOM (0xFEFF), advance the position
        if (Position == 0 && CurrentChar == '\uFEFF') {
            Advance();
        }
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

    public XferDocument Parse(string input) {
        if (string.IsNullOrEmpty(input)) {
            throw new ArgumentNullException(nameof(input));
        }

        return Parse(Encoding.UTF8.GetBytes(input));
    }

    public XferDocument Parse(byte[] input) {
        if (input == null || input.Length == 0) {
            throw new ArgumentNullException(nameof(input));
        }

        ScanString = Encoding.GetString(input);

        if (CurrentChar == '\0') {
            return new XferDocument();
        }

        // Maybe do something with the BOM here?
        SkipBOM();

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

            if (ElementOpening(EvaluatedElement.OpeningMarker, ref markerCount)) {
                var literalElement = ParseEvaluatedElement(markerCount);
                SkipWhitespace();
                return literalElement;
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

            if (ElementOpening(IntegerElement.OpeningMarker, ref markerCount)) {
                var integerElement = ParseIntegerElement(markerCount);
                SkipWhitespace();
                return integerElement;
            }

            if (ElementOpening(LongElement.OpeningMarker, ref markerCount)) {
                var longIntegerElement = ParseLongIntegerElement(markerCount);
                SkipWhitespace();
                return longIntegerElement;
            }

            if (ElementOpening(DecimalElement.OpeningMarker, ref markerCount)) {
                var decimalElement = ParseDecimalElement(markerCount);
                SkipWhitespace();
                return decimalElement;
            }

            if (ElementOpening(DoubleElement.OpeningMarker, ref markerCount)) {
                var doubleElement = ParseDoubleElement(markerCount);
                SkipWhitespace();
                return doubleElement;
            }

            if (ElementOpening(BooleanElement.OpeningMarker, ref markerCount)) {
                var booleanElement = ParseBooleanElement(markerCount);
                SkipWhitespace();
                return booleanElement;
            }

            if (ElementOpening(DateElement.OpeningMarker, ref markerCount)) {
                var dateElement = ParseDateElement(markerCount);
                SkipWhitespace();
                return dateElement;
            }

            if (ElementOpening(PlaceholderElement.OpeningMarker, ref markerCount)) {
                var placeholderElement = ParsePlaceholderElement(markerCount);
                SkipWhitespace();
                return placeholderElement;
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

    private PlaceholderElement ParsePlaceholderElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(PlaceholderElement.ClosingMarker, markerCount)) {
                var variable = valueBuilder.ToString().Normalize(NormalizationForm.FormC);

                if (string.IsNullOrEmpty(variable)) {
                    throw new InvalidOperationException("Placeholder variable must be a non-empty string.");
                }

                var value = Environment.GetEnvironmentVariable(variable);
                return new PlaceholderElement(value ?? string.Empty, markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {EvaluatedElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
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

        var keyValuePairElement = new KeyValuePairElement(keyElement, markerCount);

        while (IsCharAvailable()) {
            if (ElementClosing(KeyValuePairElement.ClosingMarker, markerCount)) {
                return keyValuePairElement;
            }

            Element valueElement = ParseElement();
            SkipWhitespace();
            keyValuePairElement.TypedValue = valueElement;
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

        if (string.IsNullOrEmpty(charString)) {
            throw new InvalidOperationException($"Empty character element at row {CurrentRow}, column {CurrentColumn}.");
        }

        if (char.IsLetter(charString[0])) {
            // Keyword, e.g., <\nl\> for newline
            return charString.ToLower() switch {
                "nul" => new CharacterElement('\0'),
                "cr" => new CharacterElement('\r'),
                "lf" => new CharacterElement('\n'),
                "nl" => new CharacterElement('\n'),
                "tab" => new CharacterElement('\t'),
                "vtab" => new CharacterElement('\v'),
                "bksp" => new CharacterElement('\b'),
                "ff" => new CharacterElement('\f'),
                "bel" => new CharacterElement('\a'),
                "quote" => new CharacterElement('"'),
                "apos" => new CharacterElement('\''),
                "backslash" => new CharacterElement('\\'),
                "lt" => new CharacterElement('<'),
                "gt" => new CharacterElement('>'),
                _ => throw new InvalidOperationException($"Unknown character keyword '{charString}' at row {CurrentRow}, column {CurrentColumn}.")
            };
        }

        var codePoint = ParseNumericValue<int>(charString);
        char character = (char)codePoint;
        return new CharacterElement(character);
    }

    private EvaluatedElement ParseEvaluatedElement(int markerCount) {
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            int embeddedMarkerCount = 1;

            if (ElementOpening(StringElement.OpeningMarker, ref embeddedMarkerCount)) {
                StringElement stringElement = ParseStringElement(embeddedMarkerCount);
                valueBuilder.Append(stringElement.Value);
                continue;
            }

            if (ElementOpening(CharacterElement.OpeningMarker, ref embeddedMarkerCount)) {
                CharacterElement characterElement = ParseCharacterElement(embeddedMarkerCount);
                valueBuilder.Append(characterElement.Value);
                continue;
            }

            if (ElementOpening(IntegerElement.OpeningMarker, ref embeddedMarkerCount)) {
                IntegerElement integerElement = ParseIntegerElement(embeddedMarkerCount);
                valueBuilder.Append(integerElement.Value);
                continue;
            }

            if (ElementOpening(LongElement.OpeningMarker, ref embeddedMarkerCount)) {
                LongElement longElement = ParseLongIntegerElement(embeddedMarkerCount);
                valueBuilder.Append(longElement.Value);
                continue;
            }

            if (ElementOpening(DecimalElement.OpeningMarker, ref embeddedMarkerCount)) {
                DecimalElement decimalElement = ParseDecimalElement(embeddedMarkerCount);
                valueBuilder.Append(decimalElement.Value);
                continue;
            }

            if (ElementOpening(DoubleElement.OpeningMarker, ref embeddedMarkerCount)) {
                DoubleElement doubleElement = ParseDoubleElement(embeddedMarkerCount);
                valueBuilder.Append(doubleElement.Value);
                continue;
            }

            if (ElementOpening(BooleanElement.OpeningMarker, ref embeddedMarkerCount)) {
                BooleanElement booleanElement = ParseBooleanElement(embeddedMarkerCount);
                valueBuilder.Append(booleanElement.Value);
                continue;
            }

            if (ElementOpening(DateElement.OpeningMarker, ref embeddedMarkerCount)) {
                DateElement dateElement = ParseDateElement(embeddedMarkerCount);
                valueBuilder.Append(dateElement.Value);
                continue;
            }

            if (ElementOpening(EvaluatedElement.OpeningMarker, ref embeddedMarkerCount)) {
                EvaluatedElement evaluatedElement = ParseEvaluatedElement(embeddedMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementOpening(PlaceholderElement.OpeningMarker, ref embeddedMarkerCount)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement(embeddedMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing(EvaluatedElement.ClosingMarker, markerCount)) {
                return new EvaluatedElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {EvaluatedElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private StringElement ParseStringElement(int markerCount) {
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(StringElement.ClosingMarker, markerCount)) {
                return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {StringElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private IntegerElement ParseIntegerElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(IntegerElement.ClosingMarker, markerCount)) {
                var value = ParseNumericValue<int>(valueBuilder.ToString());
                return new IntegerElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {IntegerElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }


    private DateElement ParseDateElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(DateElement.ClosingMarker, markerCount)) {
                var value = valueBuilder.ToString();
                return new DateElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DateElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }        
    
    private LongElement ParseLongIntegerElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(LongElement.ClosingMarker, markerCount)) {
                var value = ParseNumericValue<long>(valueBuilder.ToString());
                return new LongElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {LongElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DecimalElement ParseDecimalElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(DecimalElement.ClosingMarker, markerCount)) {
                var value = ParseNumericValue<decimal>(valueBuilder.ToString());
                return new DecimalElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DecimalElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DoubleElement ParseDoubleElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(DoubleElement.ClosingMarker, markerCount)) {
                var value = ParseNumericValue<double>(valueBuilder.ToString());
                return new DoubleElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DoubleElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private BooleanElement ParseBooleanElement(int markerCount) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing(BooleanElement.ClosingMarker, markerCount)) {
                string valueString = valueBuilder.ToString().ToLower();
                bool value = valueString switch {
                    "true" => true,
                    "false" => false,
                    _ => throw new InvalidOperationException($"Invalid boolean value '{valueString}' at row {CurrentRow}, column {CurrentColumn}.")
                };

                return new BooleanElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {BooleanElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private T ParseNumericValue<T>(string valueString) where T : struct, IConvertible {
        if (string.IsNullOrEmpty(valueString)) {
            throw new ArgumentException("The numeric value string cannot be null or empty.", nameof(valueString));
        }

        char basePrefix = valueString[0];
        string numberString = valueString;

        // Determine the base (Hexadecimal, Binary, Decimal).
        int numberBase = 10; // Default to decimal

        if (basePrefix == '$' || basePrefix == '%') {
            numberBase = basePrefix == '$' ? 16 : 2;
            numberString = valueString.Substring(1); // Remove base prefix character
        }

        try {
            if (numberBase == 10) {
                // Handle decimal values
                if (typeof(T) == typeof(float)) {
                    return (T)Convert.ChangeType(float.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                }
                else if (typeof(T) == typeof(double)) {
                    return (T)Convert.ChangeType(double.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                }
                else if (typeof(T) == typeof(short)) {
                    return (T)Convert.ChangeType(short.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                }
                else if (typeof(T) == typeof(int)) {
                    return (T)Convert.ChangeType(int.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                }
                else if (typeof(T) == typeof(long)) {
                    return (T)Convert.ChangeType(long.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                }
                else if (typeof(T) == typeof(decimal)) {
                    return (T)Convert.ChangeType(decimal.Parse(valueString, CultureInfo.InvariantCulture), typeof(T));
                }
                else {
                    throw new InvalidOperationException($"Unsupported type '{typeof(T)}' for decimal value parsing at row {CurrentRow}, column {CurrentColumn}.");
                }
            }
            else if (numberBase == 16) {
                // Handle hexadecimal values
                long hexValue = long.Parse(numberString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return (T)Convert.ChangeType(hexValue, typeof(T));
            }
            else if (numberBase == 2) {
                // Handle binary values
                long binaryValue = long.Parse(numberString, NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
                return (T)Convert.ChangeType(binaryValue, typeof(T));
            }
            else {
                throw new InvalidOperationException($"Unsupported numeric base '{numberBase}' at row {CurrentRow}, column {CurrentColumn}.");
            }
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"Failed to parse numeric value '{valueString}' at row {CurrentRow}, column {CurrentColumn}. Error: {ex.Message}");
        }
    }
}
