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
    private readonly List<ProcessingInstruction> _pendingPIs = new();
    // Used to hold a pending meta PI to attach as XferMetadata to the next element
    private ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver _dynamicSourceResolver = new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();

    // PI processor delegate type
    public delegate ProcessingInstruction PIProcessor(KeyValuePairElement kvp, Parser parser);

    // Registry for PI processors
    private readonly Dictionary<string, PIProcessor> _piProcessorRegistry = new(StringComparer.InvariantCultureIgnoreCase);

    // Extensible PI and element processors (existing functionality)
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
    /// Register a processing instruction processor that creates a specific PI type from a KVP.
    /// </summary>
    public void RegisterPIProcessor(string piKey, PIProcessor processor) {
        _piProcessorRegistry[piKey] = processor;
    }

    /// <summary>
    /// Unregister a processing instruction processor.
    /// </summary>
    public void UnregisterPIProcessor(string piKey) {
        _piProcessorRegistry.Remove(piKey);
    }

    /// <summary>
    /// Check if a processing instruction processor is registered for the given key.
    /// </summary>
    public bool HasPIProcessor(string piKey) {
        return _piProcessorRegistry.ContainsKey(piKey);
    }

    /// <summary>
    /// Register an element processor. Called with each element as it is constructed.
    /// </summary>
    public void RegisterElementProcessor(Action<Element> processor) => _elementProcessors.Add(processor);

    /// <summary>
    /// Adds a warning to the current document being parsed.
    /// </summary>
    private void AddWarning(WarningType type, string message, string? context = null) {
        if (_currentDocument != null) {
            _currentDocument.Warnings.Add(new ParseWarning(type, message, CurrentRow, CurrentColumn, context));
        }
    }



    public ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver DynamicSourceResolver {
        get => _dynamicSourceResolver;
        set => _dynamicSourceResolver = value ?? new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();
    }

    private XferDocument? _currentDocument = null;

    public static readonly string Version = "0.13";


    public Parser() : this(Encoding.UTF8) { }

    public Parser(Encoding encoding) {
        Encoding = encoding;
        RegisterBuiltInPIProcessors();
    }

    /// <summary>
    /// Register the built-in processing instruction processors.
    /// </summary>
    private void RegisterBuiltInPIProcessors() {
        RegisterPIProcessor(CharDefProcessingInstruction.Keyword, CreateCharDefProcessingInstruction);
        RegisterPIProcessor(DocumentProcessingInstruction.Keyword, CreateDocumentProcessingInstruction);
        RegisterPIProcessor(IdProcessingInstruction.Keyword, CreateIdProcessingInstruction);
        RegisterPIProcessor(DynamicSourceProcessingInstruction.Keyword, CreateDynamicSourceProcessingInstruction);
    }

    // Built-in PI processor factory methods
    private static ProcessingInstruction CreateCharDefProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: CharDef PI must contain an object containing text-to-character-element key/value pairs.");
        }

        return new CharDefProcessingInstruction(obj);
    }

    private static ProcessingInstruction CreateIdProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not TextElement textElement) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: Id PI must contain a text element.");
        }

        return new IdProcessingInstruction(textElement);
    }

    private ProcessingInstruction CreateDocumentProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: Document PI must contain an object with metadata.");
        }

        var pi = new DocumentProcessingInstruction(obj);

        foreach (var innerKvp in obj.Dictionary.Values) {
            if (_piProcessorRegistry.TryGetValue(innerKvp.Key, out var processor)) {
                // Call the processor for the KVP
                processor(innerKvp, this);
            }
            else {
                // If no processors are registered, just log or handle as needed
                Debug.WriteLine($"No processors registered for key: {innerKvp.Key}");
            }
        }

        MapToXferDocumentMetadata(pi);
        return pi;
    }

    private static ProcessingInstruction CreateDynamicSourceProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: DynamicSource PI must contain an object with source configuration.");
        }

        return new DynamicSourceProcessingInstruction(obj);
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

    internal XferDocument ParseDocument()
    {
        var document = new XferDocument();
        _currentDocument = document;

        CollectionElement? rootElement = null;

        // Parse all top-level elements in order
        while (IsCharAvailable()) {
            SkipWhitespace();

            if (!IsCharAvailable()) {
                break;
            }

            var element = ParseElement();

            if (element is ProcessingInstruction pi) {
                // PI at document level - store it and set next element as target
                document.ProcessingInstructions.Add(pi);
                Console.WriteLine($"[PARSER] Added document-level PI: {pi.GetType().Name} - {pi.Kvp?.Key}");
            }
            else if (element is CollectionElement collectionElement) {
                if (rootElement != null) {
                    throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Multiple root elements found. A document can only have one root element.");
                }
                rootElement = collectionElement;

                // Set any previous document-level PIs to target this root element
                foreach (var docPI in document.ProcessingInstructions) {
                    if (docPI.Target == null) {
                        docPI.Target = rootElement;
                        Console.WriteLine($"[PARSER] Set PI target: {docPI.Kvp?.Key} -> {rootElement.GetType().Name}");
                    }
                }

                Console.WriteLine($"[PARSER] Found root element: {rootElement.GetType().Name}");
            }
            else if (element is EmptyElement) {
                // Skip empty elements
            }
            else {
                throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Invalid top-level element '{element.GetType().Name}'. Top-level elements must be processing instructions or a single collection element (object, array, or tuple).");
            }

            SkipWhitespace();
        }

        Console.WriteLine($"[PARSER] Total document-level PIs stored: {document.ProcessingInstructions.Count}");

        // Set the root element (default to empty tuple if none found)
        document.Root = rootElement ?? new TupleElement();

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


    internal Element ParseElement() {
        SkipWhitespace();

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

            return element ?? new EmptyElement();
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

                // Look up the PI processor in the registry
                if (_piProcessorRegistry.TryGetValue(piKey, out var processor)) {
                    pi = processor(kvp, this);
                }
                else {
                    // Add warning for unregistered PI and create a generic PI to preserve the data
                    AddWarning(WarningType.UnregisteredProcessingInstruction,
                              $"Unknown processing instruction '{piKey}'. Creating generic PI to preserve data.",
                              piKey);

                    // Create a generic ProcessingInstruction to preserve the data structure
                    pi = new ProcessingInstruction(kvp.Value, piKey);
                }

                // Call PI processors/handlers with the KVP and its target
                if (pi is CharDefProcessingInstruction charDefPi) {
                    charDefPi.ProcessingInstructionHandler();
                }
                else if (pi is DynamicSourceProcessingInstruction dynamicSourcePi) {
                    dynamicSourcePi.ProcessingInstructionHandler();
                }

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

    private ArrayElement ParseArrayElement(int specifierCount = 1) {
        var lastElementRow = LastElementRow;
        var lastElementColumn = LastElementColumn;
        var style = _delimStack.Peek().Style;
        SkipWhitespace();

        // Always create a simple ArrayElement which can hold any Element type
        ArrayElement arrayElement;
        try {
            arrayElement = new ArrayElement(style: style);
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to create array element. {ex.Message}", ex);
        }

        // Apply any pending PIs to this array element
        foreach (var pendingPI in _pendingPIs)
        {
            pendingPI.Target = arrayElement;
            pendingPI.ElementHandler(arrayElement);
        }
        _pendingPIs.Clear();

        if (IsCharAvailable()) {
            if (ArrayElementClosing()) {
                return arrayElement;
            }

            // Handle processing instructions specially in arrays
            if (ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount)) {
                var pi = ParseProcessingInstruction(piSpecifierCount);
                // Add PI to array for serialization
                arrayElement.Add(pi);
                // Also add to pending PIs for application to next element
                _pendingPIs.Add(pi);
                SkipWhitespace();
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
                    return arrayElement;
                }

                // Handle processing instructions specially in arrays
                if (ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount2)) {
                    var pi = ParseProcessingInstruction(piSpecifierCount2);
                    // Add PI to array for serialization
                    arrayElement.Add(pi);
                    // Also add to pending PIs for application to next element
                    _pendingPIs.Add(pi);
                    SkipWhitespace();
                    continue;
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
        var objectElement = new ObjectElement();

        // Apply any pending PIs to this object element
        foreach (var pendingPI in _pendingPIs)
        {
            pendingPI.Target = objectElement;
            pendingPI.ElementHandler(objectElement);
        }
        _pendingPIs.Clear();

        while (IsCharAvailable()) {
            if (ObjectElementClosing()) {
                return objectElement;
            }

            var lastRow = CurrentRow;
            var lastColumn = CurrentColumn;

            // Handle processing instructions specially in objects
            if (ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount)) {
                var pi = ParseProcessingInstruction(piSpecifierCount);
                // Add PI to object for serialization
                objectElement.AddOrUpdate(pi);
                // Also add to pending PIs for application to next element
                _pendingPIs.Add(pi);
                SkipWhitespace();
                continue;
            }

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
        var tupleElement = new TupleElement(style);

        List<ProcessingInstruction> pendingPIs = new List<ProcessingInstruction>();

        while (IsCharAvailable()) {
            if (TupleElementClosing()) {
                return tupleElement;
            }

            var element = ParseElement();

            if (element is ProcessingInstruction pi) {
                // Add PI to tuple and track it for target assignment
                tupleElement.Add(pi);
                pendingPIs.Add(pi);
                Console.WriteLine($"[PARSER] Added PI to tuple: {pi.Kvp?.Key}");
            } else {
                // Non-PI element: set it as target for any pending PIs
                foreach (var pendingPI in pendingPIs) {
                    pendingPI.Target = element;
                    Console.WriteLine($"[PARSER] Set PI target: {pendingPI.Kvp?.Key} -> {element.GetType().Name}");
                }
                pendingPIs.Clear();

                tupleElement.Add(element);
            }
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

        if (IsCharAvailable()) {
            // Store any pending PIs that should be applied to this KVP
            var savedPendingPIs = new List<ProcessingInstruction>(_pendingPIs);
            _pendingPIs.Clear();

            // Check for processing instructions that should apply to the value
            var valuePIs = new List<ProcessingInstruction>();
            while (ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount)) {
                var pi = ParseProcessingInstruction(piSpecifierCount);
                valuePIs.Add(pi);
                SkipWhitespace();
            }

            Element valueElement = ParseElement();
            keyValuePairElement.Value = valueElement;

            // Add value PIs as children of the KVP for serialization
            foreach (var valuePI in valuePIs) {
                keyValuePairElement.Children.Add(valuePI);
                valuePI.Parent = keyValuePairElement;
                // Also apply the PI to the value element
                valuePI.Target = valueElement;
                valuePI.ElementHandler(valueElement);
            }

            // Apply the saved PIs to this KVP
            foreach (var pendingPI in savedPendingPIs)
            {
                pendingPI.Target = keyValuePairElement;
                pendingPI.ElementHandler(keyValuePairElement);
            }

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
            // Add warning for empty character element and use replacement character
            AddWarning(WarningType.EmptyCharacterElement,
                      "Empty character element, using '?' (U+003F) as replacement",
                      null);
            return new CharacterElement('?', specifierCount, style: style);
        }

        int codePoint = '?';

        if (char.IsLetter(charString[0])) {
            var resolved = CharacterIdRegistry.Resolve(charString);

            if (resolved.HasValue) {
                codePoint = resolved.Value;
            }
            else {
                // Add warning for unresolved character name
                AddWarning(WarningType.CharacterResolutionFailure,
                          $"Unknown character name '{charString}', using '?' (U+003F) as fallback",
                          charString);
                codePoint = '?';
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
