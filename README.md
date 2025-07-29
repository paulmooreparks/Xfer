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
  - [1. Introduction \& Philosophy](#1-introduction--philosophy)
  - [2. XferLang by Example](#2-xferlang-by-example)
  - [3. Language Specification](#3-language-specification)
    - [3.1. Document Structure](#31-document-structure)
    - [3.2. Element Syntax Variations](#32-element-syntax-variations)
      - [3.2.1. Implicit Syntax](#321-implicit-syntax)
      - [3.2.2. Compact Syntax](#322-compact-syntax)
      - [3.2.3. Explicit Syntax](#323-explicit-syntax)
    - [3.3. Element Reference](#33-element-reference)
      - [3.3.1. Primitive Types](#331-primitive-types)
      - [3.3.1.1. Hexadecimal and Binary Formatting](#3311-hexadecimal-and-binary-formatting)
      - [3.3.2. Structural Types](#332-structural-types)
      - [3.3.3. Special-Purpose Types](#333-special-purpose-types)
  - [4. The `.NET XferLang Library`](#4-the-net-xferlang-library)
    - [4.1. Basic Serialization \& Deserialization](#41-basic-serialization--deserialization)
    - [4.2. Advanced Usage with `XferSerializerSettings`](#42-advanced-usage-with-xferserializersettings)
      - [4.2.1. Null Value Handling](#421-null-value-handling)
      - [4.2.2. Customizing Property Names with `IContractResolver`](#422-customizing-property-names-with-icontractresolver)
      - [4.2.3. Custom Type Converters with `IXferConverter`](#423-custom-type-converters-with-ixferconverter)
      - [4.2.4. Numeric Formatting with Attributes](#424-numeric-formatting-with-attributes)
    - [4.3 DynamicSource PI Override: Powerful and Flexible](#43-dynamicsource-pi-override-powerful-and-flexible)
      - [How it works](#how-it-works)
      - [Example](#example)
      - [Why this matters](#why-this-matters)
  - [5. Project Status \& Roadmap](#5-project-status--roadmap)
  - [6. Contributing](#6-contributing)
  - [7. Grammar](#7-grammar)

## 1. Introduction & Philosophy

XferLang is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management. Its design philosophy is centered around four key principles:

*   **Clarity and Readability**: The syntax is designed to be human-readable, even without separators like commas. This is achieved by using whitespace to delimit elements in most cases.
*   **Explicit Typing**: All values are explicitly typed. This avoids the type ambiguity that can sometimes occur in formats like JSON, leading to more predictable parsing and less defensive coding.
*   **Elimination of Escaping**: XferLang does not require escaping of special characters in values. Instead, values are enclosed in unique delimiters. If the content contains a sequence that matches the delimiter, the delimiter character can be repeated as many times as necessary to make it unique, ensuring that data can be embedded without modification.
*   **Safety in Embedding**: The delimiter-repetition strategy allows for the safe embedding of complex, nested data structures without the risk of delimiter collision.

## 2. XferLang by Example

Perhaps the easiest way to understand XferLang is to see it compared to a familiar format like JSON.

Here's a simple JSON document:

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

And here is the equivalent XferLang document:

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

Because whitespace is flexible, the same document can be made extremely compact:

```xfer
{name"Alice"age 30 isMember~true scores[*85*90*78.5]profile{email"alice@example.com"joinedDate@2023-05-05T20:00:00@}}
```

## 3. Language Specification

### 3.1. Document Structure

An XferLang document consists of two main parts: an optional **Metadata Element** followed by a root **Tuple Element**.

*   **Metadata Element**: If present, this must be the very first non-comment element in the document. It is used to store information about the document itself, such as the XferLang version.
*   **Root Tuple**: The main content of the document is contained within an implicit root tuple. This means an XferLang document can contain a sequence of multiple, distinct elements at its top level.

```xfer
</ A document metadata element, which is signified with the reserved keyword `xfer` is optional, but if present it must be the first element in the document. />
<! xfer { version "1.0" } !>

</ The rest of the document is a sequence of elements in the root tuple. />
"Hello, World!"
42
{ key "value" }
```

### 3.2. Element Syntax Variations

XferLang elements have a flexible syntax with up to three variations. This allows you to choose the most readable and concise form for your data, only using more verbose syntax when necessary to resolve ambiguity.

#### 3.2.1. Implicit Syntax
For the most common types, like integers and simple keywords, no special characters are needed at all. The parser infers the type from the content.

```xfer
123                 </ An integer />
name "Alice"        </ A key/value pair of a keyword 'name' and a string value />
```

#### 3.2.2. Compact Syntax
For most other types, a single specifier character (or a pair for collections) denotes the type. This is the most common syntax.

```xfer
~true               </ A boolean />
*123.45             </ A decimal />
"Hello, World!"     </ A string />
[ 1 2 3 ]           </ An array of integers />
```

#### 3.2.3. Explicit Syntax
When an element's content might be ambiguous (e.g., a string containing a quote), you can wrap the compact form in angle brackets (`<` and `>`). This is the most verbose but also the most powerful form, as it allows for delimiter repetition to avoid any collision.

```xfer
<"Alice said, "Boo!"">
<// A comment containing </another comment/> //>
```

### 3.3. Element Reference

This section provides a detailed reference for each XferLang element type.

#### 3.3.1. Primitive Types

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

#### 3.3.1.1. Hexadecimal and Binary Formatting

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
// Decimal 42 in different formats
decimal: 42
hex: #$2A
binary: #%101010
padded_hex: #$002A    // MinDigits = 4
padded_binary: #%00101010  // MinBits = 8
```

**Safety Note:** Hex and binary formatting are only supported for integer types (`int`, `long`). Decimal and double types preserve fractional precision and do not support these formats.

#### 3.3.2. Structural Types

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

#### 3.3.3. Special-Purpose Types

**Metadata Element**
*   **Specifiers:** `!` (Exclamation Mark)
*   **Description:** A special object that can only appear at the start of a document. It contains metadata about the document, such as the `xfer` version.
*   **Example:** `<! xfer {version "1.0"} description "Sample document" !>`

**Comment Element**
*   **Specifier:** `/` (Slash)
*   **Description:** A comment that is ignored by the parser. It always requires explicit syntax.
*   **Example:** `</ This is a comment. />`, `<// Nested </comment/> //>`

**Dynamic Element**
*   **Specifier:** `|` (Pipe)
*   **Description:** Represents a value to be substituted at runtime, by default from an environment variable. You can override the default dynamic value resolution by subclassing `DefaultDynamicSourceResolver` or by implementing the `IDynamicSourceResolver` interface. This allows you to provide custom logic for resolving dynamic values in your XferLang documents.
*   **Example:** `'Hello, <|USERNAME|>!'`

## 4. The `.NET XferLang Library`

The primary implementation of XferLang is the `ParksComputing.Xfer.Lang` library for .NET. It provides a comprehensive object model, a robust parser, and a powerful serialization/deserialization utility class, `XferConvert`.

### 4.1. Basic Serialization & Deserialization

The `XferConvert` class provides a simple, static interface for converting between .NET objects and XferLang strings.

```csharp
public class MyData {
    public string Name { get; set; }
    public int Value { get; set; }
}

var data = new MyData { Name = "Example", Value = 123 };

// Serialize to an Xfer string
string xfer = XferConvert.Serialize(data, Formatting.Indented);

// {
//     Name "Example"
//     Value 123
// }

// Deserialize back to an object
var deserializedData = XferConvert.Deserialize<MyData>(xfer);
```

### 4.2. Advanced Usage with `XferSerializerSettings`

For more control, you can pass an instance of `XferSerializerSettings` to the `Serialize` and `Deserialize` methods.

#### 4.2.1. Null Value Handling
By default, properties with `null` values are included. You can set `NullValueHandling` to `Ignore` to omit them.

```csharp
var settings = new XferSerializerSettings {
    NullValueHandling = NullValueHandling.Ignore
};
```

#### 4.2.2. Customizing Property Names with `IContractResolver`
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

#### 4.2.3. Custom Type Converters with `IXferConverter`
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

#### 4.2.4. Numeric Formatting with Attributes

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

### 4.3 DynamicSource PI Override: Powerful and Flexible

XferLang allows you to override dynamic value resolution in your document using the `dynamicSource` processing instruction (PI). This feature provides significant flexibility for configuration, testing, and integration scenarios.

#### How it works
- The resolver uses its default logic (e.g., reverse, DB, etc.) unless a PI override is present.
- If a `dynamicSource` PI is present, you can specify per-key overrides:
    - `demo "reverse:!dlroW ,olleH"` (custom logic)
    - `password "env:MY_PASSWORD"` (environment variable)
    - `greeting "This is a hard-coded greeting."` (hard-coded value)
- The PI can be placed at the top of your Xfer document.
- If a key is not present in the PI, the resolver falls back to its default logic.

#### Example
```xfer
<! dynamicSource {
    demo "reverse:!dlroW ,olleH"
    greeting "env:GREETING_MSG"
    password "hardcoded-demo-password"
} !>
message {
    text '<|demo|>'
    greeting '<|greeting|>'
    password '<|password|>'
}
```

#### Why this matters
- Enables flexible, testable, and environment-specific configuration.
- Supports secrets, test data, and runtime overrides without code changes.

This feature makes XferLang highly extensible and adaptable for a wide range of use cases.

## 5. Project Status & Roadmap

The .NET implementation of XferLang is becoming more robust, with a focus on professional-grade features like custom converters and contract resolvers. However, the project as a whole is still experimental.

The future roadmap includes:
*   Reimplementing the core library in Rust.
*   Exposing a C ABI from the Rust implementation.
*   Creating language wrappers (e.g., for C#, Python, JavaScript) that communicate with the C ABI.

This will provide a single, high-performance, and memory-safe core for all future XferLang implementations. I'm looking for contributors and collaborators to get that work started.

## 6. Contributing

This is an open-source project, and contributions are always welcome! If you are interested in helping with the Rust implementation, creating language wrappers, improving the existing .NET library, or have other ideas, please feel free to open an issue or a pull request on GitHub. You can also reach out to me directly via email at [paul@parkscomputing.com](mailto:paul@parkscomputing.com).

## 7. Grammar

The formal Backus–Naur form (BNF) grammar for XferLang can be found in the repository: [xfer.bnf](https://github.com/paulmooreparks/Xfer/blob/master/xfer.bnf).
