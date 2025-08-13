using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Schema;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang.Services;

/// <summary>
/// The main XferLang parser that converts XferLang text into a structured document model.
/// Supports extensible processing instructions, element processors, and dynamic source resolution.
/// This parser provides comprehensive parsing capabilities including ID uniqueness validation,
/// character definition resolution, and flexible element processing.
/// </summary>
public partial class Parser : IXferParser {
    // Stack to track parent elements for context-sensitive parsing
    /// <summary>
    /// Optional: A delegate for resolving charDef keywords. Should return the codepoint for a keyword, or null if not found.
    /// </summary>
    public Func<Element, string, int?>? CharDefResolver { get; set; }

    private const char PlaceholderCharacter = '?';

    // Used to assign IDs from inline PIs to subsequent elements
    private Queue<string> _pendingIds = new Queue<string>();
    private readonly List<ProcessingInstruction> _pendingPIs = new();
    // Track used IDs to ensure uniqueness
    private readonly HashSet<string> _usedIds = new(StringComparer.Ordinal);
    // Used to hold a pending meta PI to attach as XferMetadata to the next element
    private ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver _dynamicSourceResolver = new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();

    /// <summary>
    /// Delegate type for processing instruction processors.
    /// Defines the signature for custom processing instruction handlers.
    /// </summary>
    /// <param name="kvp">The key-value pair element containing the processing instruction data.</param>
    /// <param name="parser">The parser instance processing the document.</param>
    /// <returns>A ProcessingInstruction instance representing the processed instruction.</returns>
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
    internal void AddWarning(WarningType type, string message, string? context = null) {
        if (_currentDocument != null) {
            _currentDocument.Warnings.Add(new ParseWarning(type, message, CurrentRow, CurrentColumn, context));
        }
    }

    /// <summary>
    /// Adds a warning at a specific row/column location. Use when the logical
    /// source of the issue is earlier than the current cursor position.
    /// </summary>
    internal void AddWarningAt(WarningType type, string message, int row, int column, string? context = null) {
        if (_currentDocument != null) {
            _currentDocument.Warnings.Add(new ParseWarning(type, message, row, column, context));
        }
    }

    /// <summary>
    /// Adds a warning anchored to the start of the most recently opened element
    /// (tracked in LastElementRow/LastElementColumn).
    /// </summary>
    internal void AddWarningAtElementStart(WarningType type, string message, string? context = null) {
        AddWarningAt(type, message, LastElementRow, LastElementColumn, context);
    }

    /// <summary>
    /// Validates and registers an ID to ensure uniqueness within the document.
    /// </summary>
    /// <param name="id">The ID to validate and register</param>
    /// <exception cref="InvalidOperationException">Thrown when the ID is already in use</exception>
    internal void ValidateAndRegisterElementId(string id) {
        if (string.IsNullOrEmpty(id)) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Element ID cannot be null or empty.");
        }

        if (!_usedIds.Add(id)) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Duplicate element ID '{id}'. Element IDs must be unique within a document.");
        }
    }



    /// <summary>
    /// Gets or sets the dynamic source resolver used for resolving dynamic content references in XferLang documents.
    /// If set to null, defaults to the DefaultDynamicSourceResolver implementation.
    /// </summary>
    public ParksComputing.Xfer.Lang.DynamicSource.IDynamicSourceResolver DynamicSourceResolver {
        get => _dynamicSourceResolver;
        set => _dynamicSourceResolver = value ?? new ParksComputing.Xfer.Lang.DynamicSource.DefaultDynamicSourceResolver();
    }

    private XferDocument? _currentDocument = null;
    // Expose current document for internal consumers (e.g., script PI local suppression)
    internal XferDocument? CurrentDocument => _currentDocument;
    // Legacy reference bindings (removed in favor of scoped stacks)
    private readonly Dictionary<string, Element> _referenceBindings = new(StringComparer.Ordinal); // retained only temporarily for compatibility with any remaining calls
    // Scoped binding stack for script 'let'
    private readonly Stack<Dictionary<string, Element>> _bindingScopes = new();

    internal void PushBindingScope() {
        _bindingScopes.Push(new Dictionary<string, Element>(StringComparer.Ordinal));
    }

    internal void PopBindingScope() {
        if (_bindingScopes.Count > 0) { _bindingScopes.Pop(); }
    }

    internal void BindReference(string name, Element valueElement) {
        if (_bindingScopes.Count == 0) { PushBindingScope(); }
        _bindingScopes.Peek()[name] = valueElement; // store original (mutations visible to future clones)
        // Trace binding using dedicated Trace warning type
        if (_currentDocument != null) {
            _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] bind '{name}' => {valueElement.GetType().Name}", CurrentRow, CurrentColumn, name));
        }
    }

    internal bool TryResolveBinding(string name, out Element? element) {
        foreach (var scope in _bindingScopes) { // stack enumerates with most recent first
            if (scope.TryGetValue(name, out element)) { return true; }
        }
        element = null;
        return false;
    }

    /// <summary>
    /// Gets the version of the XferLang parser.
    /// </summary>
    public static readonly string Version = "0.14";

    /// <summary>
    /// Initializes a new instance of the Parser class with UTF-8 encoding.
    /// </summary>
    public Parser() : this(Encoding.UTF8) { }

    /// <summary>
    /// Initializes a new instance of the Parser class with the specified encoding.
    /// </summary>
    /// <param name="encoding">The text encoding to use for parsing input data.</param>
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
        RegisterPIProcessor(TagProcessingInstruction.Keyword, CreateTagProcessingInstruction);
        RegisterPIProcessor(DynamicSourceProcessingInstruction.Keyword, CreateDynamicSourceProcessingInstruction);
        RegisterPIProcessor(DefinedProcessingInstruction.Keyword, CreateDefinedProcessingInstruction);
        RegisterPIProcessor(IfProcessingInstruction.Keyword, CreateIfProcessingInstruction);
        RegisterPIProcessor(ScriptProcessingInstruction.Keyword, CreateScriptProcessingInstruction);
        RegisterPIProcessor(LetProcessingInstruction.Keyword, CreateLetProcessingInstruction);
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

        // Validate ID uniqueness before creating the PI
        parser.ValidateAndRegisterElementId(textElement.Value);

        return new IdProcessingInstruction(textElement);
    }

    private static ProcessingInstruction CreateTagProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not TextElement textElement) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: Tag PI must contain a text element.");
        }

        return new TagProcessingInstruction(textElement);
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
        return pi;
    }

    private static ProcessingInstruction CreateDynamicSourceProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"At row {parser.CurrentRow}, column {parser.CurrentColumn}: DynamicSource PI must contain an object with source configuration.");
        }
        return new DynamicSourceProcessingInstruction(obj);
    }

    private static ProcessingInstruction CreateDefinedProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        // Accept any element type for defined processing instruction
        return new DefinedProcessingInstruction(kvp.Value);
    }

    private static ProcessingInstruction CreateIfProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        // Value can be any element representing the condition expression (text, dynamic, collection operator, etc.)
        return new IfProcessingInstruction(kvp.Value, parser);
    }

    private static ProcessingInstruction CreateScriptProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        return new ScriptProcessingInstruction(kvp, parser);
    }

    private static ProcessingInstruction CreateLetProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        return new LetProcessingInstruction(kvp, parser);
    }

    /// <summary>
    /// Gets the text encoding used by this parser instance for converting byte arrays to strings.
    /// </summary>
    public Encoding Encoding { get; private set; } = Encoding.UTF8;

    private string _scanString = string.Empty;

    internal class ParserState {
        // Core buffer window
        internal string ScanString { get; set; } = string.Empty;
        internal int Start { get; set; } = 0;
        internal int Position { get; set; } = 0;

        // Cursor/diagnostics
        internal int CurrentRow { get; set; } = 1;
        internal int CurrentColumn { get; set; } = 1;
        internal int LastElementRow { get; set; } = 1;
        internal int LastElementColumn { get; set; } = 1;

        // Delimiter stack snapshot (bottom->top order to allow accurate reconstruction)
        internal ElementDelimiter[] DelimStackSnapshot { get; set; } = Array.Empty<ElementDelimiter>();

        // Transient parse state to isolate across nested parses
        internal List<ProcessingInstruction> PendingPIs { get; set; } = new();
        internal string[] PendingIds { get; set; } = Array.Empty<string>();
        internal List<string> UsedIds { get; set; } = new();

        // Bindings and references
        internal List<Dictionary<string, Element>> BindingScopes { get; set; } = new();
        internal Dictionary<string, Element> ReferenceBindings { get; set; } = new(StringComparer.Ordinal);

        // Document pointer (for warnings, etc.)
        internal XferDocument? CurrentDocument { get; set; } = null;
    }

    private Stack<ParserState> _parserStateStack = new Stack<ParserState>();

    private void PushParserState() {
        // Snapshot delimiter stack as bottom->top
        var delimArray = _delimStack.ToArray(); // top->bottom
        Array.Reverse(delimArray); // bottom->top

        // Snapshot binding scopes as bottom->top and clone each dictionary
        var scopesArray = _bindingScopes.ToArray(); // top->bottom
        Array.Reverse(scopesArray); // bottom->top
        var scopesClone = new List<Dictionary<string, Element>>(scopesArray.Length);
        foreach (var scope in scopesArray) {
            scopesClone.Add(new Dictionary<string, Element>(scope));
        }

        var state = new ParserState {
            // Core buffer window
            ScanString = ScanString,
            Start = Start,
            Position = Position,

            // Cursor/diagnostics
            CurrentRow = CurrentRow,
            CurrentColumn = CurrentColumn,
            LastElementRow = LastElementRow,
            LastElementColumn = LastElementColumn,

            // Delimiters
            DelimStackSnapshot = delimArray,

            // Transient parse state
            PendingPIs = new List<ProcessingInstruction>(_pendingPIs),
            PendingIds = _pendingIds.ToArray(),
            UsedIds = new List<string>(_usedIds),

            // Bindings and references
            BindingScopes = scopesClone,
            ReferenceBindings = new Dictionary<string, Element>(_referenceBindings),

            // Document
            CurrentDocument = _currentDocument,
        };

        _parserStateStack.Push(state);

        // Isolate transient state for nested parse
        _pendingPIs.Clear();
        _pendingIds.Clear();
        _usedIds.Clear();

        // Start nested parse with a clean delimiter stack environment
        _delimStack = new Stack<ElementDelimiter>();
        // Note: We keep live bindings available for resolution but they will be restored on Pop
    }

    private void PopParserState() {
        if (_parserStateStack.Count > 0) {
            var state = _parserStateStack.Pop();

            // Restore delimiter stack from bottom->top snapshot
            _delimStack = new Stack<ElementDelimiter>(state.DelimStackSnapshot);

            // Restore transient collections
            _pendingPIs.Clear();
            _pendingPIs.AddRange(state.PendingPIs);

            _pendingIds.Clear();
            foreach (var id in state.PendingIds) { _pendingIds.Enqueue(id); }

            _usedIds.Clear();
            foreach (var id in state.UsedIds) { _usedIds.Add(id); }

            // Restore bindings (rebuild bottom->top)
            _bindingScopes.Clear();
            foreach (var scope in state.BindingScopes) {
                _bindingScopes.Push(new Dictionary<string, Element>(scope));
            }

            _referenceBindings.Clear();
            foreach (var kv in state.ReferenceBindings) { _referenceBindings[kv.Key] = kv.Value; }

            _currentDocument = state.CurrentDocument;

            // Critical: restore ScanString before Start/Position to avoid setter reset
            ScanString = state.ScanString;
            Start = state.Start;
            Position = state.Position;
            CurrentRow = state.CurrentRow;
            CurrentColumn = state.CurrentColumn;
            LastElementRow = state.LastElementRow;
            LastElementColumn = state.LastElementColumn;
        }
    }

    private int Start { get; set; } = 0;

    private int Position { get; set; } = 0;

    private string ScanString {
        get {
            return _scanString;
        }
        set {
            _scanString = value ?? string.Empty;
            Start = 0;
            Position = 0;
        }
    }

    private char CurrentChar {
        get {
            if (Position >= ScanString.Length) { return '\0'; }
            return ScanString[Position];
        }
    }

    private char Peek {
        get {
            if (Position + 1 >= ScanString.Length) { return '\0'; }
            return ScanString[Position + 1];
        }
    }

    private string CurrentString {
        get {
            if (Start >= ScanString.Length || Position >= ScanString.Length) { return string.Empty; }
            return ScanString.Substring(Start, Position - (Start - 1));
        }
    }

    private char PreviousChar {
        get {
            if (Position - 1 < 0) { return '\0'; }
            return ScanString[Position - 1];
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
        if (KeywordElement.IsKeywordLeadingChar(CurrentChar)) {
            specifierCount = 1;
            LastElementRow = CurrentRow;
            LastElementColumn = CurrentColumn;
            _delimStack.Push(new ElementDelimiter(KeywordElement.OpeningSpecifier, KeywordElement.ClosingSpecifier, specifierCount, ElementStyle.Implicit));
            return true;
        }

        return ElementOpening(KeywordElement.ElementDelimiter, out specifierCount);
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

    internal bool ReferenceElementOpening(out int specifierCount) {
        // Leading '_' followed by at least one identifier char (letter, digit, '_' or '-')
        if (CurrentChar == ReferenceElement.OpeningSpecifier && (char.IsLetterOrDigit(Peek) || Peek == '_' || Peek == '-')) {
            specifierCount = 1;
            LastElementRow = CurrentRow;
            LastElementColumn = CurrentColumn;
            _delimStack.Push(new EmptyClosingElementDelimiter(ReferenceElement.OpeningSpecifier, ReferenceElement.ClosingSpecifier, specifierCount, ElementStyle.Compact));
            Advance(); // consume leading deref specifier (similar to ElementOpening behavior)
            return true;
        }
        return ElementOpening(ReferenceElement.ElementDelimiter, out specifierCount);
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

    internal bool ReferenceElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == ReferenceElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool IdentifierElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == IdentifierElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool KeywordElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == KeywordElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    // Added CharacterElementClosing for character element parsing; mirrors compact closing semantics.
    internal bool CharacterElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == CharacterElement.ClosingSpecifier);
        return ElementCompactClosing();
    }

    internal bool InterpolatedElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == InterpolatedElement.ClosingSpecifier);
        return ElementClosing();
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

    internal bool QueryElementClosing() {
        Debug.Assert(_delimStack.Peek().ClosingSpecifier == QueryElement.ClosingSpecifier);
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
            // If we're closing on the closing specifier, advance past it
            if (CurrentChar == closingSpecifier) {
                Advance();
            }
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
        char.IsDigit(c) ||
        c == '-' || c == '+' || c == '_' ||
        c == 'x' || c == 'X' || // hex prefixes
        c == 'b' || c == 'B' || // binary prefixes
        c == 'o' || c == 'O' || // octal prefixes
        (c >= 'A' && c <= 'F') ||
        (c >= 'a' && c <= 'f') ||
        c == '$' || c == '%' || c == '.' || c == ',';

    // Removed duplicate IsKeywordChar; use CharExtensions.IsKeywordChar instead.

    /// <summary>
    /// Parses a string containing XferLang content into an XferDocument.
    /// </summary>
    /// <param name="input">The XferLang content to parse as a string.</param>
    /// <exception cref="ArgumentNullException">Thrown when input is null or empty.</exception>
    public XferDocument Parse(string input) {
        if (string.IsNullOrEmpty(input)) {
            throw new ArgumentNullException(nameof(input));
        }

        return Parse(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>
    /// Parses a byte array containing XferLang content into an XferDocument.
    /// Uses the parser's configured encoding to convert bytes to text before parsing.
    /// </summary>
    /// <param name="input">The XferLang content to parse as a byte array.</param>
    /// <returns>An XferDocument containing the parsed elements and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null or has zero length.</exception>
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

    /// <summary>
    /// Parses a standalone XferLang fragment into a single Element using a sandboxed parser state.
    /// Useful for nested parsing scenarios (e.g., interpolated content) without disturbing outer state.
    /// </summary>
    /// <param name="fragment">The XferLang text representing a single element.</param>
    /// <returns>The parsed Element.</returns>
    /// <exception cref="InvalidOperationException">When no element is found or extra trailing tokens remain.</exception>
    public Element ParseFragment(string fragment) {
        if (fragment is null) { throw new ArgumentNullException(nameof(fragment)); }
        PushParserState();
        try {
            // Initialize scan for fragment (resets Start/Position)
            ScanString = fragment;
            CurrentRow = 1; CurrentColumn = 1;
            LastElementRow = 1; LastElementColumn = 1;

            SkipBOM();
            SkipWhitespace();

            // Parse exactly one element from the fragment
            var element = ParseElement();

            // After parsing the element, ensure only trailing whitespace remains
            SkipWhitespace();
            if (IsCharAvailable()) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected trailing content in fragment.");
            }

            return element;
        }
        finally {
            PopParserState();
        }
    }

    /// <summary>
    /// Parses a fragment that may contain zero or more elements; returns a TupleElement of parsed children.
    /// State is fully sandboxed and restored on exit.
    /// </summary>
    /// <param name="fragment">The XferLang text with one or more elements.</param>
    /// <returns>A TupleElement containing all parsed elements in order.</returns>
    public TupleElement ParseFragmentMany(string fragment) {
        if (fragment is null) { throw new ArgumentNullException(nameof(fragment)); }
        PushParserState();
        try {
            ScanString = fragment;
            CurrentRow = 1; CurrentColumn = 1;
            LastElementRow = 1; LastElementColumn = 1;

            SkipBOM();
            var tuple = new TupleElement();

            while (IsCharAvailable()) {
                SkipWhitespace();
                if (!IsCharAvailable()) { break; }
                var element = ParseElement();
                if (element is not EmptyElement) {
                    tuple.Add(element);
                }
            }

            return tuple;
        }
        finally {
            PopParserState();
        }
    }
    internal XferDocument ParseDocument()
    {
        var document = new XferDocument();
        _currentDocument = document;

        // Clear ID tracking for new document
        _usedIds.Clear();
    _referenceBindings.Clear();

        CollectionElement? rootElement = null;

        // Parse all top-level elements in order
        while (IsCharAvailable()) {
            SkipWhitespace();

            if (!IsCharAvailable()) {
                break;
            }

            // (Late-removed approach of bulk executing before next element replaced by immediate execution after parsing each PI.)

            var element = ParseElement();

            if (element is ProcessingInstruction pi) {
                document.ProcessingInstructions.Add(pi);
                if (rootElement == null) {
                    switch (pi) {
                        case ProcessingInstructions.ScriptProcessingInstruction spiTop:
                            spiTop.ExecuteTopLevelEarlyBindings();
                            break;
                        case ProcessingInstructions.LetProcessingInstruction lpiTop:
                            lpiTop.ExecuteEarly();
                            break;
                    }
                }
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
                        try {
                            docPI.ElementHandler(rootElement);
                        }
                        catch (ConditionalElementException) {
                            // Root suppressed: replace with empty tuple so document remains valid.
                            var replacement = new TupleElement();
                            rootElement = replacement;
                            docPI.Target = replacement; // keep PI targeting new root for serialization visibility
                            // retain PI (SuppressSerialization already false by default)
                        }
                    }
                }
                // After applying PIs (including script) the root content already parsed with active script scopes.
            }
            else if (element is EmptyElement) {
                // Skip empty elements
            }
            else {
                // Relax rule: allow a single non-collection root element by auto-wrapping it in a tuple root
                if (rootElement != null) {
                    throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Multiple root elements found. A document can only have one root element.");
                }
                var tuple = new TupleElement();
                // Attach previously parsed PIs to this new synthetic tuple root so they can target its first child uniformly.
                foreach (var docPI in document.ProcessingInstructions) {
                    if (docPI.Target == null) { docPI.Target = tuple; }
                }
                tuple.Add(element);
                rootElement = tuple;
            }

            SkipWhitespace();
        }

        // Set the root element (default to empty tuple if none found)
        document.Root = rootElement ?? new TupleElement();

        // Apply any remaining pending PIs to the root element
        foreach (var pendingPI in _pendingPIs) {
            pendingPI.Target = document.Root;
            pendingPI.ElementHandler(document.Root);
        }
        _pendingPIs.Clear();

        // Execute any top-level script PIs (those parsed before a root element existed) now that a root is known.
        foreach (var topLevelPI in document.ProcessingInstructions) {
            if (topLevelPI is ProcessingInstructions.ScriptProcessingInstruction spi) {
                if (spi.Target == null) { spi.Target = document.Root; }
                // Ensure operators executed; ElementHandler is idempotent due to internal guard.
                spi.ElementHandler(spi.Target);
            }
        }

        // Final dereference resolution pass: any remaining ReferenceElements
        // (e.g. those appearing after their script PIs in the same tuple/object) are
        // replaced now that all bindings have been collected. This complements the
        // immediate resolution that occurs during element parsing for earlier siblings.
        if (document.Root is not null) {
            ResolveRemainingDereferences(document.Root);
        }

        // Deactivate any active script scopes now that parsing complete
        foreach (var docPI in document.ProcessingInstructions) {
            // No script PI scope deactivation needed (no persistent scope object now).
        }

        return document;
    }

    private void ResolveRemainingDereferences(Element element) {
        // Handle object separately (it is also a CollectionElement, so check first)
        if (element is ObjectElement obj) {
            foreach (var kv in obj.Dictionary) {
                var v = kv.Value.Value;
                if (v is ReferenceElement d2) {
                    if (TryResolveBinding(d2.Value, out var bound2)) {
                        kv.Value.Value = Helpers.ElementCloner.Clone(bound2!);
                        if (_currentDocument != null) {
                            _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] post-pass resolved '{d2.Value}' in object", CurrentRow, CurrentColumn, d2.Value));
                            SuppressEarlierUnresolvedReferenceWarning(d2.Value);
                        }
                    }
                } else {
                    ResolveRemainingDereferences(v);
                }
            }
        }
        else if (element is InterpolatedElement ie) {
            // Re-parse the interpolated string using the real parser to resolve any
            // explicit elements (e.g., <_name_>) now that bindings are available.
            try {
                var before = ie.Value ?? string.Empty;
                var beforeNames = ExtractDereferenceNames(before);
                var serialized = ie.ToXfer(); // produce a canonical interpolated literal with proper quoting
                if (!string.IsNullOrEmpty(serialized)) {
                    if (ParseFragment(serialized) is InterpolatedElement reparsed && reparsed.Value != ie.Value) {
                        ie.Value = reparsed.Value;
                        var after = ie.Value ?? string.Empty;
                        var afterNames = ExtractDereferenceNames(after);
                        // Any names that were present before but no longer present after reparse were resolved
                        foreach (var resolvedName in beforeNames.Except(afterNames)) {
                            if (_currentDocument != null) {
                                _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] post-pass resolved '{resolvedName}' in interpolated", CurrentRow, CurrentColumn, resolvedName));
                            }
                            SuppressEarlierUnresolvedReferenceWarning(resolvedName);
                        }
                    }
                }
            } catch {
                // If re-parse fails for any reason, leave value as-is.
            }
        }
        else if (element is CollectionElement coll) {
            for (int i = 0; i < coll.Children.Count; i++) {
                var child = coll.Children[i];
                if (child is ReferenceElement d) {
                    if (TryResolveBinding(d.Value, out var bound)) {
                        coll.Children[i] = Helpers.ElementCloner.Clone(bound!);
                        if (_currentDocument != null) {
                            _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] post-pass resolved '{d.Value}' in collection", CurrentRow, CurrentColumn, d.Value));
                            SuppressEarlierUnresolvedReferenceWarning(d.Value);
                        }
                    }
                } else {
                    ResolveRemainingDereferences(child);
                }
            }
        }
        else {
            // Scalar element: nothing to traverse
        }
    }

    private void SuppressEarlierUnresolvedReferenceWarning(string name) {
        if (_currentDocument == null) { return; }
        for (int i = 0; i < _currentDocument.Warnings.Count; i++) {
            var w = _currentDocument.Warnings[i];
            if (w.Type == WarningType.UnresolvedReference && string.Equals(w.Context, name, StringComparison.Ordinal)) {
                w.Message += " (resolved later – suppressed)";
                w.Type = WarningType.Trace;
                break;
            }
        }
    }

    private static IEnumerable<string> ExtractDereferenceNames(string text) {
        if (string.IsNullOrEmpty(text)) { yield break; }
        // Match explicit deref tokens: <__name__> where underscore counts match (captured via backreference)
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"<(_+)([A-Za-z0-9_-]+)\1>");
        foreach (System.Text.RegularExpressions.Match m in matches) {
            if (m.Success && m.Groups.Count >= 3) {
                yield return m.Groups[2].Value;
            }
        }
    }

    // SubstituteScriptBindings removed (immediate resolution now handled during parsing).

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
            if (ReferenceElementOpening(out int derefImplicitCount)) {
                element = ParseReferenceElement(derefImplicitCount);
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
                element = ParseTemporalElement(dateSpecifierCount);
            }
            else if (ElementOpening(DynamicElement.ElementDelimiter, out int phSpecifierCount)) {
                element = ParseDynamicElement(phSpecifierCount);
            }
            else if (ElementOpening(IdentifierElement.ElementDelimiter, out int identifierSpecifierCount)) {
                element = ParseIdentifierElement(identifierSpecifierCount);
            }
            else if (ElementOpening(QueryElement.ElementDelimiter, out int querySpecifierCount)) {
                element = ParseQueryElement(querySpecifierCount);
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

    private QueryElement ParseQueryElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;

        KeyValuePairElement? kvpElem = null;

        while (IsCharAvailable()) {
            if (QueryElementClosing()) {
                if (kvpElem != null) {
                    return new QueryElement(kvpElem, specifierCount, style);
                }
                else {
                    throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Unexpected element type.");
                }
            }

            if (kvpElem is not null) {
                throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Unexpected element.");
            }

            Element element = ParseElement();

            if (element is not KeyValuePairElement kvp) {
                throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Expected key-value pair element in query.");
            }

            kvpElem = kvp;
            var key = kvp.Key ?? string.Empty;
            if (string.IsNullOrEmpty(key)) {
                throw new InvalidOperationException($"At row {LastElementRow}, column {LastElementColumn}: Query key cannot be null or empty.");
            }

            // TODO: Implement query logic here
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {IdentifierElement.ElementName} element.");
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

                // Call the processing instruction handler generically for all PIs
                pi.ProcessingInstructionHandler();
#if DEBUG
                if (pi is IfProcessingInstruction dbgIfCreated) {
                    #if DEBUG
                    Console.WriteLine($"[TRACE-PI][CREATE] IF PI created cond='{dbgIfCreated.ConditionExpression}' met={dbgIfCreated.ConditionMet} unknown={dbgIfCreated.UnknownOperator} suppressSer={dbgIfCreated.SuppressSerialization}");
                    #endif
                }
#endif

                // Policy update: strip all If PIs from serialization regardless of outcome (success, failure, unknown op)
                // per specified behavior that conditional directives are non-material after evaluation.
                if (pi is IfProcessingInstruction) {
                    pi.SuppressSerialization = true;
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

        // Create an ArrayElement with homogeneous type checking enforced by ArrayElement.Add()
        ArrayElement arrayElement;
        try {
            arrayElement = new ArrayElement(style: style);
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to create array element. {ex.Message}", ex);
        }

        // Apply any pending PIs from higher level parsing to this array element
        foreach (var pendingPI in _pendingPIs)
        {
            pendingPI.Target = arrayElement;
            pendingPI.ElementHandler(arrayElement);
        }
        _pendingPIs.Clear();

        // Local pending PIs for this array's parsing context
        var localPendingPIs = new List<ProcessingInstruction>();

        if (IsCharAvailable()) {
            if (ArrayElementClosing()) {
                return arrayElement;
            }

            // Collect all consecutive processing instructions first
            while (IsCharAvailable() && ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount)) {
                var pi = ParseProcessingInstruction(piSpecifierCount);
                // Add PI to array for serialization
                arrayElement.Add(pi);
                // Also add to local pending PIs for application to next element
                localPendingPIs.Add(pi);
                if (pi is ScriptProcessingInstruction spiEarly1) { spiEarly1.ExecuteTopLevelEarlyBindings(); } else if (pi is LetProcessingInstruction lpiEarly1) { lpiEarly1.ExecuteEarly(); }
                SkipWhitespace();
            }

            // Now parse the target element if available
            if (IsCharAvailable() && !ArrayElementClosing()) {
                Element element;
                try {
                    element = ParseElement();

                    // Apply any local pending PIs to this element
                    bool shouldAddElement = true;
#if DEBUG
                    #if DEBUG
                    Console.WriteLine($"[TRACE-PARSE][ARRAY-HEAD] Applying {localPendingPIs.Count} pending PIs to element type={element.GetType().Name}");
                    #endif
#endif
                    foreach (var pendingPI in localPendingPIs) {
                        pendingPI.Target = element;
                        try {
#if DEBUG
                            if (pendingPI is IfProcessingInstruction dbgIf1) {
                                #if DEBUG
                                Console.WriteLine($"[TRACE-PARSE][ARRAY-HEAD] About to apply IF PI cond='{dbgIf1.ConditionExpression}' conditionMet={dbgIf1.ConditionMet} unknown={dbgIf1.UnknownOperator} targetType={element.GetType().Name}");
                                #endif
                            }
#endif
                            pendingPI.ElementHandler(element);
                        }
                        catch (ConditionalElementException) {
#if DEBUG
                            if (pendingPI is IfProcessingInstruction dbgIfFail1) {
                                #if DEBUG
                                Console.WriteLine($"[TRACE-PARSE][ARRAY-HEAD] IF PI suppressed element cond='{dbgIfFail1.ConditionExpression}'");
                                #endif
                            }
#endif
                            // PI indicates this element should not be added
                            shouldAddElement = false;
                            break;
                        }
                    }
                    // If element suppressed, remove any failed conditional IF PIs from the parent container so they don't serialize orphaned
                    // Policy: retain failed conditional IF PIs for visibility; do not remove them
                    localPendingPIs.Clear();

                    // Only add the element if all PIs approved it
                    if (shouldAddElement) {
                        arrayElement.Add(element);
                        // Legacy let binding processing removed.
                    }
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to parse first array element. {ex.Message}", ex);
                }
            }

        while (IsCharAvailable()) {
            if (ArrayElementClosing()) {
                return arrayElement;
            }

            // Collect all consecutive processing instructions first
            while (IsCharAvailable() && ElementOpening(ProcessingInstruction.ElementDelimiter, out int piSpecifierCount2)) {
                var pi = ParseProcessingInstruction(piSpecifierCount2);
                // Add PI to array for serialization
                arrayElement.Add(pi);
                // Also add to local pending PIs for application to next element
                localPendingPIs.Add(pi);
                if (pi is ScriptProcessingInstruction spiEarly2) { spiEarly2.ExecuteTopLevelEarlyBindings(); } else if (pi is LetProcessingInstruction lpiEarly2) { lpiEarly2.ExecuteEarly(); }
                SkipWhitespace();
            }

            // Now parse the target element if available
            if (IsCharAvailable() && !ArrayElementClosing()) {
                try {
                    var element = ParseElement();

                    // Apply any local pending PIs to this element
                    bool shouldAddElement = true;
#if DEBUG
                    #if DEBUG
                    Console.WriteLine($"[TRACE-PARSE][ARRAY] Applying {localPendingPIs.Count} pending PIs to element type={element.GetType().Name}");
                    #endif
#endif
                    foreach (var pendingPI in localPendingPIs) {
                        pendingPI.Target = element;
                        try {
#if DEBUG
                            if (pendingPI is IfProcessingInstruction dbgIf2) {
                                #if DEBUG
                                Console.WriteLine($"[TRACE-PARSE][ARRAY] About to apply IF PI cond='{dbgIf2.ConditionExpression}' conditionMet={dbgIf2.ConditionMet} unknown={dbgIf2.UnknownOperator} targetType={element.GetType().Name}");
                                #endif
                            }
#endif
                            pendingPI.ElementHandler(element);
                        }
                        catch (ConditionalElementException) {
#if DEBUG
                            if (pendingPI is IfProcessingInstruction dbgIfFail2) {
                                #if DEBUG
                                Console.WriteLine($"[TRACE-PARSE][ARRAY] IF PI suppressed element cond='{dbgIfFail2.ConditionExpression}'");
                                #endif
                            }
#endif
                            // PI indicates this element should not be added
                            shouldAddElement = false;
                            break;
                        }
                    }
                    // Policy: retain failed conditional IF PIs for visibility
                    localPendingPIs.Clear();

                    // Only add the element if all PIs approved it
                    if (shouldAddElement) {
                        arrayElement.Add(element);
                        // Legacy let binding processing removed.
                    }
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"At row {lastElementRow}, column {lastElementColumn}: Failed to parse array element. {ex.Message}", ex);
                }
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

        // Apply any pending PIs from higher level parsing to this object element
        foreach (var pendingPI in _pendingPIs)
        {
            pendingPI.Target = objectElement;
            pendingPI.ElementHandler(objectElement);
        }
        _pendingPIs.Clear();

        // Local pending PIs for this object's parsing context
        var localPendingPIs = new List<ProcessingInstruction>();

        while (IsCharAvailable()) {
            // Allow whitespace before closing or next member
            SkipWhitespace();
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
                // Also add to local pending PIs for application to next element
                localPendingPIs.Add(pi);
                SkipWhitespace();
                continue;
            }

            var element = ParseElement();

            // Apply any local pending PIs to this element (if it's not a PI itself)
            bool shouldAddElement = true;
            if (!(element is ProcessingInstruction)) {
                foreach (var pendingPI in localPendingPIs) {
                    pendingPI.Target = element;
                    try {
#if DEBUG
                        if (pendingPI is IfProcessingInstruction dbgIfObj) {
                            #if DEBUG
                            Console.WriteLine($"[TRACE-PARSE][OBJECT] About to apply IF PI cond='{dbgIfObj.ConditionExpression}' conditionMet={dbgIfObj.ConditionMet} unknown={dbgIfObj.UnknownOperator} targetType={element.GetType().Name}");
                            #endif
                        }
#endif
                        pendingPI.ElementHandler(element);
                    }
                    catch (ConditionalElementException) {
#if DEBUG
                        if (pendingPI is IfProcessingInstruction dbgIfFailObj) {
                            #if DEBUG
                            Console.WriteLine($"[TRACE-PARSE][OBJECT] IF PI suppressed element cond='{dbgIfFailObj.ConditionExpression}'");
                            #endif
                        }
#endif
                        // PI indicates this element should not be added
                        shouldAddElement = false;
                        break;
                    }
                }
                // Policy: retain failed conditional IF PIs for visibility; do not remove
                localPendingPIs.Clear();
            }

            // Only process the element if PIs approved it
            if (shouldAddElement) {
                if (element is KeyValuePairElement kvp) {
                    if (objectElement.ContainsKey(kvp.Key)) {
                        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Duplicate key '{kvp.Key}' in object.");
                    }
                    objectElement.AddOrUpdate(kvp);
                }
                else if (element is ProcessingInstruction meta) {
                    objectElement.AddOrUpdate(meta);
                    if (meta is ScriptProcessingInstruction spiEarly) {
                        spiEarly.ExecuteTopLevelEarlyBindings();
                    }
                    else if (meta is LetProcessingInstruction lpiEarly) {
                        lpiEarly.ExecuteEarly();
                    }
                }
                else if (element is EmptyElement) {
                    // Ignore empty elements in objects
                    continue;
                }
                else if (element is IdentifierElement identifierElement) {
                    // IdentifierElement found without a key - this shouldn't happen in well-formed XferLang
                    // but we'll handle it by creating a temporary KVP
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Standalone IdentifierElement '{identifierElement.Value}' found in object. IdentifierElements must be values in key-value pairs.");
                }
                else {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected element type '{element.GetType().Name}'. Objects can only contain KeyValuePairElement, ProcessingInstruction, or EmptyElement. Found: {element}");
                }
                // Legacy let processing removed.
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
                // Deactivate any script scopes targeting this tuple now that its contents are fully parsed
                foreach (var child in tupleElement.Children) {
                    if (child is ScriptProcessingInstruction spi && spi.Target == tupleElement) {
                        // No scope deactivation required.
                    }
                }
                return tupleElement;
            }

            var element = ParseElement();

            if (element is ProcessingInstruction pi) {
                // Add PI to tuple and track it for target assignment
                tupleElement.Add(pi);
                pendingPIs.Add(pi);
                // For script PIs, execute let bindings immediately so they are visible
                // to the very next sibling element (e.g., an interpolated string)
                if (pi is ScriptProcessingInstruction spiEarly) {
                    spiEarly.ExecuteTopLevelEarlyBindings(); // safe re-entry guarded internally
                }
                else if (pi is LetProcessingInstruction lpiEarly) {
                    lpiEarly.ExecuteEarly();
                }
            } else {
                // Non-PI element: set it as target for any pending PIs
                bool shouldAddElement = true;
                foreach (var pendingPI in pendingPIs) {
                    pendingPI.Target = element;
                    try {
                        if (pendingPI is ScriptProcessingInstruction spi) {
                            // Ensure scope active & operators executed before handling element
                            // Already executed.
                        }
#if DEBUG
                        if (pendingPI is IfProcessingInstruction dbgIfTuple) {
                            #if DEBUG
                            Console.WriteLine($"[TRACE-PARSE][TUPLE] About to apply IF PI cond='{dbgIfTuple.ConditionExpression}' conditionMet={dbgIfTuple.ConditionMet} unknown={dbgIfTuple.UnknownOperator} targetType={element.GetType().Name}");
                            #endif
                        }
#endif
                        pendingPI.ElementHandler(element);
                    }
                    catch (ConditionalElementException) {
#if DEBUG
                        if (pendingPI is IfProcessingInstruction dbgIfFailTuple) {
                            #if DEBUG
                            Console.WriteLine($"[TRACE-PARSE][TUPLE] IF PI suppressed element cond='{dbgIfFailTuple.ConditionExpression}'");
                            #endif
                        }
#endif
                        shouldAddElement = false;
                        break;
                    }
                }
                // Policy: retain failed conditional IF PIs for visibility
                pendingPIs.Clear();

                if (shouldAddElement) {
                    tupleElement.Add(element);
                }
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
                }            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DynamicElement.ElementName} element.");
    }

    private IdentifierElement ParseIdentifierElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        var key = string.Empty;

        StringBuilder valueBuilder = new StringBuilder();

        while (IsCharAvailable()) {
            if (IdentifierElementClosing()) {
                return new IdentifierElement(valueBuilder.ToString().Normalize(NormalizationForm.FormC), specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {IdentifierElement.ElementName} element.");
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

    private KeyValuePairElement ParseKeyValuePairElement(KeywordElement keyElement) {
        var keyValuePairElement = new KeyValuePairElement(keyElement);

        if (IsCharAvailable()) {
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

            return keyValuePairElement;
        }

        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {KeyValuePairElement.ElementName} element.");
    }

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

        int codePoint = PlaceholderCharacter;
        var numericValue = new NumericValue<int>(codePoint, NumericBase.Hexadecimal);

        string charString = charContent.ToString();
        if (string.IsNullOrEmpty(charString)) {
            // Add warning for empty character element and use replacement character
            AddWarningAtElementStart(WarningType.EmptyCharacterElement,
                      $"Empty character element, using '{PlaceholderCharacter}' as replacement",
                      null);
            return new CharacterElement(numericValue, specifierCount, style: style);
        }


        if (char.IsLetter(charString[0])) {
            var resolved = CharacterIdRegistry.Resolve(charString);

            if (resolved.HasValue) {
                codePoint = resolved.Value;
                numericValue.Value = codePoint;
            }
            else {
                // Add warning for unresolved character name
                AddWarningAtElementStart(WarningType.CharacterResolutionFailure,
                          $"Unknown character name '{charString}', using '{PlaceholderCharacter}' as fallback",
                          charString);
                codePoint = PlaceholderCharacter;
            }
        }
        else {
            numericValue = ParseNumericValue<int>(charString);

            if (numericValue.HasValue) {
                codePoint = numericValue.Value;
            }
            else {
                // Add warning for invalid character value
                AddWarningAtElementStart(WarningType.InvalidCharacterValue,
                          $"Invalid character value '{charString}', using '{PlaceholderCharacter}' as fallback",
                          charString);
                codePoint = PlaceholderCharacter;
            }
        }

        return new CharacterElement(numericValue, specifierCount, style: style);
    }

    private static string ElementToInterpolatedText(Element e) {
        return e switch {
            StringElement s => s.Value, // omit quotes in interpolation
            InterpolatedElement i => i.Value,
            IntegerElement i2 => i2.Value.ToString(CultureInfo.InvariantCulture),
            LongElement l => l.Value.ToString(CultureInfo.InvariantCulture),
            DecimalElement d => d.Value.ToString(CultureInfo.InvariantCulture),
            DoubleElement dbl => dbl.Value.ToString(CultureInfo.InvariantCulture),
            BooleanElement b => b.Value.ToString(),
            DateTimeElement dt => dt.Value.ToString("o", CultureInfo.InvariantCulture),
            DateElement da => da.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeElement tm => tm.Value.ToString(),
            TimeSpanElement ts => ts.Value.ToString(),
            _ => e.ToString()
        } ?? string.Empty;
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

            if (ElementExplicitOpening(ReferenceElement.ElementDelimiter, out int referenceElementCount)) {
                Element referenceElement = ParseReferenceElement(referenceElementCount);
                // If unresolved during ParseReferenceElement (returned ReferenceElement), just preserve the token;
                // immediate resolution has already been attempted there.
                if (referenceElement is ReferenceElement unresolved) {
                    valueBuilder.Append(unresolved.ToXfer(Formatting.None));
                }
                else {
                    // Already resolved to a cloned element; use its textual representation (unquoted for strings)
                    valueBuilder.Append(ElementToInterpolatedText(referenceElement));
                }
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
                Element dateElement = ParseTemporalElement(dateSpecifierCount);

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

            if (InterpolatedElementClosing()) {
                var raw = valueBuilder.ToString();
                return new InterpolatedElement(raw.Normalize(NormalizationForm.FormC), specifierCount, style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style (consistent with other scalar parsers)
        if (style == ElementStyle.Compact || style == ElementStyle.Implicit) {
            var rawAtEof = valueBuilder.ToString();
            return new InterpolatedElement(rawAtEof.Normalize(NormalizationForm.FormC), specifierCount, style);
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

    private Element ParseTemporalElement(int specifierCount = 1) {
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

            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid integer value '{valueBuilder}'.");
            }
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

            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid long integer value '{valueBuilder}'.");
            }

            return new LongElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (LongElementClosing()) {
                var value = ParseNumericValue<long>(valueBuilder.ToString());

                if (!value.HasValue) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid long integer value '{valueBuilder}'.");
                }

                return new LongElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<long>(valueBuilder.ToString());

            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid long integer value '{valueBuilder}'.");
            }

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
            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid decimal value '{valueBuilder}'.");
            }
            return new DecimalElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (DecimalElementClosing()) {
                var value = ParseNumericValue<decimal>(valueBuilder.ToString());
                if (!value.HasValue) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid decimal value '{valueBuilder}'.");
                }
                return new DecimalElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<decimal>(valueBuilder.ToString());
            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid decimal value '{valueBuilder}'.");
            }
            return new DecimalElement(value, specifierCount, style: style);
        }
        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unexpected end of {DecimalElement.ElementName} element.");
    }

    private Element ParseReferenceElement(int specifierCount = 1) {
        var style = _delimStack.Peek().Style;
        var sb = new StringBuilder();

        while (IsCharAvailable()) {
            if (ReferenceElementClosing()) {
                break;
            }

            sb.Append(CurrentChar);
            Expand();
        }

        var name = sb.ToString();

        if (string.IsNullOrEmpty(name)) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Dereference must specify a name after '{ReferenceElement.OpeningSpecifier}'.");
        }

        if (TryResolveBinding(name, out var bound)) {
            if (_currentDocument != null) {
                // Keep trace at current cursor; it reflects resolution moment
                _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] deref '{name}' resolved immediately", CurrentRow, CurrentColumn, name));
            }
            return Helpers.ElementCloner.Clone(bound!);
        }

        if (_currentDocument != null) {
            // Anchor the trace to the start of the element so caret navigation points to the token
            _currentDocument.Warnings.Add(new ParseWarning(WarningType.Trace, $"[trace] deref '{name}' unresolved (will attempt post-pass)", LastElementRow, LastElementColumn, name));
        }

        // Primary warning: point to the deref token start
        AddWarningAtElementStart(WarningType.UnresolvedReference, $"Unresolved reference '{name}'", name);
        return new ReferenceElement(name, specifierCount, style);
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

            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid double value '{valueBuilder}'.");
            }

            return new DoubleElement(value, specifierCount, style: style);
        }

        while (IsCharAvailable()) {
            if (DoubleElementClosing()) {
                var value = ParseNumericValue<double>(valueBuilder.ToString());

                if (!value.HasValue) {
                    throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid double value '{valueBuilder}'.");
                }

                return new DoubleElement(value, specifierCount, style: style);
            }

            valueBuilder.Append(CurrentChar);
            Expand();
        }
        // If we reach end of input, treat as closed for compact/implicit style
        if (valueBuilder.Length > 0 && (style == ElementStyle.Compact || style == ElementStyle.Implicit)) {
            var value = ParseNumericValue<double>(valueBuilder.ToString());

            if (!value.HasValue) {
                throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Invalid double value '{valueBuilder}'.");
            }

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

    private NumericValue<T> ParseNumericValue<T>(string valueString) where T : struct, IConvertible {
        var returnValue = new NumericValue<T>();

        if (string.IsNullOrEmpty(valueString)) {
            return returnValue;
        }

        char basePrefix = valueString[0];
        string numberString = valueString;

        /* Determine the base (decimal, hexadecimal, or binary). */
        int numberBase = basePrefix switch {
            Element.HexadecimalPrefix => 16,
            Element.BinaryPrefix => 2,
            _ => 10
        };

        returnValue.Base = (NumericBase)numberBase;

        if (numberBase != 10) {
            numberString = valueString.Substring(1); /* Remove base prefix character */
        }

        try {
            switch (numberBase) {
                case 10: {
                        if (typeof(T) == typeof(float)) {
                            returnValue.Value = (T) Convert.ChangeType(float.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(double)) {
                            returnValue.Value = (T) Convert.ChangeType(double.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(short)) {
                            returnValue.Value = (T) Convert.ChangeType(short.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(char)) {
                            returnValue.Value = (T) Convert.ChangeType(int.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(int)) {
                            returnValue.Value = (T) Convert.ChangeType(int.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(long)) {
                            returnValue.Value = (T) Convert.ChangeType(long.Parse(numberString, NumberStyles.Integer, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else if (typeof(T) == typeof(decimal)) {
                            returnValue.Value = (T) Convert.ChangeType(decimal.Parse(numberString, CultureInfo.InvariantCulture), typeof(T));
                        }
                        else {
                            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unsupported type '{typeof(T)}' for decimal value parsing.");
                        }

                        break;
                    }
                case 16: {
                        long hexValue = long.Parse(numberString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        returnValue.Value = (T) Convert.ChangeType(hexValue, typeof(T));
                        break;
                    }
                case 2: {
                        long binaryValue = long.Parse(numberString, NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
                        returnValue.Value = (T) Convert.ChangeType(binaryValue, typeof(T));
                        break;
                    }
                default: {
                        throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Unsupported numeric base '{numberBase}'.");
                    }
            }

            return returnValue;
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"At row {CurrentRow}, column {CurrentColumn}: Failed to parse numeric value '{valueString}'. Error: {ex.Message}");
        }
    }

    // Legacy let binding processing removed (immediate substitution model now active).
}
