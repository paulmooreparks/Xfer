# LINQ to Xfer - Design Specification

## 1. Overview

LINQ to Xfer will provide a powerful, fluent API for querying and manipulating XferLang documents without requiring full deserialization to POCOs. This API will be modeled after the successful patterns established by LINQ to JSON and LINQ to XML, adapted for XferLang's unique features.

## 2. Design Goals

### Primary Goals
- **Developer Productivity**: Intuitive, fluent API that feels natural to .NET developers
- **Performance**: Efficient querying without unnecessary object allocation or full deserialization
- **Type Safety**: Strong typing where possible, with runtime safety for dynamic operations
- **XferLang Feature Support**: Full support for XferLang's unique features (explicit typing, processing instructions, IDs)
- **LINQ Integration**: Native support for LINQ operators (Where, Select, etc.)

### Secondary Goals
- **Compatibility**: Seamless integration with existing XferDocument/Element hierarchy
- **Extensibility**: Support for custom query operations and extensions
- **Debugging**: Rich ToString() implementations and meaningful error messages

## 3. Core Architecture

### 3.1 Main Wrapper Classes

```csharp
// Base wrapper class for all XferLang elements
public abstract class XValue : IEnumerable<XValue>
{
    protected Element _element;

    // Core conversion methods
    public static implicit operator XValue(Element element);
    public static explicit operator Element(XValue value);

    // Value access with type safety
    public T Value<T>();
    public object? Value { get; }

    // Hierarchy navigation
    public XValue? Parent { get; }
    public IEnumerable<XValue> Children { get; }

    // Element type checking
    public XferElementType ElementType { get; }
    public bool IsCollection { get; }
    public bool IsValue { get; }

    // XferLang-specific features
    public string? Id { get; }
    public IEnumerable<XProcessingInstruction> ProcessingInstructions { get; }
}

// Collection wrapper (base for XObject, XArray, XTuple)
public abstract class XCollection : XValue
{
    // Common collection operations
    public int Count { get; }
    public bool IsEmpty { get; }
    public XValue? this[int index] { get; }

    // LINQ-style operations
    public IEnumerable<XValue> Values();
    public IEnumerable<XValue> Descendants();
    public IEnumerable<XValue> DescendantsAndSelf();
}

// Object wrapper for ObjectElement
public class XObject : XCollection
{
    // Key-based access
    public XValue? this[string key] { get; set; }

    // Object-specific operations
    public IEnumerable<string> Keys { get; }
    public IEnumerable<XProperty> Properties();
    public XValue? Property(string name);
    public bool ContainsKey(string key);

    // Modification methods
    public void Add(string key, XValue value);
    public void Remove(string key);
    public bool TryGetValue(string key, out XValue? value);

    // Creation from anonymous objects
    public static XObject FromObject(object obj);
}

// Array wrapper for ArrayElement (homogeneous collections)
public class XArray : XCollection
{
    // Array-specific operations
    public Type? ElementType { get; }
    public bool IsHomogeneous { get; }

    // Modification methods
    public void Add(XValue value);
    public void Insert(int index, XValue value);
    public void RemoveAt(int index);

    // Type-safe enumeration
    public IEnumerable<T> Values<T>();
}

// Tuple wrapper for TupleElement (heterogeneous collections)
public class XTuple : XCollection
{
    // Tuple-specific operations (mixed types allowed)
    public void Add(XValue value);
    public void Insert(int index, XValue value);
    public void RemoveAt(int index);
}
```

### 3.2 Value Wrapper Classes

```csharp
// Primitive value wrappers
public class XString : XValue
{
    public string Value { get; }
    public static implicit operator string(XString value);
    public static implicit operator XString(string value);
}

public class XInteger : XValue
{
    public int Value { get; }
    public XferNumericFormat Format { get; } // Hex, Binary, Decimal
    public static implicit operator int(XInteger value);
    public static implicit operator XInteger(int value);
}

public class XDecimal : XValue
{
    public decimal Value { get; }
    public int? Precision { get; }
    public static implicit operator decimal(XDecimal value);
    public static implicit operator XDecimal(decimal value);
}

public class XBoolean : XValue
{
    public bool Value { get; }
    public static implicit operator bool(XBoolean value);
    public static implicit operator XBoolean(bool value);
}

public class XDateTime : XValue
{
    public DateTime Value { get; }
    public DateTimeHandling Handling { get; }
    public static implicit operator DateTime(XDateTime value);
    public static implicit operator XDateTime(DateTime value);
}

public class XNull : XValue
{
    public static readonly XNull Instance = new();
}

// XferLang-specific wrappers
public class XDynamic : XValue
{
    public string Key { get; }
    public string? ResolvedValue { get; }
    public bool IsResolved { get; }
}

public class XInterpolated : XValue
{
    public string Template { get; }
    public IEnumerable<XValue> EmbeddedValues { get; }
    public string ResolvedValue { get; }
}

public class XCharacter : XValue
{
    public char Value { get; }
    public int CodePoint { get; }
    public CharacterRepresentation Representation { get; } // Decimal, Hex, Binary, Keyword
}
```

### 3.3 Utility Classes

```csharp
// Property wrapper for key-value pairs in objects
public class XProperty
{
    public string Name { get; }
    public XValue Value { get; set; }
    public Element KeyElement { get; } // Access to original keyword element
    public Element ValueElement { get; } // Access to original value element
}

// Processing instruction wrapper
public class XProcessingInstruction
{
    public string Name { get; }
    public XValue Content { get; }
    public Element? Target { get; } // Element this PI applies to
}

// Element type enumeration
public enum XferElementType
{
    Object, Array, Tuple,
    String, Integer, Long, Double, Decimal, Boolean,
    DateTime, Date, Time, TimeSpan,
    Character, Null, Dynamic, Interpolated,
    Comment, ProcessingInstruction
}

// Numeric format enumeration
public enum CharacterRepresentation
{
    Decimal, Hexadecimal, Binary, Keyword
}
```

## 4. Query Operations

### 4.1 Standard LINQ Support

```csharp
// Extension methods to make XValue collections LINQ-compatible
public static class XferLinqExtensions
{
    // Collection filtering and selection
    public static IEnumerable<XValue> Where(this XValue source, Func<XValue, bool> predicate);
    public static IEnumerable<TResult> Select<TResult>(this XValue source, Func<XValue, TResult> selector);
    public static IEnumerable<XValue> SelectMany(this XValue source, Func<XValue, IEnumerable<XValue>> selector);

    // Type-specific filtering
    public static IEnumerable<XObject> Objects(this IEnumerable<XValue> source);
    public static IEnumerable<XArray> Arrays(this IEnumerable<XValue> source);
    public static IEnumerable<XTuple> Tuples(this IEnumerable<XValue> source);
    public static IEnumerable<T> Values<T>(this IEnumerable<XValue> source);

    // Hierarchy navigation
    public static IEnumerable<XValue> Children(this XValue source, string? name = null);
    public static IEnumerable<XValue> Descendants(this XValue source, string? name = null);
    public static IEnumerable<XValue> Ancestors(this XValue source, string? name = null);

    // XferLang-specific queries
    public static IEnumerable<XValue> WithId(this IEnumerable<XValue> source, string id);
    public static XValue? GetElementById(this XValue source, string id);
    public static IEnumerable<XProcessingInstruction> ProcessingInstructions(this XValue source, string? name = null);
}
```

### 4.2 XferLang-Specific Query Methods

```csharp
public static class XferQueryExtensions
{
    // Type-safe value extraction
    public static IEnumerable<string> StringValues(this IEnumerable<XValue> source);
    public static IEnumerable<int> IntValues(this IEnumerable<XValue> source);
    public static IEnumerable<decimal> DecimalValues(this IEnumerable<XValue> source);

    // Element format queries
    public static IEnumerable<XInteger> HexIntegers(this IEnumerable<XValue> source);
    public static IEnumerable<XInteger> BinaryIntegers(this IEnumerable<XValue> source);
    public static IEnumerable<XDateTime> UtcDateTimes(this IEnumerable<XValue> source);

    // Collection type filtering
    public static IEnumerable<XArray> HomogeneousArrays(this IEnumerable<XValue> source);
    public static IEnumerable<XArray> ArraysOfType<T>(this IEnumerable<XValue> source);

    // Path-based queries (JSONPath-like syntax)
    public static IEnumerable<XValue> SelectTokens(this XValue source, string path);
    public static XValue? SelectToken(this XValue source, string path);

    // Dynamic content queries
    public static IEnumerable<XDynamic> UnresolvedDynamicElements(this IEnumerable<XValue> source);
    public static IEnumerable<XValue> ResolveAll(this IEnumerable<XValue> source);
}
```

## 5. Creation and Modification API

### 5.1 Fluent Creation API

```csharp
public static class XferBuilder
{
    // Object creation
    public static XObject Object() => new XObject();
    public static XObject Object(object properties) => XObject.FromObject(properties);
    public static XObject Object(params (string key, XValue value)[] properties);

    // Array creation
    public static XArray Array<T>(params T[] values);
    public static XArray Array(params XValue[] values);

    // Tuple creation
    public static XTuple Tuple(params XValue[] values);

    // Value creation with explicit formatting
    public static XInteger Integer(int value, XferNumericFormat format = XferNumericFormat.Decimal);
    public static XString String(string value, ElementStyle style = ElementStyle.Compact);
    public static XDateTime DateTime(DateTime value, DateTimeHandling handling = DateTimeHandling.RoundTrip);

    // Processing instruction creation
    public static XProcessingInstruction PI(string name, XObject content);
    public static XProcessingInstruction IdPI(string id);
}
```

### 5.2 Modification Operations

```csharp
// Extension methods for modifying XferLang documents
public static class XferModificationExtensions
{
    // In-place modifications
    public static XObject AddProperty(this XObject obj, string key, XValue value);
    public static XObject RemoveProperty(this XObject obj, string key);
    public static XObject SetProperty(this XObject obj, string key, XValue value);

    // Collection modifications
    public static XArray AddItem(this XArray array, XValue item);
    public static XTuple AddItem(this XTuple tuple, XValue item);
    public static XCollection RemoveAt(this XCollection collection, int index);

    // Bulk operations
    public static XObject Merge(this XObject target, XObject source);
    public static XArray AddRange(this XArray array, IEnumerable<XValue> items);

    // Processing instruction operations
    public static XValue AddProcessingInstruction(this XValue value, XProcessingInstruction pi);
    public static XValue SetId(this XValue value, string id);
}
```

## 6. Serialization Integration

```csharp
// Integration with existing XferConvert API
public static class XferConvertExtensions
{
    // Convert between XValue and Element hierarchies
    public static XValue ToXValue(this Element element);
    public static Element ToElement(this XValue value);

    // Serialize XValue directly
    public static string Serialize(this XValue value, Formatting formatting = Formatting.None);
    public static string Serialize(this XValue value, XferSerializerSettings settings);

    // Parse to XValue
    public static XValue ParseXValue(string xfer);
    public static XValue ParseXValue(string xfer, XferSerializerSettings settings);

    // Deserialize with LINQ support
    public static T ToObject<T>(this XValue value);
    public static T ToObject<T>(this XValue value, XferSerializerSettings settings);
}
```

## 7. Usage Examples

### 7.1 Basic Querying

```csharp
// Parse a document and query it
var doc = XferDocument.Parse(xferString);
var root = doc.Root.ToXValue();

// Find all users with age > 30
var adults = root["users"]
    .Children()
    .Where(user => user["age"].Value<int>() > 30)
    .ToList();

// Get all email addresses
var emails = root
    .Descendants()
    .Where(x => x.ElementType == XferElementType.String)
    .Where(x => x.Parent?.Property("email") != null)
    .Select(x => x.Value<string>())
    .ToList();

// Find elements by ID
var userProfile = root.GetElementById("user-profile");
```

### 7.2 Document Creation and Modification

```csharp
// Create a new document using fluent API
var doc = XferBuilder.Object(
    ("name", "Alice Johnson"),
    ("age", XferBuilder.Integer(30)),
    ("email", "alice@example.com"),
    ("preferences", XferBuilder.Object(
        ("theme", "dark"),
        ("notifications", true)
    )),
    ("scores", XferBuilder.Array(85, 92, 78))
);

// Add processing instructions
doc.AddProcessingInstruction(XferBuilder.IdPI("user-profile"));

// Modify existing data
doc["preferences"]["theme"] = "light";
doc["scores"].AsArray().Add(XferBuilder.Integer(95));

// Serialize back to XferLang
string result = doc.Serialize(Formatting.Pretty);
```

### 7.3 Complex Queries

```csharp
// Find all numeric values in hexadecimal format
var hexValues = root
    .Descendants()
    .OfType<XInteger>()
    .Where(x => x.Format == XferNumericFormat.Hexadecimal)
    .Select(x => new { Value = x.Value, HexString = x.ToString() })
    .ToList();

// Get unresolved dynamic elements
var unresolvedDynamic = root
    .Descendants()
    .OfType<XDynamic>()
    .Where(x => !x.IsResolved)
    .Select(x => x.Key)
    .ToList();

// Complex path-based query (JSONPath-like)
var nestedValues = root.SelectTokens("$.users[*].preferences.notifications")
    .Values<bool>()
    .ToList();
```

### 7.4 Type-Safe Operations

```csharp
// Work with strongly-typed arrays
var scores = root["scores"].AsArray();
if (scores.ElementType == typeof(IntegerElement))
{
    var intScores = scores.Values<int>().ToList();
    var average = intScores.Average();
}

// Homogeneous array validation
var users = root["users"].AsArray();
if (users.IsHomogeneous)
{
    foreach (var user in users.Children().OfType<XObject>())
    {
        ProcessUser(user);
    }
}
```

## 8. Performance Considerations

### 8.1 Lazy Evaluation
- Wrapper objects are created on-demand
- Enumeration operations use lazy evaluation where possible
- Large document traversal is optimized with streaming

### 8.2 Memory Management
- Lightweight wrapper objects that delegate to underlying Elements
- Reuse of Element objects when possible
- Efficient collection enumeration without unnecessary allocations

### 8.3 Caching Strategies
- Cache commonly accessed wrapper objects
- Cache type information for performance-critical paths
- Optimize ID lookups with internal indexing

## 9. Error Handling

### 9.1 Type Safety
- Runtime type checking with meaningful error messages
- Graceful handling of type mismatches
- Support for nullable and optional value access

### 9.2 Query Safety
- Null-safe navigation operators
- Graceful handling of missing properties/elements
- Clear error messages for invalid paths or operations

## 10. Implementation Phases

### Phase 1: Core Infrastructure
1. Implement base XValue class and type hierarchy
2. Create wrapper classes for all Element types
3. Implement basic value access and navigation

### Phase 2: Collection Support
1. Implement XObject, XArray, XTuple classes
2. Add collection-specific operations
3. Implement enumeration support

### Phase 3: LINQ Integration
1. Add standard LINQ extension methods
2. Implement XferLang-specific query operations
3. Add path-based query support

### Phase 4: Creation and Modification
1. Implement fluent creation API
2. Add modification operations
3. Integrate with existing serialization

### Phase 5: Advanced Features
1. Add processing instruction support
2. Implement ID-based querying
3. Add performance optimizations and caching

## 11. Testing Strategy

### 11.1 Unit Tests
- Comprehensive coverage of all wrapper classes
- Test all LINQ operations and edge cases
- Validate type safety and error handling

### 11.2 Integration Tests
- Test with real XferLang documents
- Validate round-trip serialization
- Performance benchmarks against direct Element access

### 11.3 Documentation Tests
- All examples in documentation must be working code
- Interactive documentation with live examples

## 12. Future Enhancements

### 12.1 Advanced Query Features
- XPath-like query syntax
- Query compilation and optimization
- Custom query operators

### 12.2 Schema Integration
- Schema-aware querying
- Validation during modification
- Type inference from schema

### 12.3 Async Support
- Async enumeration for large documents
- Streaming query operations
- Async modification operations

---

This design provides a comprehensive, type-safe, and performant LINQ API for XferLang that leverages .NET developers' existing knowledge while supporting XferLang's unique features.
