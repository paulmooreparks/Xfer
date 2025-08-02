using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Diagnostics;

using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Schema;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang.Services;

/* This parser is ROUGH. I'm trying out a lot of ideas, some of them supported in parallel. Once I
settle on a solid grammar, I'll redo the parser or use some kind of tool to generate it. */


public class Parser : IXferParser {
    // Stack to track parent elements for context-sensitive parsing
    /// <summary>
    /// Optional: A delegate for resolving charDef keywords. Should return the codepoint for a keyword, or null if not found.
    /// </summary>
    public Func<Element, string, int?>? CharDefResolver { get; set; }
    // Used to assign IDs from inline PIs to subsequent elements
    private Queue<string> _pendingIds = new Queue<string>();
    // Used to hold a pending meta PI to attach as XferMetadata to the next element
    private ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver _dynamicSourceResolver = new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();

    // Extensible PI and element processors
    private readonly Dictionary<string, List<Action<KeyValuePairElement>>> _piProcessors = new();
    private readonly List<Action<Element>> _elementProcessors = new();

    /// <summary>
    /// Register a PI processor. Called with each ProcessingInstruction and its parent element (if any).
    /// </summary>
    public void RegisterPIProcessor(string piKey, Action<KeyValuePairElement> processor) {
        if (!_piProcessors.ContainsKey(piKey)) {
            _piProcessors[piKey] = new List<Action<KeyValuePairElement>>();
        }
        _piProcessors[piKey].Add(processor);
    }

    /// <summary>
    /// Register an element processor. Called with each element as it is constructed.
    /// </summary>
    public void RegisterElementProcessor(Action<Element> processor) => _elementProcessors.Add(processor);



    public ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver DynamicSourceResolver {
        get => _dynamicSourceResolver;
        set => _dynamicSourceResolver = value ?? new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();
    }

    private XferDocument? _currentDocument = null;

    public static readonly string Version = "0.12";


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

    internal bool ProcessingInstructionClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == ProcessingInstruction.ClosingSpecifier);
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

    internal bool InterpolatedElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == InterpolatedElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool DynamicElementClosing() {
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

        return document!;
    }

    internal XferDocument ParseDocument() {
        var document = new XferDocument();
        _currentDocument = document;

        // Create DocumentElement as the root for all top-level elements
        var docElement = new DocumentElement();
        document.Root = docElement;

        while (IsCharAvailable()) {
            var element = ParseElement();

            if (element is not EmptyElement) {
                if (element is ProcessingInstruction pi) {
                    if (pi.Name == DocumentProcessingInstruction.Keyword) {
                        if (document.Root.Children.Count > 0) {
                            throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Document metadata must be the first element.");
                        }
                    }

                    document.Root.AddChild(element);
                }
                else if (element is CollectionElement collectionElement) {
                    document.Root.AddChild(element);
                }
                else {
                    // For all other elements, add to the root
                    throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Unexpected element type '{element.GetType().Name}' at the root level.");
                }
            }
        }

        return document;
    }

    // Map MetadataElement with XferKeyword to XferDocumentMetadata (with extensions)
    private void MapToXferDocumentMetadata(ProcessingInstruction metaElem) {
        if (_currentDocument is null) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Current document is not set.");
        }

        if (_currentDocument.Metadata is not null) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Metadata has already been set for the current document.");
        }

        if (metaElem.Kvp?.Key == DocumentProcessingInstruction.Keyword) {
            var metadata = new XferMetadata();

            if (metaElem.Kvp.Value is ObjectElement obj) {
                foreach (var kv in obj.Dictionary) {
                    var key = kv.Key;
                    var value = kv.Value.Value;
                    switch (key.ToLowerInvariant()) {
                        case XferMetadata.XferKey:
                            if (value is TextElement txt1) {
                                metadata.Xfer = txt1.Value;
                            }
                            break;
                        case XferMetadata.VersionKey:
                            if (value is TextElement txt2) {
                                metadata.Version = txt2.Value;
                            }
                            break;
                        // Add more known properties here as needed
                        default:
                            metadata.Extensions[key] = value;
                            break;
                    }
                }
            }

            _currentDocument.Metadata = metadata;
        }
    }

    // Track the last PI element(s) for association
    private readonly Stack<List<ProcessingInstruction>> _piStack = new();

    internal Element ParseElement() {
        SkipWhitespace();
        _piStack.Push(new List<ProcessingInstruction>());

        while (IsCharAvailable()) {
            Element? element = null;
            // For containers, create the element, push to stack, parse, then pop
            if (IdentifierElementOpening(out int identifierSpecifierCount)) {
                element = ParseIdentifierElement(identifierSpecifierCount);
            }
            else if (KeywordElementOpening(out int keywordSpecifierCount)) {
                element = ParseKeywordElement(keywordSpecifierCount);
            }
            else if (ElementOpening(InterpolatedElement.ElementDelimiter, out int evalSpecifierCount)) {
                element = ParseInterpolatedElement(evalSpecifierCount);
            }
            else if (ElementOpening(CharacterElement.ElementDelimiter, out int charSpecifierCount)) {
                element = ParseCharacterElement();
            }
            else if (ElementOpening(TupleElement.ElementDelimiter, out int propSpecifierCount)) {
                element = ParseTupleElement(propSpecifierCount);
            }
            else if (ElementOpening(ObjectElement.ElementDelimiter, out int objSpecifierCount)) {
                element = ParseObjectElement(objSpecifierCount);
            }
            else if (ElementOpening(ArrayElement.ElementDelimiter, out int arraySpecifierCount)) {
                element = ParseArrayElement(arraySpecifierCount);
            }
            else if (ElementOpening(StringElement.ElementDelimiter, out int stringSpecifierCount)) {
                element = ParseStringElement(stringSpecifierCount);
            }
            else if (IntegerElementOpening(out int intSpecifierCount)) {
                element = ParseIntegerElement(intSpecifierCount);
            }
            else if (ElementOpening(LongElement.ElementDelimiter, out int longSpecifierCount)) {
                element = ParseLongIntegerElement(longSpecifierCount);
            }
            else if (ElementOpening(DecimalElement.ElementDelimiter, out int decSpecifierCount)) {
                element = ParseDecimalElement(decSpecifierCount);
            }
            else if (ElementOpening(DoubleElement.ElementDelimiter, out int doubleSpecifierCount)) {
                element = ParseDoubleElement(doubleSpecifierCount);
            }
            else if (ElementOpening(BooleanElement.ElementDelimiter, out int boolSpecifierCount)) {
                element = ParseBooleanElement(boolSpecifierCount);
            }
            else if (ElementOpening(DateTimeElement.ElementDelimiter, out int dateSpecifierCount)) {
                element = ParseDateElement(dateSpecifierCount);
            }
            else if (ElementOpening(DynamicElement.ElementDelimiter, out int phSpecifierCount)) {
                element = ParseDynamicElement(phSpecifierCount);
            }
            else if (ElementOpening(NullElement.ElementDelimiter, out int nullSpecifierCount)) {
                element = ParseNullElement(nullSpecifierCount);
            }
            else if (ElementOpening(ProcessingInstruction.ElementDelimiter, out int metaSpecifierCount2)) {
                element = ParseProcessingInstruction(metaSpecifierCount2);
            }
            else if (ElementOpening(CommentElement.ElementDelimiter, out int commentSpecifierCount)) {
                /* Parse comment but don't return it, as comments are not part of the logical output. */
                ParseCommentElement(commentSpecifierCount);
                SkipWhitespace();
                continue;
            }
            else {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Expected element.");
            }

            SkipWhitespace();

            if (element is not null && element is not ProcessingInstruction && element is not EmptyElement) {
                CloseProcessingInstructionScope(element);
            }

            return element ?? new EmptyElement();
        }
        return new EmptyElement();
    }

    private void OpenProcessingInstructionScope() {
        _piStack.Push([]);
    }

    private void CloseProcessingInstructionScope(Element? element) {
        if (element is not null && element is not EmptyElement) {
            var pendingPIs = _piStack.Pop();

            foreach (var pi in pendingPIs) {
                // If this is a document-level PI with an object value, dispatch each KVP to PI processors
                if (pi.Kvp != null && pi.Kvp.Key == DocumentProcessingInstruction.Keyword) {
                    pi.Target = _currentDocument?.Root;
                }
                else {
                    pi.Target = element;
                }
            }

            pendingPIs.Clear();
        }
    }

    private void ParseCommentElement(int specifierCount = 1) {
        while (IsCharAvailable()) {
            if (CommentElementClosing()) {
                break;
            }

            Advance();
        }
    }

    private ProcessingInstruction ParseProcessingInstruction(int specifierCount = 1) {
        SkipWhitespace();
        KeyValuePairElement? kvp = null;
        while (IsCharAvailable()) {
            if (ProcessingInstructionClosing()) {
                if (kvp == null) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Processing instruction must contain exactly one KVP");
                }

                if (kvp.Key == null) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Processing instruction key cannot be null");
                }

                if (kvp.Value is null) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Processing instruction value cannot be null");
                }

                var piKey = kvp.Key;

                ProcessingInstruction? pi;

                if (string.Equals(piKey, CharDefProcessingInstruction.Keyword, StringComparison.InvariantCultureIgnoreCase)) {
                    pi = ParseCharDefProcessingInstruction(kvp);
                }
                else if (string.Equals(piKey, DocumentProcessingInstruction.Keyword, StringComparison.InvariantCultureIgnoreCase)) {
                    pi = ParseDocumentProcessingInstruction(kvp);
                }
                else {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Failed to parse processing instruction.");
                }

                // Call PI processors/handlers with the KVP and its target
                if (pi is CharDefProcessingInstruction charDefPi) {
                    charDefPi.ProcessingInstructionHandler();
                }

                var pendingPIs = _piStack.Peek();
                pendingPIs.Add(pi);
                return pi;
            }

            var lastRow = CurrentRow;
            var lastColumn = CurrentColumn;
            var element = ParseElement();

            if (element is KeyValuePairElement kvpElem) {
                if (kvp != null) {
                    throw new InvalidOperationException($"At row {lastRow}, column {lastColumn}: PI must contain exactly one key-value pair");
                }

                kvp = kvpElem;
            }
            else {
                throw new InvalidOperationException($"At row {lastRow}, column {lastColumn}: Unexpected element type.");
            }
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {ProcessingInstruction.ElementName} element.");

    }

    private ProcessingInstruction ParseCharDefProcessingInstruction(KeyValuePairElement kvp) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Document PI must contain an object with metadata.");
        }

        var pi = new CharDefProcessingInstruction(obj);
        return pi;
    }

    private ProcessingInstruction ParseDocumentProcessingInstruction(KeyValuePairElement kvp) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Document PI must contain an object with metadata.");
        }

        var pi = new DocumentProcessingInstruction(obj);

        foreach (var innerKvp in obj.Dictionary.Values) {
            if (_piProcessors.TryGetValue(innerKvp.Key, out var processors)) {
                // Call each processor for the KVP
                foreach (var processor in processors) {
                    processor(innerKvp);
                }
            }
            else {
                // If no processors are registered, just log or handle as needed
                Debug.WriteLine($"No processors registered for key: {innerKvp.Key}");
            }
        }

        MapToXferDocumentMetadata(pi);
        return pi;
    }

    private ArrayElement ParseArrayElement(int specifierCount = 1) {
        var lastElementRow = LastElementRow;
        var lastElementColumn = LastElementColumn;
        var style = _delimStack.Peek().Style;
        SkipWhitespace();
        OpenProcessingInstructionScope();

        // Always create a simple ArrayElement which can hold any Element type
        ArrayElement arrayElement;
        try {
            arrayElement = new ArrayElement(style: style);
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to create array element. {ex.Message}", ex);
        }

        if (IsCharAvailable()) {
            if (ArrayElementClosing()) {
                CloseProcessingInstructionScope(arrayElement);
                return arrayElement;
            }

            Element element;
            try {
                element = ParseElement();
                arrayElement.Add(element);
            }
            catch (Exception ex) {
                throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to parse first array element. {ex.Message}", ex);
            }

            while (IsCharAvailable()) {
                if (ArrayElementClosing()) {
                    CloseProcessingInstructionScope(arrayElement);
                    return arrayElement;
                }

                try {
                    element = ParseElement();
                    arrayElement.Add(element);
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to parse array element. {ex.Message}", ex);
                }
            }
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {ArrayElement.ElementName} element.");
    }

    private Element ParseNullElement(int specifierCount) {
        while (IsCharAvailable()) {
            if (NullElementClosing()) {
                return new NullElement();
            }
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {NullElement.ElementName} element.");
    }

    private ObjectElement ParseObjectElement(int specifierCount = 1) {
        SkipWhitespace();
        OpenProcessingInstructionScope();
        var objectElement = new ObjectElement();

        while (IsCharAvailable()) {
            if (ObjectElementClosing()) {
                CloseProcessingInstructionScope(objectElement);
                return objectElement;
            }

            var lastRow = CurrentRow;
            var lastColumn = CurrentColumn;
            var element = ParseElement();

            if (element is KeyValuePairElement kvp) {
                if (objectElement.ContainsKey(kvp.Key)) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Duplicate key '{kvp.Key}' in object.");
                }
                objectElement.AddOrUpdate(kvp);
            }
            else if (element is ProcessingInstruction meta) {
                objectElement.AddOrUpdate(meta);
            }
            else if (element is EmptyElement) {
                // Ignore empty elements in objects
                continue;
            }
            else {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected element type.");
            }
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {ObjectElement.ElementName} element.");
    }

    private TupleElement ParseTupleElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        SkipWhitespace();
        OpenProcessingInstructionScope();
        var tupleElement = new TupleElement(style);

        while (IsCharAvailable()) {
            if (TupleElementClosing()) {
                CloseProcessingInstructionScope(tupleElement);
                return tupleElement;
            }

            var element = ParseElement();
            tupleElement.Add(element);
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {TupleElement.ElementName} element.");
    }

    private DynamicElement ParseDynamicElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (DynamicElementClosing()) {
                var variable = valueBuilder.ToString().Normalize(NormalizationForm.FormC);

                if (string.IsNullOrEmpty(variable)) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Dynamic variable must be a non-empty string.");
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

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DynamicElement.ElementName} element.");
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
                throw new InvalidOperationException($"At row {lastRow}, column {lastColumn}: Key must be a non-empty string.");
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
                throw new InvalidOperationException($"At row {lastRow}, column {lastColumn}: Key must be a non-empty string.");
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
        OpenProcessingInstructionScope();

        if (IsCharAvailable()) {
            // Assign pending ID to the KeyValuePairElement itself, if present
            Element valueElement = ParseElement();
            // Do NOT assign pending ID to valueElement here
            keyValuePairElement.Value = valueElement;
            CloseProcessingInstructionScope(keyValuePairElement);
            return keyValuePairElement;
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {KeyValuePairElement.ElementName} element.");
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
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Empty character element.");
        }

        int codePoint = '?';

        if (char.IsLetter(charString[0])) {
            var resolved = CharacterIdRegistry.Resolve(charString);

            if (resolved.HasValue) {
                codePoint = resolved.Value;
            }
        }
        else {
            codePoint = ParseNumericValue<char>(charString);
        }

        return new CharacterElement(codePoint, specifierCount, style: style);
    }

    private InterpolatedElement ParseInterpolatedElement(int specifierCount = 1) {
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
                        _ => throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected element type.")
                    }
                    );
                continue;
            }

            if (ElementExplicitOpening(InterpolatedElement.ElementDelimiter, out int evalSpecifierCount)) {
                InterpolatedElement interpolatedElement = ParseInterpolatedElement(evalSpecifierCount);
                valueBuilder.Append(interpolatedElement.Value);
                continue;
            }

            if (ElementExplicitOpening(DynamicElement.ElementDelimiter, out int phSpecifierCount)) {
                DynamicElement dynamicElement = ParseDynamicElement(phSpecifierCount);
                valueBuilder.Append(dynamicElement.Value);
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

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {InterpolatedElement.ElementName} element.");
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {StringElement.ElementName} element.");
    }

    private Element ParseDateElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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

                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid time string '{stringValue}'. Expected ISO 8601 format.");
                }

                valueBuilder.Append(CurrentChar);
                Expand();
            }
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DateTimeElement.ElementName} element.");
    }

    private IntegerElement ParseIntegerElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact or ElementStyle.Implicit) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {IntegerElement.ElementName} element.");
    }


    private LongElement ParseLongIntegerElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {LongElement.ElementName} element.");
    }

    private DecimalElement ParseDecimalElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DecimalElement.ElementName} element.");
    }

    private DoubleElement ParseDoubleElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        if (style is not ElementStyle.Compact) {
            SkipWhitespace();
        }

        StringBuilder valueBuilder = new StringBuilder();

        if (IsCharAvailable() && ElementOpening(DynamicElement.ElementDelimiter)) {
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DoubleElement.ElementName} element.");
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
            DynamicElement dynamicElement = ParseDynamicElement();
            valueBuilder.Append(dynamicElement.Value);
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
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {BooleanElement.ElementName} element.");

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
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid boolean value '{valueString}'.");
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
                            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unsupported type '{typeof(T)}' for decimal value parsing.");
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
                        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unsupported numeric base '{numberBase}'.");
                    }
            }
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Failed to parse numeric value '{valueString}'. Error: {ex.Message}");
        }
    }
}
