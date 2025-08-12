using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for document-level metadata and configuration in XferLang.
/// Contains document properties, schema information, versioning, and other metadata
/// that applies to the entire XferLang document. The instruction expects an object
/// containing document configuration parameters.
/// </summary>
public class DocumentProcessingInstruction : ProcessingInstruction {
    // Centralized property keys
    public static class PropertyKeys {
        public const string XferLang = "xferlang";
        public const string Title = "title";
        public const string Description = "description";
        public const string Version = "version";
        public const string Id = "id";
        public const string Author = "author";
        public const string Authors = "authors";
        public const string Tags = "tags";
        public const string Profile = "profile";
        public const string Environment = "environment";
        public const string License = "license";
        public const string CreatedAt = "createdAt";
        public const string UpdatedAt = "updatedAt";
    }
    /// <summary>
    /// The keyword used to identify document processing instructions.
    /// </summary>
    public const string Keyword = "document";

    /// <summary>
    /// Initializes a new instance of the DocumentProcessingInstruction class with the specified object value.
    /// </summary>
    /// <param name="value">The object element containing document configuration parameters.</param>
    public DocumentProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    /// <summary>
    /// Initializes a new instance of the DocumentProcessingInstruction class with an empty object value.
    /// </summary>
    public DocumentProcessingInstruction() : base(new ObjectElement(), Keyword) { }

    /// <summary>
    /// The underlying object that stores all document properties.
    /// </summary>
    public ObjectElement Object => (ObjectElement) Kvp.Value;

    // --- Core document metadata (as XferLang elements) ---

    /// <summary>
    /// The version of XferLang targeted by this document.
    /// </summary>
    public StringElement? XferLang {
        get => Get<StringElement>(PropertyKeys.XferLang);
        set => SetOrRemove(PropertyKeys.XferLang, value);
    }

    /// <summary>Document title/name.</summary>
    public StringElement? Title {
        get => Get<StringElement>(PropertyKeys.Title);
        set => SetOrRemove(PropertyKeys.Title, value);
    }

    /// <summary>Short description or summary.</summary>
    public StringElement? Description {
        get => Get<StringElement>(PropertyKeys.Description);
        set => SetOrRemove(PropertyKeys.Description, value);
    }

    /// <summary>Human or semantic version string.</summary>
    public StringElement? Version {
        get => Get<StringElement>(PropertyKeys.Version);
        set => SetOrRemove(PropertyKeys.Version, value);
    }

    /// <summary>Document identifier (string or GUID as string).</summary>
    public StringElement? DocumentId {
        get => Get<StringElement>(PropertyKeys.Id);
        set => SetOrRemove(PropertyKeys.Id, value);
    }

    /// <summary>Primary author/owner (use Authors for multiples).</summary>
    public StringElement? Author {
        get => Get<StringElement>(PropertyKeys.Author);
        set => SetOrRemove(PropertyKeys.Author, value);
    }

    /// <summary>Authors list.</summary>
    public ArrayElement? Authors {
        get => Get<ArrayElement>(PropertyKeys.Authors);
        set => SetOrRemove(PropertyKeys.Authors, value);
    }

    /// <summary>Freeform tags/labels.</summary>
    public ArrayElement? Tags {
        get => Get<ArrayElement>(PropertyKeys.Tags);
        set => SetOrRemove(PropertyKeys.Tags, value);
    }

    /// <summary>Profile or variant identifier.</summary>
    public StringElement? Profile {
        get => Get<StringElement>(PropertyKeys.Profile);
        set => SetOrRemove(PropertyKeys.Profile, value);
    }

    /// <summary>Environment name (e.g., dev/test/prod).</summary>
    public StringElement? Environment {
        get => Get<StringElement>(PropertyKeys.Environment);
        set => SetOrRemove(PropertyKeys.Environment, value);
    }

    /// <summary>License identifier or text.</summary>
    public StringElement? License {
        get => Get<StringElement>(PropertyKeys.License);
        set => SetOrRemove(PropertyKeys.License, value);
    }

    /// <summary>Creation timestamp.</summary>
    public DateTimeElement? CreatedAt {
        get => Get<DateTimeElement>(PropertyKeys.CreatedAt);
        set => SetOrRemove(PropertyKeys.CreatedAt, value);
    }

    /// <summary>Last updated timestamp.</summary>
    public DateTimeElement? UpdatedAt {
        get => Get<DateTimeElement>(PropertyKeys.UpdatedAt);
        set => SetOrRemove(PropertyKeys.UpdatedAt, value);
    }

    // --- Extensibility helpers ---

    /// <summary>Indexer to access custom properties directly.</summary>
    public Element this[string key] {
        get => Object[key];
        set => Object[key] = value;
    }

    /// <summary>
    /// Try get a custom element by key and type.
    /// </summary>
    public bool TryGetCustom<TElement>(string key, out TElement? element) where TElement : Element
        => Object.TryGetElement<TElement>(key, out element);

    /// <summary>
    /// Set a custom element value by key.
    /// </summary>
    public void SetCustom(string key, Element element) => Object[key] = element;

    /// <summary>
    /// Remove a property by key.
    /// </summary>
    public bool Remove(string key) => Object.Remove(key);

    // --- Public helpers for clean usage/round-trip ---

    /// <summary>
    /// Get a typed element by key.
    /// </summary>
    public TElement? Get<TElement>(string key) where TElement : Element
        => Object.TryGetElement<TElement>(key, out var element) ? element : null;

    /// <summary>
    /// Set a typed element by key.
    /// </summary>
    public void Set(string key, Element? element) => SetOrRemove(key, element);

    /// <summary>
    /// Remove a property by key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool RemoveProperty(string key) => Object.Remove(key);

    /// <summary>
    /// Gets all properties as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, KeyValuePairElement> Properties => Object.Dictionary;

    /// <summary>
    /// Convert to a dictionary of element values.
    /// </summary>
    public Dictionary<string, Element> ToDictionary() =>
        Object.Dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

    /// <summary>
    /// Create an instance from a dictionary of element values.
    /// </summary>
    public static DocumentProcessingInstruction From(IDictionary<string, Element> properties) {
        var obj = new ObjectElement();
        foreach (var kvp in properties) {
            obj[kvp.Key] = kvp.Value;
        }
        return new DocumentProcessingInstruction(obj);
    }

    /// <summary>
    /// Create a new instance with the specified configuration.
    /// </summary>
    public static DocumentProcessingInstruction Create(Action<ObjectElement> configure) {
        var obj = new ObjectElement();
        configure(obj);
        return new DocumentProcessingInstruction(obj);
    }

    /// <summary>
    /// Configure the document processing instruction.
    /// </summary>
    public DocumentProcessingInstruction Configure(Action<ObjectElement> configure) {
        configure(Object);
        return this;
    }

    private void SetOrRemove(string key, Element? element) {
        if (element == null) { Object.Remove(key); return; }
        Object[key] = element;
    }
}
