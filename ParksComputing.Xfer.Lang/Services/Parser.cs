using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Diagnostics;

using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Schema;

namespace ParksComputing.Xfer.Lang.Services;

/* This parser is ROUGH. I'm trying out a lot of ideas, some of them supported in parallel. Once I
settle on a solid grammar, I'll redo the parser or use some kind of tool to generate it. */

public class Parser : IXferParser {
    // Used to assign IDs from inline PIs to subsequent elements
    private Queue<string> _pendingIds = new Queue<string>();
    private ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver _dynamicSourceResolver = new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();
    private ParksComputing.Xfer.Lang.Deserialization.IDeserializationInstructionResolver _deserializationInstructionResolver = new ParksComputing.Xfer.Lang.Deserialization.DefaultDeserializationInstructionResolver();

    public ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver DynamicSourceResolver {
        get => _dynamicSourceResolver;
        set => _dynamicSourceResolver = value ?? new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();
    }

    public ParksComputing.Xfer.Lang.Deserialization.IDeserializationInstructionResolver DeserializationInstructionResolver {
        get => _deserializationInstructionResolver;
        set => _deserializationInstructionResolver = value ?? new ParksComputing.Xfer.Lang.Deserialization.DefaultDeserializationInstructionResolver();
    }

    private XferDocument? _currentDocument = null;

    private Dictionary<string, int> _pendingCharDefs = new();

    public static readonly string Version = "0.11";

    // Call this after parsing metadata PIs
    private void ApplyCharDefPI() {
        if (_pendingCharDefs.Count > 0) {
            ParksComputing.Xfer.Lang.Services.CharacterIdRegistry.SetCustomIds(_pendingCharDefs);
        }
    }

    // Example stub: call this when you parse a charDef PI
    private void ParseCharDefPI(KeyValuePairElement piKv) {
        // piKv.Value should be an ObjectElement or similar containing mappings
        if (piKv.Value is ObjectElement obj) {
            foreach (var kv in obj.Values.Values) {
                var name = kv.KeyElement.ToString();

                if (kv.Value is CharacterElement charElem) {
                    _pendingCharDefs[name] = charElem.Value;
                }
                else {
                    throw new InvalidOperationException($"charDef PI expects a character element for key '{name}' at row {LastElementRow}, column {LastElementColumn}. Found: {kv.Value.GetType().Name}");
                }
            }
        }
    }

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

    private int LastElementRow { get; set; }
    private int LastElementColumn { get; set; }

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

        if (CurrentChar == Element.ElementOpeningCharacter && Peek == CommentElement.OpeningSpecifier) {
            int specifierCount = 1;
            ++Position;
            UpdateRowColumn();
            ++Position;
            UpdateRowColumn();

            while (CurrentChar == CommentElement.OpeningSpecifier) {
                ++specifierCount;
                ++Position;
                UpdateRowColumn();
            }

            while (IsCharAvailable()) {
                if (CurrentChar == CommentElement.ClosingSpecifier) {
                    int tmpSpecifierCount = specifierCount;
                    --specifierCount;

                    if (Peek == Element.ElementClosingCharacter && specifierCount == 0) {
                        ++Position;
                        UpdateRowColumn();
                        ++Position;
                        UpdateRowColumn();
                        break;
                    }

                    bool commentClosed = false;

                    while (specifierCount > 0 && Peek == CommentElement.ClosingSpecifier) {
                        ++Position;
                        UpdateRowColumn();
                        --specifierCount;

                        if (Peek == Element.ElementClosingCharacter && specifierCount == 0) {
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
                        specifierCount = tmpSpecifierCount;
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

    internal bool KeywordElementOpening(out int specifierCount) {
        return ElementOpening(KeywordElement.ElementDelimiter, out specifierCount);
    }

    internal bool IdentifierElementOpening(out int specifierCount) {
        if (IdentifierElement.IsIdentifierLeadingChar(CurrentChar)) {
            specifierCount = 1;
            LastElementRow = CurrentRow;
            LastElementColumn = CurrentColumn;
            _delimStack.Push(new ElementDelimiter(IdentifierElement.OpeningSpecifier, IdentifierElement.ClosingSpecifier, specifierCount, ElementStyle.Implicit));
            return true;
        }

        return ElementOpening(IdentifierElement.ElementDelimiter, out specifierCount);
    }

    internal bool IntegerElementOpening(out int specifierCount) {
        if (CurrentChar.IsIntegerLeadingChar()) {
            specifierCount = 1;
            LastElementRow = CurrentRow;
            LastElementColumn = CurrentColumn;
            _delimStack.Push(new ElementDelimiter(IntegerElement.OpeningSpecifier, IntegerElement.ClosingSpecifier, specifierCount, ElementStyle.Implicit));
            return true;
        }

        return ElementOpening(IntegerElement.ElementDelimiter, out specifierCount);
    }

    internal bool ElementOpening(ElementDelimiter delimiter) {
        return ElementOpening(delimiter, out int _);
    }

    internal bool ElementOpening(ElementDelimiter delimiter, out int specifierCount) {
        char openingSpecifier = delimiter.OpeningSpecifier;
        char closingSpecifier = delimiter.ClosingSpecifier;

        if (ElementExplicitOpening(delimiter, out specifierCount)) {
            return true;
        }

        specifierCount = 1;

        if (CurrentChar == openingSpecifier) {
            int saveCurrentRow = CurrentRow;
            int saveCurrentColumn = CurrentColumn;
            int tmpPosition = Position;
            Advance();

            while (CurrentChar == openingSpecifier) {
                ++specifierCount;
                Advance();
            }

            if (CurrentChar != Element.ElementClosingCharacter) {
                _delimStack.Push(new ElementDelimiter(openingSpecifier, closingSpecifier, specifierCount, ElementStyle.Compact));
                LastElementRow = saveCurrentRow;
                LastElementColumn = saveCurrentColumn;
                return true;
            }

            Position = tmpPosition;
        }

        return false;
    }

    internal bool ElementExplicitOpening(ElementDelimiter delimiter, out int specifierCount) {
        char openingSpecifier = delimiter.OpeningSpecifier;
        char closingSpecifier = delimiter.ClosingSpecifier;

        specifierCount = 1;

        if (CurrentChar == Element.ElementOpeningCharacter && Peek == openingSpecifier) {
            int saveCurrentRow = CurrentRow;
            int saveCurrentColumn = CurrentColumn;
            Advance();
            Advance();

            /* This is really ugly to me, and it seems I'm missing a more elegant way to parse this. What I'm
            doing here is handling empty elements, like <""> and <##>. */
            if (CurrentChar == closingSpecifier && Peek == Element.ElementClosingCharacter) {
                _delimStack.Push(new ElementDelimiter(openingSpecifier, closingSpecifier, specifierCount, ElementStyle.Explicit));
                LastElementRow = saveCurrentRow;
                LastElementColumn = saveCurrentColumn;
                return true;
            }

            while (CurrentChar == openingSpecifier) {
                ++specifierCount;
                Advance();
            }

            _delimStack.Push(new ElementDelimiter(openingSpecifier, closingSpecifier, specifierCount, ElementStyle.Explicit));
            LastElementRow = saveCurrentRow;
            LastElementColumn = saveCurrentColumn;
            return true;
        }

        return false;
    }

    internal bool MetadataElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == MetadataElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool StringElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == StringElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool IdentifierElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == IdentifierElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool KeywordElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == KeywordElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool CharacterElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == CharacterElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool EvaluatedElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == InterpolatedElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool PlaceholderElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == DynamicElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool ArrayElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == ArrayElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool ObjectElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == ObjectElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool TupleElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == TupleElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool NullElementClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        var delimiter = _delimStack.Peek();
        var style = delimiter.Style;
        var closingSpecifier = delimiter.ClosingSpecifier;

        Debug.Assert(closingSpecifier == NullElement.ClosingSpecifier);

        if (style == ElementStyle.Compact && (
               CurrentChar.IsWhiteSpace()
            || CurrentChar.IsKeywordChar()
            || CurrentChar.IsIntegerLeadingChar()
            || CurrentChar.IsElementOpeningCharacter()
            || CurrentChar.IsElementOpeningSpecifier()
            || CurrentChar.IsCollectionClosingSpecifier()
            )) {
            _delimStack.Pop();
            return true;
        }

        return ElementClosing();
    }

    internal bool DateElementClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        var delimiter = _delimStack.Peek();
        var style = delimiter.Style;
        var closingSpecifier = delimiter.ClosingSpecifier;

        Debug.Assert(closingSpecifier == DateTimeElement.ClosingSpecifier);

        if (style == ElementStyle.Compact) {
            if (CurrentChar == closingSpecifier) {
                Advance();
                _delimStack.Pop();
                return true;
            }
        }

        return ElementClosing();
    }

    internal bool IntegerElementClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        var delimiter = _delimStack.Peek();
        var style = delimiter.Style;
        var closingSpecifier = delimiter.ClosingSpecifier;

        Debug.Assert(closingSpecifier == IntegerElement.ClosingSpecifier);

        if (style is ElementStyle.Compact or ElementStyle.Implicit) {
            if (CurrentChar.IsWhiteSpace()) {
                Advance();
                _delimStack.Pop();
                return true;
            }

            if (CurrentChar.IsElementOpeningCharacter()
                || CurrentChar.IsElementOpeningSpecifier()
                || CurrentChar.IsCollectionClosingSpecifier()
                ) {
                _delimStack.Pop();
                return true;
            }
        }

        return ElementClosing();
    }

    internal bool LongElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == LongElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool DecimalElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == DecimalElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool DoubleElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == DoubleElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool BooleanElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == BooleanElement.ClosingSpecifier);
        return ElementCompactClosing() || !char.IsAsciiLetter(CurrentChar);
    }

    internal bool CommentElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == CommentElement.ClosingSpecifier);
        return ElementClosing();
    }

    internal bool ElementCompactClosing() {
        if (_delimStack.Count == 0) {
            return false;
        }

        var style = _delimStack.Peek().Style;
        var closingSpecifier = _delimStack.Peek().ClosingSpecifier;

        if ((style == ElementStyle.Compact || style == ElementStyle.Implicit) && (
               CurrentChar.IsWhiteSpace()
            || CurrentChar == closingSpecifier
            || CurrentChar.IsElementOpeningCharacter()
            || CurrentChar.IsElementOpeningSpecifier()
            || CurrentChar.IsCollectionClosingSpecifier()
            )) {
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
        int specifierCount = delimiter.SpecifierCount;

        if (CurrentChar == delimiter.ClosingSpecifier) {
            if (Peek == Element.ElementClosingCharacter && specifierCount == 1) {
                Advance();
                Advance();
                _delimStack.Pop();
                return true;
            }

            int tmpPosition = Position;

            while (specifierCount > 0 && CurrentChar == delimiter.ClosingSpecifier) {
                Advance();
                --specifierCount;

                if (specifierCount == 0) {
                    if (delimiter.Style == ElementStyle.Compact) {
                        _delimStack.Pop();
                        return true;
                    }
                    else if (CurrentChar == Element.ElementClosingCharacter) {
                        Advance();
                        _delimStack.Pop();
                        return true;
                    }
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

        // Handle empty input
        if (CurrentChar == '\0') {
            return new XferDocument();
        }

        // Skip BOM if present
        SkipBOM();

        // Parse the document using the robust main loop
        var document = ParseDocument();

        // Validate if schema validator is present
        if (document is { } && _validator is { }) {
            _validator.Validate(document.Root);
        }
        return document!;
    }

    internal XferDocument ParseDocument() {
        var document = new XferDocument();
        _currentDocument = document;
        while (IsCharAvailable()) {
            var element = ParseElement();
            // Handle PI and metadata elements
            if (element is ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement) {
                if (!document.MetadataCollection.Contains(element)) {
                    document.MetadataCollection.Add(element);
                }
                continue;
            }
            else if (element is MetadataElement metaElem) {
                if (!document.MetadataCollection.Contains(metaElem)) {
                    document.MetadataCollection.Add(metaElem);
                }
                continue;
            }

            // Assign pending ID to the next real element only
            if (element is not EmptyElement) {
                if (_pendingIds.Count > 0) {
                    element.Id = _pendingIds.Dequeue();
                }
                document.Root.Add(element);
            }
        }
        return document;
    }

    internal Element ParseElement() {
        SkipWhitespace();

        // Inline MetadataElement support: <! ... !>
        if (ElementOpening(MetadataElement.ElementDelimiter, out int metaSpecifierCount)) {
            var metadataElement = ParseMetadataElement(metaSpecifierCount);

            // Add PI elements and inline metadata to document's metadata collection for global visibility
            if (_currentDocument != null) {
                if ((metadataElement is ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement ||
                     metadataElement is MetadataElement) &&
                    !_currentDocument.MetadataCollection.Contains(metadataElement)) {
                    _currentDocument.MetadataCollection.Add(metadataElement);
                }
            }

            SkipWhitespace();
            // Do NOT parse the next element here; let the main loop handle it and assign the ID
            return new EmptyElement();
        }

        while (IsCharAvailable()) {
            Element? result = null;
            if (IdentifierElementOpening(out int identifierSpecifierCount)) {
                result = ParseIdentifierElement(identifierSpecifierCount);
            }
            else if (KeywordElementOpening(out int keywordSpecifierCount)) {
                result = ParseKeywordElement(keywordSpecifierCount);
            }
            else if (ElementOpening(StringElement.ElementDelimiter, out int stringSpecifierCount)) {
                result = ParseStringElement(stringSpecifierCount);
            }
            else if (ElementOpening(InterpolatedElement.ElementDelimiter, out int evalSpecifierCount)) {
                result = ParseEvaluatedElement(evalSpecifierCount);
            }
            else if (ElementOpening(CharacterElement.ElementDelimiter, out int charSpecifierCount)) {
                result = ParseCharacterElement();
            }
            else if (ElementOpening(TupleElement.ElementDelimiter, out int propSpecifierCount)) {
                result = ParseTupleElement(propSpecifierCount);
            }
            else if (ElementOpening(MetadataElement.ElementDelimiter, out int metaSpecifierCount2)) {
                var metadataElement2 = ParseMetadataElement(metaSpecifierCount2);
                result = metadataElement2;
            }
            else if (ElementOpening(ObjectElement.ElementDelimiter, out int objSpecifierCount)) {
                result = ParseObjectElement(objSpecifierCount);
            }
            else if (ElementOpening(ArrayElement.ElementDelimiter, out int arraySpecifierCount)) {
                result = ParseArrayElement(arraySpecifierCount);
            }
            else if (IntegerElementOpening(out int intSpecifierCount)) {
                result = ParseIntegerElement(intSpecifierCount);
            }
            else if (ElementOpening(LongElement.ElementDelimiter, out int longSpecifierCount)) {
                result = ParseLongIntegerElement(longSpecifierCount);
            }
            else if (ElementOpening(DecimalElement.ElementDelimiter, out int decSpecifierCount)) {
                result = ParseDecimalElement(decSpecifierCount);
            }
            else if (ElementOpening(DoubleElement.ElementDelimiter, out int doubleSpecifierCount)) {
                result = ParseDoubleElement(doubleSpecifierCount);
            }
            else if (ElementOpening(BooleanElement.ElementDelimiter, out int boolSpecifierCount)) {
                result = ParseBooleanElement(boolSpecifierCount);
            }
            else if (ElementOpening(DateTimeElement.ElementDelimiter, out int dateSpecifierCount)) {
                result = ParseDateElement(dateSpecifierCount);
            }
            else if (ElementOpening(DynamicElement.ElementDelimiter, out int phSpecifierCount)) {
                result = ParsePlaceholderElement(phSpecifierCount);
            }
            else if (ElementOpening(NullElement.ElementDelimiter, out int nullSpecifierCount)) {
                result = ParseNullElement(nullSpecifierCount);
            }
            else if (ElementOpening(CommentElement.ElementDelimiter, out int commentSpecifierCount)) {
                /* Parse comment but don't return it, as comments are not part of the logical output. */
                ParseCommentElement(commentSpecifierCount);
                SkipWhitespace();
                continue;
            }
            else {
                throw new InvalidOperationException($"Expected element at row {CurrentRow}, column {CurrentColumn}.");
            }
            SkipWhitespace();
            // Only assign pending ID to the very next real element, then stop
            if (result is not null && result is not EmptyElement && _pendingIds.Count > 0) {
                result.Id = _pendingIds.Dequeue();
            }
            return result!;
        }
        return new EmptyElement();
    }

    private void ParseCommentElement(int specifierCount = 1) {
        while (IsCharAvailable()) {
            if (CommentElementClosing()) {
                break;
            }

            Advance();
        }
    }

    XferSchemaValidator? _validator = null;

    private MetadataElement ParseMetadataElement(int specifierCount = 1) {
        SkipWhitespace();
        var kvps = new List<KeyValuePairElement>();
        while (IsCharAvailable()) {
            if (MetadataElementClosing()) {
                // Check for PI keywords
                foreach (var piKvp in kvps) {
                    var keyLower = piKvp.Key.ToLowerInvariant();
                    if (ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.KnownPIKeywords.Contains(keyLower)) {
                        // Instantiate the correct PI element type
                        if (keyLower == ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.CharDefKeyword) {
                            var pi = new ParksComputing.Xfer.Lang.Elements.CharDefPIElement();
                            ParseCharDefPI(piKvp);
                            foreach (var k in kvps) {
                                pi.Add(k);
                            }
                            ApplyCharDefPI();
                            return pi;
                        }

                        if (keyLower == ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.DeserializeKeyword) {
                            var pi = new ParksComputing.Xfer.Lang.Elements.DeserializePIElement();
                            foreach (var k in kvps) {
                                pi.Add(k);
                            }
                            return pi;
                        }

                        if (keyLower == ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.IncludeKeyword) {
                            var pi = new ParksComputing.Xfer.Lang.Elements.IncludePIElement();
                            foreach (var k in kvps) {
                                pi.Add(k);
                            }
                            return pi;
                        }
                    }
                }
                // If no PI keyword, return regular MetadataElement
                var metadataElement = new MetadataElement();

                foreach (var k in kvps) {
                    metadataElement.Add(k);
                }

                if (metadataElement.ContainsKey(ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.IdKeyword)) {
                    if (metadataElement.Values[ParksComputing.Xfer.Lang.Elements.ProcessingInstructionElement.IdKeyword].Value is ParksComputing.Xfer.Lang.Elements.TextElement idValue) {
                        _pendingIds.Enqueue(idValue.Value);
                    }
                }

                // Schema validation logic
                if (metadataElement.ContainsKey(MetadataElement.SchemaKeyword)) {
                    var schemaElement = metadataElement[MetadataElement.SchemaKeyword];
                    if (schemaElement.Value is ObjectElement schemaObject) {
                        var schemaParser = new XferSchemaParser();
                        var schemaObjects = schemaParser.ParseSchema(schemaObject);
                        if (schemaObjects is { }) {
                            _validator = new XferSchemaValidator(schemaObjects);
                        }
                    }
                }
                return metadataElement;
            }
            var lastRow = CurrentRow;
            var lastColumn = CurrentColumn;
            var element = ParseElement();
            if (element is KeyValuePairElement kvp) {
                // Check for duplicate keys
                if (kvps.Exists(x => x.Key == kvp.Key)) {
                    throw new InvalidOperationException($"Duplicate key '{kvp.Key}' in object at row {lastRow}, column {lastColumn}.");
                }
                kvps.Add(kvp);
            }
            else {
                throw new InvalidOperationException($"Unexpected element type at row {lastRow}, column {lastColumn}.");
            }
        }
        throw new InvalidOperationException($"Unexpected end of {MetadataElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private ArrayElement ParseArrayElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        SkipWhitespace();


        if (IsCharAvailable()) {
            if (ArrayElementClosing()) {
                return new TypedArrayElement<NullElement>(style: style);
            }

            var element = ParseElement();

            /* This looks and feels kind of dirty, for some reason, yet it does the job. Sure, I could put this in a
            dictionary or make a factory, but that just moves the ugliness around. */

            ArrayElement arrayElement = element switch {
                IntegerElement => new TypedArrayElement<IntegerElement>(style: style),
                LongElement => new TypedArrayElement<LongElement>(style: style),
                DecimalElement => new TypedArrayElement<DecimalElement>(style: style),
                DoubleElement => new TypedArrayElement<DoubleElement>(style: style),
                BooleanElement => new TypedArrayElement<BooleanElement>(style: style),
                DateTimeElement => new TypedArrayElement<DateTimeElement>(style: style),
                TextElement => new TypedArrayElement<TextElement>(style: style),
                CharacterElement => new TypedArrayElement<CharacterElement>(style: style),
                KeyValuePairElement => new TypedArrayElement<KeyValuePairElement>(style: style),
                ObjectElement => new TypedArrayElement<ObjectElement>(style: style),
                MetadataElement => new TypedArrayElement<MetadataElement>(style: style),
                TupleElement => new TypedArrayElement<TupleElement>(style: style),
                _ => new TypedArrayElement<NullElement>(style: style)
            };

            arrayElement.Add(element);

            while (IsCharAvailable()) {
                if (ArrayElementClosing()) {
                    return arrayElement;
                }

                element = ParseElement();
                arrayElement.Add(element);
            }

        }

        throw new InvalidOperationException($"Unexpected end of {ArrayElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private Element ParseNullElement(int specifierCount) {
        while (IsCharAvailable()) {
            if (NullElementClosing()) {
                return new NullElement();
            }
        }

        throw new InvalidOperationException($"Unexpected end of {NullElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private ObjectElement ParseObjectElement(int specifierCount = 1) {
        SkipWhitespace();
        var objectElement = new ObjectElement();

        while (IsCharAvailable()) {
            if (ObjectElementClosing()) {
                return objectElement;
            }

            var lastRow = CurrentRow;
            var lastColumn = CurrentColumn;
            var element = ParseElement();

            if (element is KeyValuePairElement kvp) {
                if (!objectElement.Add(kvp)) {
                    throw new InvalidOperationException($"Duplicate key '{kvp.Key}' in object at row {lastRow}, column {lastColumn}.");
                }
            }
            else {
                throw new InvalidOperationException($"Unexpected element type at row {lastRow}, column {lastColumn}.");
            }
        }

        throw new InvalidOperationException($"Unexpected end of {ObjectElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private TupleElement ParseTupleElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        SkipWhitespace();
        var tupleElement = new TupleElement(style);

        while (IsCharAvailable()) {
            if (TupleElementClosing()) {
                return tupleElement;
            }

            var element = ParseElement();
            tupleElement.Add(element);
        }

        throw new InvalidOperationException($"Unexpected end of {TupleElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DynamicElement ParsePlaceholderElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (PlaceholderElementClosing()) {
                var variable = valueBuilder.ToString().Normalize(NormalizationForm.FormC);

                if (string.IsNullOrEmpty(variable)) {
                    throw new InvalidOperationException($"Placeholder variable must be a non-empty string at row {CurrentRow}, column {CurrentColumn}.");
                }

                string? value = null;
                if (_currentDocument != null) {
                    value = _dynamicSourceResolver.Resolve(variable, _currentDocument);
                }
                return new DynamicElement(value ?? string.Empty, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {DynamicElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private KeyValuePairElement ParseKeywordElement(int specifierCount = 1) {
        var lastRow = CurrentRow;
        var lastColumn = CurrentColumn;
        var style = _delimStack.Peek().Style;
        var key = string.Empty;

        if (style == ElementStyle.Implicit) {
            while (Peek.IsKeywordChar()) {
                Expand();
            }

            if (string.IsNullOrEmpty(CurrentString)) {
                throw new InvalidOperationException($"Key must be a non-empty string at row {lastRow}, column {lastColumn}.");
            }

            key = CurrentString;
            Advance();
            SkipWhitespace();
            _delimStack.Pop();
        }
        else {
            StringBuilder valueBuilder = new StringBuilder();

            while (IsCharAvailable()) {
                if (KeywordElementClosing()) {
                    key = valueBuilder.ToString().Normalize(NormalizationForm.FormC);
                    break;
                }

                valueBuilder.Append(CurrentChar);
                Expand();
            }
        }

        var keyElement = new KeywordElement(key, specifierCount, style: style);
        return ParseKeyValuePairElement(keyElement);
    }

    private KeyValuePairElement ParseIdentifierElement(int specifierCount = 1) {
        var lastRow = CurrentRow;
        var lastColumn = CurrentColumn;
        var style = _delimStack.Peek().Style;
        var key = string.Empty;

        if (style == ElementStyle.Implicit) {
            while (Peek.IsKeywordChar()) {
                Expand();
            }

            if (string.IsNullOrEmpty(CurrentString)) {
                throw new InvalidOperationException($"Key must be a non-empty string at row {lastRow}, column {lastColumn}.");
            }

            key = CurrentString;
            Advance();
            SkipWhitespace();
            _delimStack.Pop();
        }
        else {
            StringBuilder valueBuilder = new StringBuilder();

            while (IsCharAvailable()) {
                if (IdentifierElementClosing()) {
                    key = valueBuilder.ToString().Normalize(NormalizationForm.FormC);
                    break;
                }

                valueBuilder.Append(CurrentChar);
                Expand();
            }
        }

        var keyElement = new IdentifierElement(key, specifierCount, style: style);
        return ParseKeyValuePairElement(keyElement);
    }

    private KeyValuePairElement ParseKeyValuePairElement(TextElement keyElement) {
        var keyValuePairElement = new KeyValuePairElement(keyElement);

        if (IsCharAvailable()) {
            // Assign pending ID to the KeyValuePairElement itself, if present
            string? id = null;
            if (_pendingIds.Count > 0) {
                id = _pendingIds.Dequeue();
            }
            Element valueElement = ParseElement();
            // Do NOT assign pending ID to valueElement here
            keyValuePairElement.Value = valueElement;
            keyValuePairElement.Id = id;
            return keyValuePairElement;
        }

        throw new InvalidOperationException($"Unexpected end of {KeyValuePairElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    /*
    The following two methods need a LOT of work to handle the vagaries of Unicode.
    I'm just trying to get through the basic scenarios first.
    */

    private CharacterElement ParseCharacterElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder charContent = new();

        while (IsCharAvailable() && !CharacterElementClosing()) {
            charContent.Append(CurrentChar);
            Advance();
        }

        string charString = charContent.ToString();

        if (string.IsNullOrEmpty(charString)) {
            throw new InvalidOperationException($"Empty character element at row {CurrentRow}, column {CurrentColumn}.");
        }

        if (char.IsLetter(charString[0])) {
            // Try to resolve custom character ID first
            var resolved = ParksComputing.Xfer.Lang.Services.CharacterIdRegistry.Resolve(charString);

            if (resolved != null) {
                return new CharacterElement(resolved.Value, specifierCount, style: style);
            }

            // Fallback to built-in keywords
            throw new InvalidOperationException($"Unknown character keyword '{charString}' at row {CurrentRow}, column {CurrentColumn}.");
        }

        var codePoint = ParseNumericValue<int>(charString);
        return new CharacterElement(codePoint, specifierCount, style: style);
    }

    private InterpolatedElement ParseEvaluatedElement(int specifierCount = 1) {
        StringBuilder valueBuilder = new();
        var style = _delimStack.Peek().Style;

        while (IsCharAvailable()) {
            if (ElementExplicitOpening(StringElement.ElementDelimiter, out int stringSpecifierCount)) {
                StringElement stringElement = ParseStringElement(stringSpecifierCount);
                valueBuilder.Append(stringElement.Value);
                continue;
            }

            if (ElementExplicitOpening(CharacterElement.ElementDelimiter, out int charSpecifierCount)) {
                CharacterElement characterElement = ParseCharacterElement(charSpecifierCount);
                valueBuilder.Append(char.ConvertFromUtf32(characterElement.Value));
                continue;
            }

            if (ElementExplicitOpening(IntegerElement.ElementDelimiter, out int intSpecifierCount)) {
                IntegerElement integerElement = ParseIntegerElement(intSpecifierCount);
                valueBuilder.Append(integerElement.Value);
                continue;
            }

            if (ElementExplicitOpening(LongElement.ElementDelimiter, out int longSpecifierCount)) {
                LongElement longElement = ParseLongIntegerElement(longSpecifierCount);
                valueBuilder.Append(longElement.Value);
                continue;
            }

            if (ElementExplicitOpening(DecimalElement.ElementDelimiter, out int decSpecifierCount)) {
                DecimalElement decimalElement = ParseDecimalElement(decSpecifierCount);
                valueBuilder.Append(decimalElement.Value);
                continue;
            }

            if (ElementExplicitOpening(DoubleElement.ElementDelimiter, out int doubleSpecifierCount)) {
                DoubleElement doubleElement = ParseDoubleElement(doubleSpecifierCount);
                valueBuilder.Append(doubleElement.Value);
                continue;
            }

            if (ElementExplicitOpening(BooleanElement.ElementDelimiter, out int boolSpecifierCount)) {
                BooleanElement booleanElement = ParseBooleanElement(boolSpecifierCount);
                valueBuilder.Append(booleanElement.Value);
                continue;
            }

            if (ElementExplicitOpening(DateTimeElement.ElementDelimiter, out int dateSpecifierCount)) {
                Element dateElement = ParseDateElement(dateSpecifierCount);

                valueBuilder.Append(
                    dateElement switch {
                        DateTimeElement dateTimeElement => dateTimeElement.Value,
                        DateElement dateOnlyElement => dateOnlyElement.Value,
                        TimeElement timeOnlyElement => timeOnlyElement.Value,
                        TimeSpanElement timeSpanElement => timeSpanElement.Value,
                        _ => throw new InvalidOperationException($"Unexpected element type at row {CurrentRow}, column {CurrentColumn}.")
                    }
                    );
                continue;
            }

            if (ElementExplicitOpening(InterpolatedElement.ElementDelimiter, out int evalSpecifierCount)) {
                InterpolatedElement evaluatedElement = ParseEvaluatedElement(evalSpecifierCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementExplicitOpening(DynamicElement.ElementDelimiter, out int phSpecifierCount)) {
                DynamicElement evaluatedElement = ParsePlaceholderElement(phSpecifierCount);
                valueBuilder.Append(evaluatedElement.Value);
                continue;
            }

            if (ElementExplicitOpening(CommentElement.ElementDelimiter, out int commentSpecifierCount)) {
                ParseCommentElement();
                continue;
            }

            if (ElementClosing()) {
                return new InterpolatedElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), specifierCount, style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"Unexpected end of {InterpolatedElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private StringElement ParseStringElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (StringElementClosing()) {
                return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            return new StringElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), specifierCount, style: style);
        }
        throw new InvalidOperationException($"Unexpected end of {StringElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private Element ParseDateElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            var value = valueBuilder.ToString();
            return new DateTimeElement(value, DateTimeHandling.RoundTrip, specifierCount, style);
        }
        else {
            while (IsCharAvailable()) {
                if (DateElementClosing()) {
                    var stringValue = valueBuilder.ToString();

                    if (DateTime.TryParseExact(
                        stringValue,
                        ["O", "s"],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out var dateTime)) {
                        return new DateTimeElement(dateTime, DateTimeHandling.RoundTrip, specifierCount, style);
                    }

                    if (TimeOnly.TryParseExact(stringValue, ["O", "s"], out var timeOnly)) {
                        return new TimeElement(timeOnly, DateTimeHandling.RoundTrip, specifierCount, style);
                    }

                    if (DateOnly.TryParse(stringValue, out var dateOnly)) {
                        return new DateElement(dateOnly, DateTimeHandling.RoundTrip, specifierCount, style);
                    }

                    if (TimeSpan.TryParse(stringValue, out var timeSpan)) {
                        return new TimeSpanElement(timeSpan, DateTimeHandling.RoundTrip, specifierCount, style);
                    }

                    throw new InvalidOperationException($"Invalid time string '{stringValue}'. Expected ISO 8601 format.");
                }

                valueBuilder.Append(CurrentChar);
                Expand();
            }
        }

        throw new InvalidOperationException($"Unexpected end of {DateTimeElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private IntegerElement ParseIntegerElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact or ElementStyle.Implicit) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            var value = ParseNumericValue<int>(valueBuilder.ToString());
            return new IntegerElement(value, specifierCount, style);
        }

        while (IsCharAvailable()) {
            if (IntegerElementClosing()) {
                var value = ParseNumericValue<int>(valueBuilder.ToString());
                return new IntegerElement(value, specifierCount, style);
            }
            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<int>(valueBuilder.ToString());
            return new IntegerElement(value, specifierCount, style);
        }
        throw new InvalidOperationException($"Unexpected end of {IntegerElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }


    private LongElement ParseLongIntegerElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            var value = ParseNumericValue<long>(valueBuilder.ToString());
            return new LongElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (LongElementClosing()) {
                var value = ParseNumericValue<long>(valueBuilder.ToString());
                return new LongElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<long>(valueBuilder.ToString());
            return new LongElement(value, specifierCount, style: style);
        }
        throw new InvalidOperationException($"Unexpected end of {LongElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DecimalElement ParseDecimalElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            var value = ParseNumericValue<decimal>(valueBuilder.ToString());
            return new DecimalElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (DecimalElementClosing()) {
                var value = ParseNumericValue<decimal>(valueBuilder.ToString());
                return new DecimalElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<decimal>(valueBuilder.ToString());
            return new DecimalElement(value, specifierCount, style: style);
        }
        throw new InvalidOperationException($"Unexpected end of {DecimalElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private DoubleElement ParseDoubleElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            var value = ParseNumericValue<double>(valueBuilder.ToString());
            return new DoubleElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (DoubleElementClosing()) {
                var value = ParseNumericValue<double>(valueBuilder.ToString());
                return new DoubleElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<double>(valueBuilder.ToString());
            return new DoubleElement(value, specifierCount, style: style);
        }
        throw new InvalidOperationException($"Unexpected end of {DoubleElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");
    }

    private BooleanElement ParseBooleanElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();
        string valueString = valueBuilder.ToString().ToLower();
        bool value = false;

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement evaluatedElement = ParsePlaceholderElement();
            valueBuilder.Append(evaluatedElement.Value);
            _delimStack.Pop();
            goto ReturnElement;
        }

        while (IsCharAvailable()) {
            if (BooleanElementClosing()) {
                goto ReturnElement;
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            goto ReturnElement;
        }
        throw new InvalidOperationException($"Unexpected end of {BooleanElement.ElementName} element at row {CurrentRow}, column {CurrentColumn}.");

    /* "Goto Considered Harmful" considered harmful. */
    ReturnElement:
        valueString = valueBuilder.ToString().ToLower();
        value = false;

        if (string.Equals(valueString, BooleanElement.TrueValue)) {
            value = true;
        }
        else if (string.Equals(valueString, BooleanElement.FalseValue)) {
            value = false;
        }
        else {
            throw new InvalidOperationException($"Invalid boolean value '{valueString}' at row {CurrentRow}, column {CurrentColumn}.");
        }

        return new BooleanElement(value, specifierCount, style: style);
    }

    private T ParseNumericValue<T>(string valueString) where T : struct, IConvertible {
        if (string.IsNullOrEmpty(valueString)) {
            return default;
        }

        char basePrefix = valueString[0];
        string numberString = valueString;

        /* Determine the base (decimal, hexadecimal, or binary). */
        int numberBase = basePrefix switch {
            Element.HexadecimalPrefix => 16,
            Element.BinaryPrefix => 2,
            _ => 10
        };

        if (numberBase != 10) {
            numberString = valueString.Substring(1); /* Remove base prefix character */
        }

        try {
            switch (numberBase) {
                case 10: {
                        if (typeof(T) == typeof(float)) {
                            return (T) Convert.ChangeType(float.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(double)) {
                            return (T) Convert.ChangeType(double.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(short)) {
                            return (T) Convert.ChangeType(short.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(int)) {
                            return (T) Convert.ChangeType(int.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(long)) {
                            return (T) Convert.ChangeType(long.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(decimal)) {
                            return (T) Convert.ChangeType(decimal.Parse(valueString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else {
                            throw new InvalidOperationException($"Unsupported type '{typeof(T)}' for decimal value parsing at row {CurrentRow}, column {CurrentColumn}.");
                        }
                    }
                case 16: {
                        long hexValue = long.Parse(numberString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        return (T) Convert.ChangeType(hexValue, typeof(T));
                    }
                case 2: {
                        long binaryValue = long.Parse(numberString, NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
                        return (T) Convert.ChangeType(binaryValue, typeof(T));
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
