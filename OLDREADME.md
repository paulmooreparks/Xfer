![XferLogo](https://raw.githubusercontent.com/paulmooreparks/Xfer/master/logo/XferLang-sm.png)

# The XferLang Data-Interchange Format

[![.NET Build Status](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/vpre/ParksComputing.Xfer.Lang.svg)](https://www.nuget.org/packages/ParksComputing.Xfer.Lang)
[![GitHub last commit](https://img.shields.io/github/last-commit/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer)
[![GitHub issues](https://img.shields.io/github/issues/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer/issues)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

_Welcome to everyone who came here from [Hacker News](https://news.ycombinator.com/item?id=42114543). Thank you so much for all the great input and discussion!_

## Table of Contents
- [The XferLang Data-Interchange Format](#the-xferlang-data-interchange-format)
  - [Table of Contents](#table-of-contents)
  - [Introduction and Philosophy](#introduction-and-philosophy)
  - [XferLang by Example](#xferlang-by-example)
  - [Language Specification](#language-specification)
    - [Document Structure](#document-structure)
    - [Element Syntax Variations](#element-syntax-variations)
      - [Implicit Syntax](#implicit-syntax)
      - [Compact Syntax](#compact-syntax)
      - [Explicit Syntax](#explicit-syntax)
    - [Element Reference](#element-reference)
      - [Primitive Types](#primitive-types)
      - [Hexadecimal and Binary Formatting](#hexadecimal-and-binary-formatting)
      - [Structural Types](#structural-types)
      - [Common Document Patterns](#common-document-patterns)
      - [Special-Purpose Types](#special-purpose-types)
    - [Document Validation and Common Mistakes](#document-validation-and-common-mistakes)
  - [The `.NET XferLang Library`](#the-net-xferlang-library)
    - [Basic Serialization \& Deserialization](#basic-serialization--deserialization)
    - [Advanced Usage with `XferSerializerSettings`](#advanced-usage-with-xferserializersettings)
      - [Null Value Handling](#null-value-handling)
      - [Customizing Property Names with `IContractResolver`](#customizing-property-names-with-icontractresolver)
      - [Custom Type Converters with `IXferConverter`](#custom-type-converters-with-ixferconverter)
      - [Numeric Formatting with Attributes](#numeric-formatting-with-attributes)
  - [Processing Instructions \& Dynamic Content](#processing-instructions--dynamic-content)
    - [Built-in Processing Instructions](#built-in-processing-instructions)
      - [Document Metadata PI](#document-metadata-pi)
      - [DynamicSource PI](#dynamicsource-pi)
      - [CharDef PI](#chardef-pi)
      - [ID PI](#id-pi)
    - [Dynamic Elements \& Source Resolution](#dynamic-elements--source-resolution)
      - [Built-in Source Types](#built-in-source-types)
      - [Custom Source Handler Registration](#custom-source-handler-registration)
    - [Extending Processing Instructions](#extending-processing-instructions)
      - [Creating Custom PIs](#creating-custom-pis)
      - [PI Registration \& Lifecycle](#pi-registration--lifecycle)
  - [Project Status \& Roadmap](#project-status--roadmap)
  - [Contributing](#contributing)
  - [Grammar](#grammar)

## Introduction and Philosophy

XferLang is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management. Its design philosophy is centered around four key principles:

*   **Clarity and Readability**: The syntax is designed to be human-readable, even without separators like commas. This is achieved by using whitespace to delimit elements in most cases.
*   **Explicit Typing**: All values are explicitly typed. This avoids the type ambiguity that can sometimes occur in formats like JSON, leading to more predictable parsing and less defensive coding.
*   **Elimination of Escaping**: XferLang does not require escaping of special characters in values. Instead, values are enclosed in unique delimiters. If the content contains a sequence that matches the delimiter, the delimiter character can be repeated as many times as necessary to make it unique, ensuring that data can be embedded without modification.
*   **Safety in Embedding**: The delimiter-repetition strategy allows for the safe embedding of complex, nested data structures without the risk of delimiter collision.
*   **Structured Root**: All XferLang documents require a root collection element (Object, Array, or Tuple) to ensure well-defined document structure and unambiguous parsing.

## XferLang by Example

Perhaps the easiest way to understand XferLang is to see it compared to a familiar format like JSON.

Here's a simple XferLang document:

```xfer
{
    name "Alice"
    age 30
    isMember ~true
    scores [*85 *90 *78.5]
    profile {
        email "alice@example.com"
        joinedDate @2023-01-15T12:00:00@
    }
}
```

And here is the equivalent JSON document:

```json
{
    "name": "Alice",
    "age": 30,
    "isMember": true,
    "scores": [85, 90, 78.5],
    "profile": {
        "email": "alice@example.com",
        "joinedDate": "2023-01-15T12:00:00"
    }
}
```

In contrast to JSON, XferLang eliminates commas, uses explicit type prefixes for certain types (`*` for decimal, `~` for Boolean, `?` for null) while maintaining readability.

Because whitespace is flexible, the same XferLang document can be made extremely compact:

```xfer
{name"Alice"age 30 isMember~true scores[*85*90*78.5]profile{email"alice@example.com"joinedDate@2023-05-05T20:00:00@}}
```

## Language Specification

### Document Structure

An XferLang document consists of two main parts: an optional **Metadata Element** followed by a **Root Collection Element**.

*   **Metadata Element**: If present, this must be the very first non-comment element in the document. It is used to store information about the document itself, such as the XferLang version.
*   **Root Collection Element**: The main content of the document must be contained within a single collection element (Object, Array, or Tuple). This ensures the document has a well-defined structure and prevents ambiguity in parsing.

```xfer
</ A document metadata element, which is signified with the reserved keyword `document` is optional, but if present it must be the first element in the document. />
<! document { version "1.0" } !>

</ The document content must be wrapped in a collection element />
{
    message "Hello, World!"
    count 42
    config { debug ~true }
}
```

**Valid Root Collection Types:**
- **Object**: `{ key1 value1 key2 value2 }` - Most common for structured data
- **Array**: `[ element1 element2 element3 ]` - For homogeneous collections
- **Tuple**: `( element1 element2 element3 )` - For heterogeneous sequences

### Element Syntax Variations

XferLang elements have a flexible syntax with up to three variations. This allows you to choose the most readable and concise form for your data, only using more verbose syntax when necessary to resolve ambiguity.

#### Implicit Syntax
For the most common types, like integers and simple keywords, no special characters are needed at all. The parser infers the type from the content.

```xfer
123                 </ An integer />
name "Alice"        </ A key/value pair of a keyword 'name' and a string value />
```

#### Compact Syntax
For most other types, a single specifier character (or a pair for collections) denotes the type. This is the most common syntax.

```xfer
~true               </ A boolean />
*123.45             </ A decimal />
"Hello, World!"     </ A string />
[ 1 2 3 ]           </ An array of integers />
```

#### Explicit Syntax
When an element's content might be ambiguous (e.g., a string containing a quote), you can wrap the compact form in angle brackets (`<` and `>`). This is the most verbose but also the most powerful form, as it allows for delimiter repetition to avoid any collision.

```xfer
<"Alice said, "Boo!"">
<// A comment containing </another comment/> //>
```

### Element Reference

This section provides a detailed reference for each XferLang element type.

#### Primitive Types

**String Element**
*   **Specifier:** `"` (Quotation Mark)
*   **Description:** Contains text data. The content is stored verbatim. To include a `"` character that would conflict with the closing delimiter, repeat the specifier (e.g., `""...""`) or use explicit syntax (`<"..."">`).
*   **Example:** `"Hello, World!"`, `""A quote is a " character.""`, `<"Alice said, "Boo!"">`

**Interpolated Text Element**
*   **Specifier:** `'` (Apostrophe)
*   **Description:** Similar to a string, but embedded elements are evaluated and their values are interpolated into the final text.
*   **Example:** `'The value is <#42#>'` renders as `"The value is 42"`.

**Character Element**
*   **Specifier:** `\` (Backslash)
*   **Description:** Represents a single character, specified by its Unicode codepoint in decimal, hex (`$`), or binary (`%`), or by a predefined keyword (e.g., `tab`, `lf`, `gt`).
*   **Example:** `\65`, `\$41`, `\gt` (all represent 'A')

**Integer Element**
*   **Specifier:** `#` (Number Sign)
*   **Description:** A 32-bit signed integer. Can be written in decimal, hex (`$`), or binary (`%`). The specifier is optional if the syntax is unambiguous (implicit syntax).
*   **Example:** `42`, `#$2A`, `#%00101010`

**Long Element**
*   **Specifier:** `&` (Ampersand)
*   **Description:** A 64-bit signed integer. Can be written in decimal, hex (`$`), or binary (`%`).
*   **Example:** `&5000000000`, `&$12A05F200`, `&%1001010100000010111110010000000000`

**Double Element**
*   **Specifier:** `^` (Caret)
*   **Description:** A 64-bit floating-point number.
*   **Example:** `^3.14159`

**Decimal Element**
*   **Specifier:** `*` (Asterisk)
*   **Description:** A 128-bit high-precision decimal value.
*   **Example:** `*123.45`

**Boolean Element**
*   **Specifier:** `~` (Tilde)
*   **Description:** Represents a `true` or `false` value.
*   **Example:** `~true`, `~false`

**Date/Time Element**
*   **Specifier:** `@` (At Sign)
*   **Description:** Represents a date and time value in ISO 8601 format.
*   **Example:** `@2025-07-23T10:00:00@`

**Null Element**
*   **Specifier:** `?` (Question Mark)
*   **Description:** Represents a null value.
*   **Example:** `?`, `<??>`

#### Hexadecimal and Binary Formatting

Integer and Long elements support alternative numeric representations for improved readability in specific contexts:

**Hexadecimal Format**
*   **Syntax:** `#$` prefix followed by hexadecimal digits (e.g., `#$2A`, `&$12A05F200`)
*   **Use Cases:** Memory addresses, color values, bitmasks, low-level programming
*   **Parsing:** Case-insensitive (`#$2A` equals `#$2a`)
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Hexadecimal, MinDigits = 4)]` for zero-padding

**Binary Format**
*   **Syntax:** `#%` prefix followed by binary digits (e.g., `#%101010`, `&%1001010100000010111110010000000000`)
*   **Use Cases:** Bit manipulation, flags, educational purposes, embedded systems
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Binary, MinBits = 8)]` for zero-padding

**Examples:**
```xfer
{
    // Decimal 42 in different formats
    decimal 42
    hex #$2A
    binary #%101010
    padded_hex #$002A    // MinDigits = 4
    padded_binary #%00101010  // MinBits = 8
}
```

**Safety Note:** Hex and binary formatting are only supported for integer types (`int`, `long`). Decimal and double types preserve fractional precision and do not support these formats.

#### Structural Types

**Object Element**
*   **Specifiers:** `{` and `}` (Curly Brackets)
*   **Description:** A collection of key/value pairs. Keys are typically implicit keywords, and values can be any XferLang element.
*   **Example:** `{ name "Alice" age 30 }`

**Array Element**
*   **Specifiers:** `[` and `]` (Square Brackets)
*   **Description:** A collection of elements of the **same type**.
*   **Example:** `[ 1 2 3 ]`, `[ "a" "b" "c" ]`

**Tuple Element**
*   **Specifiers:** `(` and `)` (Parentheses)
*   **Description:** A collection of elements of **any type**, similar to a JSON array.
*   **Example:** `( "Alice" 30 ~true )`

**Keyword Element and Key/Value Pairs**
*   **Description:** A keyword is the key in a key/value pair. If it contains only letters, numbers, and underscores, it can be written implicitly. Otherwise, it must be enclosed in colons (`:`).
*   **Example:** `name "Paul"`, `:first name: "Alice"`

#### Common Document Patterns

**Configuration Documents**
Most configuration files use Object as the root collection:
```xfer
{
    database {
        host "localhost"
        port 5432
        ssl ~true
    }
    logging {
        level "info"
        destinations [ "console" "file" ]
    }
}
```

**Data Collections**
For homogeneous data, use Array as the root:
```xfer
[
    { name "Alice" age 30 }
    { name "Bob" age 25 }
    { name "Charlie" age 35 }
]
```

**Mixed Content Documents**
For documents with heterogeneous top-level content, use Tuple:
```xfer
(
    "Document Title"
    @2023-12-25T10:00:00@
    {
        metadata { version "1.0" author "John Doe" }
        content {
            sections [ "intro" "body" "conclusion" ]
            wordCount 1500
        }
    }
)
```

#### Special-Purpose Types

**Metadata Element**
*   **Specifiers:** `!` (Exclamation Mark)
*   **Description:** A special object that can only appear at the start of a document. It contains metadata about the document, such as the `document` version.
*   **Example:** `<! document {version "1.0"} description "Sample document" !>`

**Comment Element**
*   **Specifier:** `/` (Slash)
*   **Description:** A comment that is ignored by the parser. It always requires explicit syntax.
*   **Example:** `</ This is a comment. />`, `<// Nested </comment/> //>`

**Dynamic Element**
*   **Specifier:** `|` (Pipe)
*   **Description:** Represents a value to be substituted at runtime, by default from an environment variable. You can override the default dynamic value resolution by subclassing `DefaultDynamicSourceResolver` or by implementing the `IDynamicSourceResolver` interface. This allows you to provide custom logic for resolving dynamic values in your XferLang documents.
*   **Example:** `'Hello, <|USERNAME|>!'`

### Document Validation and Common Mistakes

**Root Collection Requirement**
Every XferLang document must have exactly one root collection element after any metadata. This is a fundamental requirement for proper parsing:

```xfer
// ❌ Invalid - No root collection
"Hello, World!"
42

// ❌ Invalid - Multiple top-level elements
{ config "value" }
[ 1 2 3 ]

// ✅ Valid - Single root object
{
    message "Hello, World!"
    count 42
}

// ✅ Valid - Single root array
[ "Hello, World!" 42 ]

// ✅ Valid - Single root tuple
( "Hello, World!" 42 { config "value" } )
```

**With Metadata**
When using metadata, it must come first, followed by exactly one root collection:

```xfer
// ✅ Valid - Metadata followed by root object
<! document { version "1.0" } !>
{
    data "example"
}

// ❌ Invalid - Missing root collection after metadata
<! document { version "1.0" } !>
"standalone string"
```

## The `.NET XferLang Library`

The primary implementation of XferLang is the `ParksComputing.Xfer.Lang` library for .NET. It provides a comprehensive object model, a robust parser, and a powerful serialization/deserialization utility class, `XferConvert`.

### Basic Serialization & Deserialization

The `XferConvert` class provides a simple, static interface for converting between .NET objects and XferLang strings. The library automatically ensures that serialized objects are wrapped in a proper root collection element.

```csharp
public class MyData {
    public string Name { get; set; }
    public int Value { get; set; }
}

var data = new MyData { Name = "Example", Value = 123 };

// Serialize to an Xfer string (automatically creates root object)
string xfer = XferConvert.Serialize(data, Formatting.Indented);

// {
//     Name "Example"
//     Value 123
// }

// Deserialize back to an object
var deserializedData = XferConvert.Deserialize<MyData>(xfer);
```

### Advanced Usage with `XferSerializerSettings`

For more control, you can pass an instance of `XferSerializerSettings` to the `Serialize` and `Deserialize` methods.

#### Null Value Handling
By default, properties with `null` values are included. You can set `NullValueHandling` to `Ignore` to omit them.

```csharp
var settings = new XferSerializerSettings {
    NullValueHandling = NullValueHandling.Ignore
};
```

#### Customizing Property Names with `IContractResolver`
You can change how property names are serialized by creating a custom contract resolver. For example, to make all property names lowercase:

```csharp
public class LowerCaseContractResolver : DefaultContractResolver {
    public override string ResolvePropertyName(string propertyName) {
        return propertyName.ToLower();
    }
}

var settings = new XferSerializerSettings {
    ContractResolver = new LowerCaseContractResolver()
};
```

#### Custom Type Converters with `IXferConverter`
For complete control over how a specific type is handled, you can create a custom converter. This is useful for types that don't map well to standard object serialization or for creating a more compact representation.

**Example: A custom converter for a `Person` class**

```csharp
// The class to convert
public class Person {
    public string Name { get; set; }
    public int Age { get; set; }
}

// The custom converter
public class PersonConverter : XferConverter<Person> {
    // Convert a Person object to a compact string element
    public override Element WriteXfer(Person value, XferSerializerSettings settings) {
        return new StringElement($"{value.Name},{value.Age}");
    }

    // Convert a string element back to a Person object
    public override Person ReadXfer(Element element, XferSerializerSettings settings) {
        if (element is StringElement stringElement) {
            var parts = stringElement.Value.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[1], out int age)) {
                return new Person { Name = parts[0], Age = age };
            }
        }
        throw new InvalidOperationException("Cannot convert element to Person.");
    }
}

// How to use it
var settings = new XferSerializerSettings();
settings.Converters.Add(new PersonConverter());

var person = new Person { Name = "John Doe", Age = 42 };
string xfer = XferConvert.Serialize(person, settings); // Result: "John Doe,42"
```

#### Numeric Formatting with Attributes

The library supports custom numeric formatting for integer and long properties using the `XferNumericFormatAttribute`. This allows you to control how numeric values are serialized in hexadecimal or binary formats.

**Available Formats:**
- `XferNumericFormat.Decimal` - Standard decimal representation (default)
- `XferNumericFormat.Hexadecimal` - Hexadecimal with `#$` prefix
- `XferNumericFormat.Binary` - Binary with `#%` prefix

**Padding Options:**
- `MinDigits` - For hexadecimal, pads with leading zeros to minimum digit count
- `MinBits` - For binary, pads with leading zeros to minimum bit count

**Example:**

```csharp
public class ConfigurationData {
    [XferNumericFormat(XferNumericFormat.Decimal)]
    public int Port { get; set; } = 8080;

    [XferNumericFormat(XferNumericFormat.Hexadecimal)]
    public int ColorValue { get; set; } = 0xFF5733;

    [XferNumericFormat(XferNumericFormat.Binary, MinBits = 8)]
    public int Flags { get; set; } = 42;

    [XferNumericFormat(XferNumericFormat.Hexadecimal, MinDigits = 8)]
    public long MemoryAddress { get; set; } = 0x7FF6C2E40000;
}

var config = new ConfigurationData();
string xfer = XferConvert.Serialize(config);
// Result: {Port 8080 ColorValue #$FF5733 Flags #%00101010 MemoryAddress &$7FF6C2E40000}
```

**Safety Notes:**
- Numeric formatting attributes are only applied to `int` and `long` properties
- `decimal` and `double` types ignore formatting attributes to preserve fractional precision
- Custom formatting respects the configured `ElementStylePreference` for syntax style

## Processing Instructions & Dynamic Content

XferLang supports Processing Instructions (PIs) that provide metadata and configuration for documents. PIs are special elements that control parsing behavior, define document metadata, and enable powerful dynamic content features. The PI system is fully extensible, allowing you to create custom instructions for specialized use cases.

### Built-in Processing Instructions

XferLang includes several built-in PIs that address common document needs:

#### Document Metadata PI

The `document` PI stores metadata about the XferLang document itself and must appear first if present:

```xfer
<! document {
    version "1.0"
    author "John Doe"
    created @2023-12-01T10:30:00@
    description "Sample configuration file"
} !>
{
    // Document content follows...
}
```

**Key features:**
- Must be the first non-comment element if present
- Provides version tracking and document attribution
- Supports any metadata fields as key-value pairs
- Accessible programmatically through `XferDocument.ProcessingInstructions`

#### DynamicSource PI

The `dynamicSource` PI configures how dynamic elements `<|key|>` are resolved, providing flexible runtime value substitution:

```xfer
<! dynamicSource {
    greeting const "Welcome to our application"
    username env "USERNAME"
    config file "app.config"
    secret vault "api-key"
} !>
{
    message '<|greeting|>'
    user '<|username|>'
    settings '<|config|>'
    apiKey '<|secret|>'
}
```

#### CharDef PI

The `chardef` PI allows you to define custom character aliases for use in character elements:

```xfer
<! chardef {
    bullet \$2022
    arrow \$2192
    check \$2713
} !>
{
    symbols [ \bullet \arrow \check ]
}
```

#### ID PI

The `id` PI assigns identifiers to elements for referencing and linking:

```xfer
{
    <! id "user-config" !>
    section {
        name "User Settings"
        enabled ~true
    }
}
```

### Dynamic Elements & Source Resolution

Dynamic elements provide runtime value substitution with an extensible source system.

#### Built-in Source Types

XferLang includes three built-in source handlers:

**Constant Sources (`const`)**
- Returns the configured value as a literal constant
- Useful for templating and configuration management
- Example: `greeting const "Hello, World!"`

**Environment Variables (`env`)**
- Reads from system environment variables
- Supports fallback values and variable name mapping
- Example: `username env "USER"` (reads from $USER environment variable)

**File Sources (`file`)**
- Reads content from files at parse time
- Supports relative and absolute paths
- Example: `config file "settings.json"`

#### Custom Source Handler Registration

Extend the system with custom source types for databases, web services, or other data sources:

```csharp
// Register a custom 'vault' source handler
DynamicSourceHandlerRegistry.RegisterHandler("vault", (sourceValue, fallbackKey) => {
    var key = sourceValue ?? fallbackKey;
    return await SecretVaultClient.GetSecretAsync(key);
});

// Register a database source handler
DynamicSourceHandlerRegistry.RegisterHandler("db", (sourceValue, fallbackKey) => {
    var query = sourceValue ?? $"SELECT value FROM config WHERE key = '{fallbackKey}'";
    return DatabaseService.ExecuteScalar(query);
});
```

**Resolution Priority:**
1. Configured source type in `dynamicSource` PI
2. Fallback to environment variables
3. Return empty string if not found

### Extending Processing Instructions

The PI system is designed for extensibility, allowing you to create custom instructions for specialized parsing and document processing needs.

#### Creating Custom PIs

To create a custom PI, inherit from `ProcessingInstruction` and implement the required behavior:

```csharp
public class ValidationProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "validation";

    public ValidationProcessingInstruction(ObjectElement rules) : base(rules, Keyword) { }

    public override void ProcessingInstructionHandler() {
        if (Value is not ObjectElement obj) {
            throw new InvalidOperationException($"{Keyword} PI expects an object element");
        }

        // Process validation rules and store globally
        foreach (var kv in obj.Dictionary) {
            var fieldName = kv.Value.Key;
            var rules = kv.Value.Value;
            ValidationRegistry.RegisterRules(fieldName, rules);
        }
    }

    public override void ElementHandler(Element element) {
        // Apply validation rules to specific elements
        ValidationRegistry.ValidateElement(element);
    }
}
```

#### PI Registration & Lifecycle

Register custom PIs with the parser during initialization:

```csharp
// Register the custom PI processor
Parser.RegisterPIProcessor(ValidationProcessingInstruction.Keyword,
    (kvp, parser) => new ValidationProcessingInstruction((ObjectElement)kvp.Value));

// Example usage in XferLang document
var xferContent = @"
<! validation {
    userEmail regex ""^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$""
    userAge range { min 0 max 120 }
} !>
{
    userEmail ""user@example.com""
    userAge 25
}";
```

**PI Lifecycle:**
1. **Discovery**: PIs are identified during metadata parsing
2. **Creation**: Registered processors create PI instances
3. **Processing**: `ProcessingInstructionHandler()` is called for document-level setup
4. **Element Processing**: `ElementHandler()` is called for each relevant element
5. **Cleanup**: PIs can register cleanup logic if needed

**Advanced Features:**
- **Scoped PIs**: Apply to specific document sections
- **Cascading PIs**: Inherit behavior from parent elements
- **Conditional PIs**: Activate based on document content or external conditions
- **Multi-phase PIs**: Process elements in multiple passes

This extensible PI system makes XferLang highly adaptable for domain-specific needs, from configuration management to data validation and transformation pipelines.

## Project Status & Roadmap

The .NET implementation of XferLang is becoming more robust, with a focus on professional-grade features like custom converters and contract resolvers. However, the project as a whole is still experimental.

The future roadmap includes:
*   Completing the .NET implementation to achieve a production-quality 1.0 release
*   Reimplementing the core library in Rust
*   Exposing a C ABI from the Rust implementation
*   Creating language wrappers (e.g., for C#, Python, JavaScript) that communicate with the C ABI

The goal of moving to a Rust core is to provide a single, high-performance, and memory-safe core for all future XferLang implementations. I'm looking for contributors and collaborators to get that work started.

## Contributing

This is an open-source project, and contributions are always welcome! If you are interested in helping, please feel free to open an issue or a pull request on GitHub. You can also reach out to me directly via email at [paul@parkscomputing.com](mailto:paul@parkscomputing.com).

## Grammar

The formal Backus–Naur form (BNF) grammar for XferLang can be found in the repository: [xfer.bnf](https://github.com/paulmooreparks/Xfer/blob/master/xfer.bnf).
