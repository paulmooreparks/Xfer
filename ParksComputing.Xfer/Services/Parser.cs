using System.Globalization;
using System.Text;

using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xfer.Services;

/* This parser is ROUGH. I'm trying out a lot of ideas, some of them supported in parallel. Once I 
settle on a solid grammar, I'll redo the parser or use some kind of tool to rewrite it. */

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

    private Stack<ElementDelimiter> _delimStack = new();

    internal bool ElementOpening(ElementDelimiter delimiter) {
        return ElementOpening(delimiter, out int _);
    }

    internal bool ElementOpening(ElementDelimiter delimiter, out int markerCount) {
        char openingMarker = delimiter.OpeningMarker;
        char closingMarker = delimiter.ClosingMarker;

        if (ElementMaxOpening(delimiter, out markerCount)) {
            return true;
        }

        markerCount = 1;

        if (CurrentChar == openingMarker) {
            int tmpPosition = Position;
            Advance();

            while (CurrentChar == openingMarker) {
                ++markerCount;
                Advance();
            }

            if (CurrentChar != Element.ElementClosingMarker) {
                _delimStack.Push(new ElementDelimiter(openingMarker, closingMarker, markerCount, ElementStyle.Minimized));
                return true;
            }

            Position = tmpPosition;
        }

        return false;
    }

    internal bool ElementMaxOpening(ElementDelimiter delimiter) {
        return ElementOpening(delimiter, out int _);
    }

    internal bool ElementMaxOpening(ElementDelimiter delimiter, out int markerCount) {
        char openingMarker = delimiter.OpeningMarker;
        char closingMarker = delimiter.ClosingMarker;

        markerCount = 1;

        if (CurrentChar == Element.ElementOpeningMarker && Peek == openingMarker) {
            Advance();
            Advance();

            /* This is really ugly to me, and it seems I'm missing a more elegant way to parse this. What I'm 
            doing here is handling empty elements, like <""> and <##>. */
            if (CurrentChar == closingMarker && Peek == Element.ElementClosingMarker) {
                _delimStack.Push(new ElementDelimiter(openingMarker, closingMarker, markerCount, ElementStyle.Normal));
                return true;
            }

            while (CurrentChar == openingMarker) {
                ++markerCount;
                Advance();
            }

            _delimStack.Push(new ElementDelimiter(openingMarker, closingMarker, markerCount, ElementStyle.Normal));
            return true;
        }

        return false;
    }

    internal bool ElementMinClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        if ((_delimStack.Peek().Style == ElementStyle.Minimized && char.IsWhiteSpace(CurrentChar)) || 
            CurrentChar == ObjectElement.ClosingMarker || 
            CurrentChar == ArrayElement.ClosingMarker || 
            CurrentChar == PropertyBagElement.ClosingMarker) 
        {
            _delimStack.Pop();
            return true;
        }

        return ElementClosing();
    }

    internal bool ElementClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        var delimiter = _delimStack.Peek();
        int markerCount = delimiter.MarkerCount;

        if (CurrentChar == delimiter.ClosingMarker) {
            if (Peek == Element.ElementClosingMarker && markerCount == 1) {
                Advance();
                Advance();
                _delimStack.Pop();
                return true;
            }

            int tmpPosition = Position;

            while (markerCount > 0 && CurrentChar == delimiter.ClosingMarker) {
                Advance();
                --markerCount;

                if (delimiter.Style == ElementStyle.Minimized) {
                    if (markerCount == 0) {
                        _delimStack.Pop();
                        return true;
                    }
                }
                else if (CurrentChar == Element.ElementClosingMarker && markerCount == 0) {
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

    private bool IsNumericChar(char c) => 
        char.IsNumber(c) || 
        char.IsBetween(c, 'A', 'F') || 
        char.IsBetween(c, 'a', 'f') || 
        c == '$' || c == '%' || c == '.' || c == ',';

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
            if (CurrentChar.IsKeywordLeadingChar()) {
                var keyValuePairElement = ParseKeyValuePairElement();
                SkipWhitespace();
                return keyValuePairElement;
            }

            if (ElementOpening(StringElement.ElementDelimiter, out int stringMarkerCount)) {
                var stringElement = ParseStringElement(stringMarkerCount);
                SkipWhitespace();
                return stringElement;
            }

            if (ElementOpening(EvaluatedElement.ElementDelimiter, out int evalMarkerCount)) {
                var literalElement = ParseEvaluatedElement(evalMarkerCount);
                SkipWhitespace();
                return literalElement;
            }

            if (ElementOpening(CharacterElement.ElementDelimiter, out int charMarkerCount)) {
                var characterElement = ParseCharacterElement();
                SkipWhitespace();
                return characterElement;
            }

            if (ElementOpening(PropertyBagElement.ElementDelimiter, out int propMarkerCount)) {
                var propertyBagElement = ParsePropertyBagElement(propMarkerCount);
                SkipWhitespace();
                return propertyBagElement;
            }

            if (ElementOpening(MetadataElement.ElementDelimiter, out int metaMarkerCount)) {
                var metadataElement = ParseMetadataElement(metaMarkerCount);
                SkipWhitespace();
                return metadataElement;
            }

            if (ElementOpening(ObjectElement.ElementDelimiter, out int objMarkerCount)) {
                var objectElement = ParseObjectElement(objMarkerCount);
                SkipWhitespace();
                return objectElement;
            }

            if (ElementOpening(ArrayElement.ElementDelimiter, out int arrayMarkerCount)) {
                var arrayElement = ParseArrayElement(arrayMarkerCount);
                SkipWhitespace();
                return arrayElement;
            }

            if (ElementOpening(IntegerElement.ElementDelimiter, out int intMarkerCount)) {
                var integerElement = ParseIntegerElement(intMarkerCount);
                SkipWhitespace();
                return integerElement;
            }

            if (ElementOpening(LongElement.ElementDelimiter, out int longMarkerCount)) {
                var longIntegerElement = ParseLongIntegerElement(longMarkerCount);
                SkipWhitespace();
                return longIntegerElement;
            }

            if (ElementOpening(DecimalElement.ElementDelimiter, out int decMarkerCount)) {
                var decimalElement = ParseDecimalElement(decMarkerCount);
                SkipWhitespace();
                return decimalElement;
            }

            if (ElementOpening(DoubleElement.ElementDelimiter, out int doubleMarkerCount)) {
                var doubleElement = ParseDoubleElement(doubleMarkerCount);
                SkipWhitespace();
                return doubleElement;
            }

            if (ElementOpening(BooleanElement.ElementDelimiter, out int boolMarkerCount)) {
                var booleanElement = ParseBooleanElement(boolMarkerCount);
                SkipWhitespace();
                return booleanElement;
            }

            if (ElementOpening(DateElement.ElementDelimiter, out int dateMarkerCount)) {
                var dateElement = ParseDateElement(dateMarkerCount);
                SkipWhitespace();
                return dateElement;
            }

            if (ElementOpening(PlaceholderElement.ElementDelimiter, out int phMarkerCount)) {
                var placeholderElement = ParsePlaceholderElement(phMarkerCount);
                SkipWhitespace();
                return placeholderElement;
            }

            if (ElementOpening(NullElement.ElementDelimiter, out int nullMarkerCount)) {
                var nullElement = ParseNullElement(nullMarkerCount);
                SkipWhitespace();
                return nullElement;
            }

            if (ElementOpening(CommentElement.ElementDelimiter, out int commentMarkerCount)) {
                // Parse comment but don't return it, as comments are not part of the logical output.
                ParseCommentElement(commentMarkerCount);
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
                throw new InvalidOperationException($"Unexpected element type at row {CurrentRow}, column {CurrentColumn}.");
            }
        }

        throw new InvalidOperationException($"Unexpected end of {MetadataElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private ArrayElement ParseArrayElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;
        SkipWhitespace();

        if (IsCharAvailable()) {
            var element = ParseElement();

            /* This looks and feels kind of dirty, for some reason, yet it does the job. Sure, I could put this in a 
            dictionary or make a factory, but that just moves the ugliness around. */

            ArrayElement arrayElement = element switch {
                IntegerElement => new TypedArrayElement<IntegerElement>(style: style),
                LongElement => new TypedArrayElement<LongElement>(style: style),
                DecimalElement => new TypedArrayElement<DecimalElement>(style: style),
                DoubleElement => new TypedArrayElement<DoubleElement>(style: style),
                BooleanElement => new TypedArrayElement<BooleanElement>(style: style),
                DateElement => new TypedArrayElement<DateElement>(style: style),
                TextElement => new TypedArrayElement<TextElement>(style: style),
                CharacterElement => new TypedArrayElement<CharacterElement>(style: style),
                KeyValuePairElement => new TypedArrayElement<KeyValuePairElement>(style: style),
                ObjectElement => new TypedArrayElement<ObjectElement>(style: style),
                MetadataElement => new TypedArrayElement<MetadataElement>(style: style),
                PropertyBagElement => new TypedArrayElement<PropertyBagElement>(style: style),
                _ => new TypedArrayElement<NullElement>(style: style)
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

    private NullElement ParseNullElement(int markerCount) {
        while (IsCharAvailable()) {
            if (ElementMinClosing()) {
                return new NullElement();
            }
        }

        throw new InvalidOperationException($"Unexpected end of {NullElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
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
                if (!objectElement.Add(kvp)) {
                    /* TODO: The row and column are wrong here. The parser has already moved past the key/value pair. 
                    Set up a stack of row/column positions to track recent elements. */
                    throw new InvalidOperationException($"Duplicate key '{kvp.Key}' in object at row {CurrentRow}, column {CurrentColumn}.");
                }
            }
            else {
                throw new InvalidOperationException($"Unexpected element type at row {CurrentRow}, column {CurrentColumn}.");
            }
        }

        throw new InvalidOperationException($"Unexpected end of {ObjectElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private PropertyBagElement ParsePropertyBagElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;
        SkipWhitespace();
        var propBagElement = new PropertyBagElement(style);

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
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementMinClosing()) {
                var variable = valueBuilder.ToString().Normalize(NormalizationForm.FormC);

                if (string.IsNullOrEmpty(variable)) {
                    throw new InvalidOperationException($"Placeholder variable must be a non-empty string at row {CurrentRow}, column {CurrentColumn}..");
                }

                var value = Environment.GetEnvironmentVariable(variable);
                return new PlaceholderElement(value ?? string.Empty, markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {PlaceholderElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private KeyValuePairElement ParseKeyValuePairElement() {
        while (Peek.IsKeywordChar()) {
            Expand();
        }

        if (string.IsNullOrEmpty(CurrentString)) {
            throw new InvalidOperationException("Key must be a non-empty string.");
        }

        var keyElement = new KeywordElement(CurrentString, style: ElementStyle.Bare);
        Advance();
        SkipWhitespace();

        var keyValuePairElement = new KeyValuePairElement(keyElement);

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
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder charContent = new();

        while (IsCharAvailable() && !ElementMinClosing()) {
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
        return new CharacterElement(character, markerCount, style: style);
    }

    private EvaluatedElement ParseEvaluatedElement(int markerCount = 1) {
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementMaxOpening(StringElement.ElementDelimiter, out int stringMarkerCount)) {
                StringElement stringElement = ParseStringElement(stringMarkerCount);
                valueBuilder.Append(stringElement.Value);
                continue;
            }

            if (ElementMaxOpening(CharacterElement.ElementDelimiter, out int charMarkerCount)) {
                CharacterElement characterElement = ParseCharacterElement(charMarkerCount);
                valueBuilder.Append(characterElement.Value);
                continue;
            }

            if (ElementMaxOpening(IntegerElement.ElementDelimiter, out int intMarkerCount)) {
                IntegerElement integerElement = ParseIntegerElement(intMarkerCount);
                valueBuilder.Append(integerElement.Value);
                continue;
            }

            if (ElementMaxOpening(LongElement.ElementDelimiter, out int longMarkerCount)) {
                LongElement longElement = ParseLongIntegerElement(longMarkerCount);
                valueBuilder.Append(longElement.Value);
                continue;
            }

            if (ElementMaxOpening(DecimalElement.ElementDelimiter, out int decMarkerCount)) {
                DecimalElement decimalElement = ParseDecimalElement(decMarkerCount);
                valueBuilder.Append(decimalElement.Value);
                continue;
            }

            if (ElementMaxOpening(DoubleElement.ElementDelimiter, out int doubleMarkerCount)) {
                DoubleElement doubleElement = ParseDoubleElement(doubleMarkerCount);
                valueBuilder.Append(doubleElement.Value);
                continue;
            }

            if (ElementMaxOpening(BooleanElement.ElementDelimiter, out int boolMarkerCount)) {
                BooleanElement booleanElement = ParseBooleanElement(boolMarkerCount);
                valueBuilder.Append(booleanElement.Value);
                continue;
            }

            if (ElementMaxOpening(DateElement.ElementDelimiter, out int dateMarkerCount)) {
                DateElement dateElement = ParseDateElement(dateMarkerCount);
                valueBuilder.Append(dateElement.Value);
                continue;
            }

            if (ElementMaxOpening(EvaluatedElement.ElementDelimiter, out int evalMarkerCount)) {
                EvaluatedElement evaluatedElement = ParseEvaluatedElement(evalMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMaxOpening(PlaceholderElement.ElementDelimiter, out int phMarkerCount)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement(phMarkerCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMaxOpening(CommentElement.ElementDelimiter, out int commentMarkerCount)) {
                ParseCommentElement();
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
        var style = _delimStack.Peek().Style;
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementClosing()) {
                return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {StringElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DateElement ParseDateElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMinClosing()) {
                var value = valueBuilder.ToString();
                return new DateElement(value, markerCount, style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DateElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }        
    
    private IntegerElement ParseIntegerElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMinClosing()) {
                var value = ParseNumericValue<int>(valueBuilder.ToString());
                return new IntegerElement(value, markerCount, style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {IntegerElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }


    private LongElement ParseLongIntegerElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMinClosing()) {
                var value = ParseNumericValue<long>(valueBuilder.ToString());
                return new LongElement(value, markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {LongElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DecimalElement ParseDecimalElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMinClosing()) {
                var value = ParseNumericValue<decimal>(valueBuilder.ToString());
                return new DecimalElement(value, markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DecimalElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DoubleElement ParseDoubleElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementMinClosing()) {
                var value = ParseNumericValue<double>(valueBuilder.ToString());
                return new DoubleElement(value, markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DoubleElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private BooleanElement ParseBooleanElement(int markerCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Minimized) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (ElementOpening(PlaceholderElement.ElementDelimiter)) {
                PlaceholderElement evaluatedElement = ParsePlaceholderElement();
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }
            
            if (ElementMinClosing() || !char.IsAsciiLetter(CurrentChar)) {
                string valueString = valueBuilder.ToString().ToLower();
                bool value = valueString switch {
                    "true" => true,
                    "false" => false,
                    "" => false,
                    _ => throw new InvalidOperationException($"Invalid boolean value '{valueString}' at row {CurrentRow}, column {CurrentColumn}.")
                };

                return new BooleanElement(value, markerCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {BooleanElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private T ParseNumericValue<T>(string valueString) where T : struct, IConvertible {
        if (string.IsNullOrEmpty(valueString)) {
            // throw new ArgumentException("The numeric value string cannot be null or empty.", nameof(valueString));
            return default;
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
            switch (numberBase) {
                case 10: {
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
                case 16: {
                    long hexValue = long.Parse(numberString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    return (T)Convert.ChangeType(hexValue, typeof(T));
                }
                case 2: {
                    long binaryValue = long.Parse(numberString, NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
                    return (T)Convert.ChangeType(binaryValue, typeof(T));
                }
                default: {
                    throw new InvalidOperationException($"Unsupported numeric base '{numberBase}' at row {CurrentRow}, column {CurrentColumn}.");
                }
            }
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"Failed to parse numeric value '{valueString}' at row {CurrentRow}, column {CurrentColumn}. Error: {ex.Message}");
        }
    }
}
