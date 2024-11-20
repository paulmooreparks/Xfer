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

        if (CurrentChar == Element.ElementOpeningMarker && Peek == CommentElement.OpeningMarker) {
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

                    if (Peek == Element.ElementClosingMarker && markerCount == 0) {
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

                        if (Peek == Element.ElementClosingMarker && markerCount == 0) {
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

    internal bool ElementOpening(char openingMarker) {
        int markerCount = 1;
        return ElementOpening(openingMarker, openingMarker, ref markerCount);
    }

    internal bool ElementOpening(Delimiter delimiter) {
        int markerCount = 1;
        return ElementOpening(delimiter.OpeningMarker, delimiter.ClosingMarker, ref markerCount);
    }

    internal bool ElementOpening(Delimiter delimiter, ref int markerCount) {
        return ElementOpening(delimiter.OpeningMarker, delimiter.ClosingMarker, ref markerCount);
    }

    private Stack<Delimiter> _delimStack = new();



    internal bool ElementOpening(char openingMarker, char closingMarker, ref int markerCount) {
        if (CurrentChar == Element.ElementOpeningMarker && Peek == openingMarker) {
            markerCount = 1;
            Advance();
            Advance();

            while (CurrentChar == openingMarker) {
                ++markerCount;
                Advance();
            }

            _delimStack.Push(new Delimiter(openingMarker, closingMarker, markerCount));
            return true;
        }

        if (CurrentChar == openingMarker) {
            int tmpPosition = Position;
            Advance();

            while (CurrentChar == openingMarker) {
                ++markerCount;
                Advance();
            }

            if (CurrentChar != Element.ElementClosingMarker) {
                _delimStack.Push(new Delimiter(openingMarker, closingMarker, markerCount, isMinimized: true));
                return true;
            }

            Position = tmpPosition;
        }

        return false;
    }

    internal bool ElementClosing() {
        if (_delimStack.Count == 0) return false;

        var delimiter = _delimStack.Peek();
        int markerCount = delimiter.Count;

        if (delimiter.IsMinimized && char.IsWhiteSpace(CurrentChar)) {
            _delimStack.Pop();
            return true;
        }

        if (CurrentChar == delimiter.ClosingMarker) {
            --markerCount;

            if (Peek == Element.ElementClosingMarker && markerCount == 0) {
                Advance();
                Advance();
                _delimStack.Pop();
                return true;
            }

            int tmpPosition = Position;

            while (markerCount > 0 && Peek == delimiter.ClosingMarker) {
                Advance();
                --markerCount;

                if (delimiter.IsMinimized) {
                    if (markerCount == 0) {
                        Advance();
                        _delimStack.Pop();
                        return true;
                    }
                }
                else if (Peek == Element.ElementClosingMarker && markerCount == 0) {
                    Advance();
                    Advance();
                    _delimStack.Pop();
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

            if (CurrentChar.IsKeywordChar()) {
                var keyValuePairElement = ParseKeyValuePairElement(markerCount);
                SkipWhitespace();
                return keyValuePairElement;
            }

            if (ElementOpening(StringElement.ElementDelimiter, ref markerCount)) {
                var stringElement = ParseStringElement(markerCount);
                SkipWhitespace();
                return stringElement;
            }

            if (ElementOpening(EvaluatedElement.ElementDelimiter, ref markerCount)) {
                var literalElement = ParseEvaluatedElement(markerCount);
                SkipWhitespace();
                return literalElement;
            }

            if (ElementOpening(CharacterElement.ElementDelimiter, ref markerCount)) {
                var characterElement = ParseCharacterElement(markerCount);
                SkipWhitespace();
                return characterElement;
            }

            if (ElementOpening(PropertyBagElement.ElementDelimiter, ref markerCount)) {
                var propertyBagElement = ParsePropertyBagElement(markerCount);
                SkipWhitespace();
                return propertyBagElement;
            }

            if (ElementOpening(MetadataElement.ElementDelimiter, ref markerCount)) {
                var metadataElement = ParseMetadataElement(markerCount);
                SkipWhitespace();
                return metadataElement;
            }

            if (ElementOpening(ObjectElement.ElementDelimiter, ref markerCount)) {
                var objectElement = ParseObjectElement(markerCount);
                SkipWhitespace();
                return objectElement;
            }

            if (ElementOpening(ArrayElement.ElementDelimiter, ref markerCount)) {
                var arrayElement = ParseArrayElement(markerCount);
                SkipWhitespace();
                return arrayElement;
            }

            if (ElementOpening(IntegerElement.ElementDelimiter, ref markerCount)) {
                var integerElement = ParseIntegerElement(markerCount);
                SkipWhitespace();
                return integerElement;
            }

            if (ElementOpening(LongElement.ElementDelimiter, ref markerCount)) {
                var longIntegerElement = ParseLongIntegerElement(markerCount);
                SkipWhitespace();
                return longIntegerElement;
            }

            if (ElementOpening(DecimalElement.ElementDelimiter, ref markerCount)) {
                var decimalElement = ParseDecimalElement(markerCount);
                SkipWhitespace();
                return decimalElement;
            }

            if (ElementOpening(DoubleElement.ElementDelimiter, ref markerCount)) {
                var doubleElement = ParseDoubleElement(markerCount);
                SkipWhitespace();
                return doubleElement;
            }

            if (ElementOpening(BooleanElement.ElementDelimiter, ref markerCount)) {
                var booleanElement = ParseBooleanElement(markerCount);
                SkipWhitespace();
                return booleanElement;
            }

            if (ElementOpening(DateElement.ElementDelimiter, ref markerCount)) {
                var dateElement = ParseDateElement(markerCount);
                SkipWhitespace();
                return dateElement;
            }

            if (ElementOpening(PlaceholderElement.ElementDelimiter, ref markerCount)) {
                var placeholderElement = ParsePlaceholderElement(markerCount);
                SkipWhitespace();
                return placeholderElement;
            }

            if (ElementOpening(CommentElement.ElementDelimiter, ref markerCount)) {
                // Parse comment but don't return it, as comments are not part of the logical output.
                ParseCommentElement(markerCount);
                SkipWhitespace();
                continue;
            }

            throw new InvalidOperationException($"Expected element at row {CurrentRow}, column {CurrentColumn}.");
        }

        return new EmptyElement();
    }

    private void ParseCommentElement(int markerCount = 1) {
        while (IsCharAvailable()) {
            if (ElementClosing()) {
                break;
            }

            Advance();
        }
    }

    private ArrayElement ParseArrayElement(int markerCount = 1) {
        SkipWhitespace();

        if (IsCharAvailable()) {
            var element = ParseElement();

            /* This looks and feels kind of dirty, for some reason, yet it does the job. Sure, I could put this in a 
            dictionary or make a factory, but that just moves the ugliness around. */

            ArrayElement arrayElement = element switch {
                IntegerElement => new TypedArrayElement<IntegerElement>(),
                LongElement => new TypedArrayElement<LongElement>(),
                DecimalElement => new TypedArrayElement<DecimalElement>(),
                DoubleElement => new TypedArrayElement<DoubleElement>(),
                BooleanElement => new TypedArrayElement<BooleanElement>(),
                DateElement => new TypedArrayElement<DateElement>(),
                TextElement => new TypedArrayElement<TextElement>(),
                CharacterElement => new TypedArrayElement<CharacterElement>(),
                KeyValuePairElement => new TypedArrayElement<KeyValuePairElement>(),
                ObjectElement => new TypedArrayElement<ObjectElement>(),
                MetadataElement => new TypedArrayElement<MetadataElement>(),
                PropertyBagElement => new TypedArrayElement<PropertyBagElement>(),
                _ => new TypedArrayElement<Element>() // Will this even happen?
            };

            arrayElement.Add(element);

            while (IsCharAvailable()) {
                if (ElementClosing()) {
                    return arrayElement;
                }

                element = ParseElement();
                arrayElement.Add(element);
            }

        }
        throw new InvalidOperationException($"Unexpected end of {ArrayElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private ObjectElement ParseObjectElement(int markerCount = 1) {
        SkipWhitespace();
        var objectElement = new ObjectElement();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
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

    private MetadataElement ParseMetadataElement(int markerCount = 1) {
        SkipWhitespace();
        var metadataElement = new MetadataElement();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
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

    private PropertyBagElement ParsePropertyBagElement(int markerCount = 1) {
        SkipWhitespace();
        var propBagElement = new PropertyBagElement();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
                return propBagElement;
            }

            var element = ParseElement();
            propBagElement.Add(element);
        }

        throw new InvalidOperationException($"Unexpected end of {PropertyBagElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private PlaceholderElement ParsePlaceholderElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
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

        throw new InvalidOperationException($"Unexpected end of {PlaceholderElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private KeyValuePairElement ParseKeyValuePairElement(int markerCount = 1) {
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

        if (IsCharAvailable()) {
            Element valueElement = ParseElement();
            keyValuePairElement.Value = valueElement;
            return keyValuePairElement;
        }

        throw new InvalidOperationException($"Unexpected end of {KeywordElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    /* 
    The following two methods need a LOT of work to handle the vagaries of Unicode. 
    I'm just trying to get through the basic scenarios first.
    */

    private CharacterElement ParseCharacterElement(int markerCount = 1) {
        StringBuilder charContent = new();

        while (IsCharAvailable() && !ElementClosing()) {
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

    private EvaluatedElement ParseEvaluatedElement(int markerCount = 1) {
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            int embeddedMarkerCount = 1;

            if (ElementOpening(StringElement.ElementDelimiter, ref embeddedMarkerCount)) {
                StringElement stringElement = ParseStringElement(embeddedMarkerCount);
                valueBuilder.Append(stringElement.Value);
                continue;
            }

            if (ElementOpening(CharacterElement.ElementDelimiter, ref embeddedMarkerCount)) {
                CharacterElement characterElement = ParseCharacterElement(embeddedMarkerCount);
                valueBuilder.Append(characterElement.Value);
                continue;
            }

            if (ElementOpening(IntegerElement.ElementDelimiter, ref embeddedMarkerCount)) {
                IntegerElement integerElement = ParseIntegerElement(embeddedMarkerCount);
                valueBuilder.Append(integerElement.Value);
                continue;
            }

            if (ElementOpening(LongElement.ElementDelimiter, ref embeddedMarkerCount)) {
                LongElement longElement = ParseLongIntegerElement(embeddedMarkerCount);
                valueBuilder.Append(longElement.Value);
                continue;
            }

            if (ElementOpening(DecimalElement.ElementDelimiter, ref embeddedMarkerCount)) {
                DecimalElement decimalElement = ParseDecimalElement(embeddedMarkerCount);
                valueBuilder.Append(decimalElement.Value);
                continue;
            }

            if (ElementOpening(DoubleElement.ElementDelimiter, ref embeddedMarkerCount)) {
                DoubleElement doubleElement = ParseDoubleElement(embeddedMarkerCount);
                valueBuilder.Append(doubleElement.Value);
                continue;
            }

            if (ElementOpening(BooleanElement.ElementDelimiter, ref embeddedMarkerCount)) {
                BooleanElement booleanElement = ParseBooleanElement(embeddedMarkerCount);
                valueBuilder.Append(booleanElement.Value);
                continue;
            }

            if (ElementOpening(DateElement.ElementDelimiter, ref embeddedMarkerCount)) {
                DateElement dateElement = ParseDateElement(embeddedMarkerCount);
                valueBuilder.Append(dateElement.Value);
                continue;
            }

            if (ElementOpening(EvaluatedElement.ElementDelimiter, ref embeddedMarkerCount)) {
                EvaluatedElement evaluatedElement = ParseEvaluatedElement(embeddedMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementOpening(PlaceholderElement.ElementDelimiter, ref embeddedMarkerCount)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement(embeddedMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementOpening(CommentElement.ElementDelimiter, ref markerCount)) {
                ParseCommentElement(markerCount);
                continue;
            }

            if (ElementClosing()) {
                return new EvaluatedElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {EvaluatedElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private StringElement ParseStringElement(int markerCount = 1) {
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
                return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {StringElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private IntegerElement ParseIntegerElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
                var value = ParseNumericValue<int>(valueBuilder.ToString());
                return new IntegerElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {IntegerElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }


    private DateElement ParseDateElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
                var value = valueBuilder.ToString();
                return new DateElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DateElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }        
    
    private LongElement ParseLongIntegerElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
                var value = ParseNumericValue<long>(valueBuilder.ToString());
                return new LongElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {LongElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DecimalElement ParseDecimalElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
                var value = ParseNumericValue<decimal>(valueBuilder.ToString());
                return new DecimalElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DecimalElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DoubleElement ParseDoubleElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
                var value = ParseNumericValue<double>(valueBuilder.ToString());
                return new DoubleElement(value);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DoubleElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private BooleanElement ParseBooleanElement(int markerCount = 1) {
        SkipWhitespace();
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementClosing()) {
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
