![XferLogo](https://raw.githubusercontent.com/paulmooreparks/Xfer/master/logo/XferLang-sm.png)

# XferLang

[![GitHub last commit](https://img.shields.io/github/last-commit/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer)
[![.NET Build Status](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/vpre/ParksComputing.Xfer.Lang.svg)](https://www.nuget.org/packages/ParksComputing.Xfer.Lang)
[![GitHub issues](https://img.shields.io/github/issues/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer/issues)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---
## Introduction

XferLang is a typed, delimiter‑driven data format for data transfer, configuration, and structured content. If you know JSON, you'll feel at home using XferLang, but then you'll begin to notice what's different: explicit types, interpolated strings, no escape sequences, bound data references, and extensible processing instructions.

---
## Getting Started

### Why XferLang

- **XferLang is readable without ceremony.** Whitespace separates elements, and you may expand or collapse formatting without changing meaning.
- **Types are explicit.** There are no heuristics or guesswork.
- **There is no escape tax.** When content would collide with a delimiter, you may lengthen the delimiter instead of inserting backslashes.
- **Parsing is programmable.** Processing instructions declare metadata, bind names to elements for dynamic insertion, control parsing output, and compose elements from external sources. You may even develop your own custom processing instructions or extend the built-in ones.

### Quick XferLang Example

```xfer
<! document { version "1" } !>
{
    title "Demo"
    active ~true
    retries 3
    ratio *0.8125
    launched @2025-08-01T09:30:00Z@
    tags [ "alpha" "preview" ]
    location ( *42.3601 *-71.0589 )
    banner 'User=<|USER|> ok=<~true~>'
}
```

### Visual Studio Code Extension

Install the XferLang extension from the Visual Studio Marketplace to enable syntax highlighting and basic diagnostics.

Marketplace: https://marketplace.visualstudio.com/items?itemName=paulmooreparks.xferlang

Open a file with a .xfer file extension to activate the XferLang extension.

---
## Project Status and Roadmap

The .NET implementation of XferLang is becoming more robust, with a focus on professional-grade features like custom converters and contract resolvers. However, the project as a whole is still experimental.

The future roadmap includes:
*   Completing the .NET implementation to achieve a production-quality 1.0 release
*   Reimplementing the core library in Rust
*   Exposing a C ABI from the Rust implementation
*   Creating language wrappers (e.g., for C#, Python, JavaScript) that communicate with the C ABI

The goal of moving to a Rust core is to provide a single, high-performance, and memory-safe core for all future XferLang implementations. I'm looking for [contributors and collaborators](#contributing) to get that work started.

---
## XferLang Basics

### Elements and Collections

```xfer
{
    name "Alice"
    age 30
    isMember ~true
    scores [ *85 *90 *78.5 ]
    profile {
        email "alice@example.com"
        joinedDate @2023-01-15T12:00:00@
    }
    point (*42.3601 *-71.0589)
    optional ?
}
```

Objects (`{}`) hold key/value pairs. Arrays (`[]`) are ordered and homogeneous (each item must be the same element type). Tuples (`()`) are ordered and heterogeneous (may contain multiple element types).

### Specifiers and the "No Escapes" Rule

Instead of using escape sequences, you may lengthen the opening and closing specifier runs in order to enclose problematic content. The parser treats the contiguous run as the delimiter.

- Strings use `"…"`, and a longer run allows embedded quotes: `""He said, \"Hello\".""`.
- Interpolated text uses `'…'`, and a longer run allows embedded apostrophes: `''Outer 'inner' still fine''`.
- Comments use `</ … />`, and a longer run allows nested markers: `<// contains </ safely //>`.
- Use an explicit form by wrapping with `<…>` when you need to isolate internals.

```xfer
{
    nestedQuotes ""He said, "Hello" then left.""
    dynamicWithPipe ||status|ok||
    <// Outer comments containing </ inner comments /> are fine //>
    explicitDate <@2025-08-01T12:00:00@>
}
```

Pick the shortest run that avoids ambiguity.

### Processing Instructions

Processing instructions (PIs) are single key/value directives that the parser consumes before continuing to parse. They use the form `<! name <value> !>`. The built‑in PIs include:

- `document` – document metadata
- `dynamicSource` – map names to source handlers (file/env/const)
- `let` – bind a name to a value
- `script` – batch multiple operators and apply them before the next element
- `if` – conditionally suppress the next element
- `chardef` – define character aliases
- `id` - assign a unique identifier to the following element
- `tag` – assign a categorization tag to the following element

```xfer
<! document { version "1.2" env "prod" } !>
<! dynamicSource { apiKey file "secrets/api-key.txt" user env "USER" tag const "2025.08.11" } !>
<! if defined |apiKey| !> { auth { key |apiKey| user |user| tag |tag| } }
```

### Interpolated Text

Interpolated‑text elements evaluate embedded elements to render a final text element. Like string elements, these will deserialize to a string type.

```xfer
<! dynamicSource { username env "USER" } !>
</ Value will render as 'User=paul LoggedIn=True Since 1 Aug 2025 09:30:00' />
banner 'User=<|username|> LoggedIn=<~true~> Since <@2025-08-01T09:30:00@>'
```

---
## Introduction to the .NET Library

Install the NuGet package [ParksComputing.Xfer.Lang](https://www.nuget.org/packages/ParksComputing.Xfer.Lang) to parse and serialize XferLang in .NET projects.

### Parse and Serialize

```csharp
using ParksComputing.Xfer.Lang.Services;

var parser = new Parser();
var doc = parser.Parse("{ name \"Alice\" age 30 }");

// Work with the document
var roundTrip = doc.ToXfer(); // Serialize back to XferLang
```

Warnings include row/column anchors to help locate issues:

```csharp
foreach (var w in doc.Warnings) {
        Console.WriteLine($"{w.Type} @ {w.Row}:{w.Column} — {w.Message} [{w.Context}]");
}
```

### Object Mapping (XferConvert)

`XferConvert` turns CLR objects into Xfer elements and back. This is useful for configuration scenarios and typed round‑trips.

```csharp
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;

var settings = new XferSerializerSettings();

var person = new Person { Name = "Ada", Age = 36 };
var element = XferConvert.FromObject(person, settings);  // ObjectElement
var xfer = element.ToXfer();

var back = XferConvert.ToObject<Person>((ObjectElement)element, settings);
```

Attributes influence names and number formatting:

```csharp
using ParksComputing.Xfer.Lang.Attributes;

public class Person {
    [XferProperty("fullName")] public string Name { get; set; } = string.Empty;
    [XferNumericFormat(XferNumericFormat.Hex)] public int Favorite { get; set; }
}
```

### Configuration (XferSerializerSettings)

`XferSerializerSettings` controls naming, numeric formatting, decimal precision, and extension points. Highlights:

- ContractResolver (default: `DefaultContractResolver`)
- Converters (`IXferConverter`) — optional custom type converters
- Decimal and double precision (`XferDecimalPrecisionAttribute`)
- Integer/long formatting (`XferNumericFormatAttribute`)

```csharp
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.ContractResolvers;
using ParksComputing.Xfer.Lang.Converters;

var settings = new XferSerializerSettings {
    ContractResolver = new DefaultContractResolver()
};

// Optional: add custom converters when you need specialized handling
settings.Converters.Add(new MySpecialConverter());
```

Note: Advanced serializer extension points (custom converters and custom contract resolvers) are supported and evolving. Keep tests nearby when extending.

---

### Binding References with `let`

You may bind names to element values with the `let` processing instruction, and you may reference them in your document as reference elements.

```xfer
<! let greeting "world" !>
message 'Hello <_greeting_>' </ <_greeting_> is replaced with "world" at parse time />
tuple ( _greeting _greeting ) </ Renders as tuple ("world" "world") />
```

If a reference cannot be resolved at parse time, the reference element is rendered as-is and the parser reports a warning.

### Grouped Let Bindings with `script`

The `script` PI groups multiple `let` operators together.

```xfer
<! script (
    let x "Hello"
    let y 'X=<_x_>'
) !>
(
    _x    </ Renders as "Hello" />
    _y    </ Renders as "X=Hello" />
)
```

In the future, additional keywords will be available for scripting as well.

### Dynamic Elements and Sources

Dynamic elements (`|name|`) resolve from sources configured by `dynamicSource` or, absent a mapping, from environment variables. The built‑in source types are `env` to read environment variables, `file` to read files, and `const` for constant values.

```xfer
<! dynamicSource {
    apiKey file "secrets/api-key.txt"
    user   env  "USER"
    build  const "2025.08.11"
} !>
{ key '|apiKey|' user '|user|' tag '|build|' }
```

### Conditional Element Inclusion with `if`

The `if` PI evaluates a condition. If the condition is false, the following sibling element is not added to the document. The `if` processing instruction itself is always stripped from output regardless of evaluation outcome.

```xfer
<! let showDebug ~false !>
<! if _showDebug !>
{ debug { level "verbose" } }
```

Defined vs undefined (existence test) may also be used:

```xfer
<! if defined _showDebug !> { note 'Evaluates as true even if the value is ~false.' }
```

Behavior & serialization rules:

* If the condition evaluates to false, the target element (the immediately following sibling element) is completely suppressed; that is, it is never added to the parsed document model.
* If the condition evaluates to true, the target element is retained.
* Regardless of outcome (true, false, evaluation error, or unknown operator name) the `if` processing instruction itself is always stripped from serialization output. It acts only as a directive at parse time.
* An unknown operator name inside the condition currently acts as a no‑op (treated as truthy so the element is preserved) but the PI is still stripped. Future versions may surface a warning; do not rely on serialization visibility for diagnostics.
* Direct reference conditions (`<! if _flag !>`) test the bound value's truthiness; use `defined` to distinguish between an undefined binding and a defined but falsy value.

Example showing outcomes (serialized form shown on right):

```xfer
<! let enabled ~true !>  <! if _enabled !> { feature { status "on" } }   </ serializes as: { feature { status "on" } } />
<! let enabled ~false !> <! if _enabled !> { feature { status "on" } }   </ serializes as: (nothing emitted) />
<! if someUnknownOp["a" "b"] !> { kept ~true }                         </ unknown op -> element kept; PI stripped />
```

### Character Definitions

Define symbolic character aliases for readability (keyword → Unicode code point):

```xfer
<! chardef { bullet \$2022 arrow \$2192 } !>
{ list ("Item" \bullet "Next" \arrow ) }
```

---
## Core Concepts

### Elements and Syntax Forms
Every value is an element. Most elements support three styles:
- Implicit: minimal form (e.g., `42`) when unambiguous.
- Compact: type delimiter(s) (e.g., `#42`, `"text"`).
- Explicit: wrapped in angle brackets (e.g., `<#42#>`, `<"A quote: "">`) allowing delimiter repetition.

### Collections
- Object: `{ key value ... }` (unordered key/value pairs; keys are keywords)
- Array: `[ value value ... ]` (homogeneous)
- Tuple: `( value value ... )` (heterogeneous)

### Keywords vs Identifiers
- Keywords (object keys) are implicit barewords composed only of letters, digits, and underscore: `[A-Za-z_][A-Za-z0-9_]*`.
- If a key needs any other character (dash, space, etc.) wrap it with leading & trailing `=` specifier runs: `=first-name=` (lengthen the run if it appears within the key itself).
- Identifiers (`:name:`) are value elements (never keys). They always use leading and trailing `:`; they cannot contain whitespace or punctuation other than underscore.

### Numbers
Integers (`#` or implicit), longs (`&`), decimals (`*` high precision), doubles (`^`). Alternate bases for integers/longs: hex `$`, binary `%`.

### Text and Characters
Strings use quotation marks, for example `"..."`. To include the delimiter itself, repeat it or switch to the explicit form `<"...">`. Characters may be written using decimal (`\\65`), hexadecimal (`\\$41`), binary (`\\%01000001`), or predefined character keywords such as `\\tab`.

### Date and Time
Date and time values use the `@...@` form with ISO‑8601 formats. Where implemented, date‑only and time‑only forms may also be parsed.

### Null
The `?` element represents a null value.

### Binding and Reference
`<! let name <value> !>` binds `name` to `<value>`. Inside subsequent elements the value may be referenced with `_name` (or inside interpolation as `<_name_>`).

Batching bindings: The `script` processing instruction currently supports only the `let` operator. (Additional operators will be added prior to general release.) You can group several sequential `let` bindings inside a single tuple so they all execute before the next element parses:

```xfer
<! script ( let first "Alice" let greeting 'Hi <_first_>' let answer 42 ) !>
{ message _greeting number _answer }
```

All listed `let` bindings are evaluated in order; later bindings can reference earlier ones (as with `_first` inside the interpolated greeting) but self‑reference is prevented. Because the script PI here contains only `let` bindings it does not serialize into the output (it is suppressed after execution).

### Interpolated Text
Interpolated text is delimited by apostrophes, for example `'Hello <_name_>'`. Embedded elements inside must use explicit forms. The expressions are structural replacements, so no character escaping is required.

### Dynamic Elements
`|identifier|` resolves through the configured dynamic source resolver (e.g., environment). Content is a single identifier; nested elements are not allowed inside the delimiters in the current implementation.

### Processing Instructions (PIs)
Processing instructions have a compact form `! name <value> !` and an explicit form `<! name <value> !>`. Each PI consists of exactly one key/value pair, where the value may be any element. Some PIs introduce bindings or affect subsequent parsing and may be suppressed from serialization after execution.

---
## XferLang Language Specification

### Document Structure

The document order is as follows:
1. Zero or more processing instructions.
2. Exactly one root collection element (Object, Array, or Tuple).

Comments (`</ ... />`) may appear anywhere and are ignored by the parser.

### Element Syntax Variations

XferLang elements have a flexible syntax with up to three variations. This allows you to choose the most readable and concise form for your data, only using more verbose syntax when necessary to resolve ambiguity.

#### Implicit Syntax
For integers and keywords, no special characters are needed when the context is unambiguous.

```xfer
123                 </ An integer />
name "Alice"        </ A key/value pair with implicit keyword 'name' and a string value />
```

#### Compact Syntax
Most elements use either a single specifier character or an enclosing pair of specifiers to denote the type. This is the most common syntax. Keywords containing whitespace or other special characters require the `=` specifier, while identifiers require the `:` specifier.

```xfer
(
    ~true                   </ A Boolean value />
    *123.45                 </ A decimal value />
    "Hello, World!"         </ A string />
    [ #1 #2 #3 ]            </ An array of integers />
    =special keyword= #42    </ A keyword with an embedded space />
)
```

Enclosing specifier characters may be repeated as many times as necessary to enable an element to contain that same specifier character.

```xfer
( ""This string contains a " character with impunity."" )
```

#### Explicit Syntax
When an element's content might be ambiguous (e.g., a string containing a quote), you may wrap the compact form in angle brackets (`<` and `>`). This is the most verbose but also the most powerful form, as it affords the most flexibility for containing special characters.

```xfer
<(
    <"Alice said, "Boo!"">
    <// A comment containing </another comment/> //>
    <=object description=> <"This tuple is inside <()> delimiters.">
)>
```

---
## XferLang Element Reference

XferLang supports a rich set of data types designed for clarity, explicitness, and flexibility. Each type is chosen to represent a distinct kind of value, making data both human-readable and machine-precise.

### Primitive Types

#### String Element
A string element contains text data. The content is stored verbatim. To include a `"` character that would conflict with the closing delimiter, repeat the specifier (e.g., `""...""`) or use explicit syntax (`<"..."">`).

*   **Specifier:** `"` (Quotation Mark)
*   **Syntax:**
    *   **Compact:** `"Hello, World!"`
    *   **Explicit:** `<"Alice said, "Boo!"">`

*   **Examples:**
    ```xfer
    </ Compact syntax />
    message "Hello, World!"

    </ Explicit syntax with quotes />
    quote <"Alice said, "Boo!"">

    </ Compact syntax with delimiter repetition />
    description ""A quote is a " character.""
    ```

#### Character Element
A character element represents a single UTF-8 character, specified by its codepoint in decimal, hex (`$`), or binary (`%`), or by a predefined keyword (e.g., `tab`, `lf`, `gt`).

*   **Specifier:** `\` (Backslash)
*   **Syntax:**
    *   **Compact:** `\65`, `\$41`, `\%01000001`, `\gt`
    *   **Explicit:** `<\65\>`, `<\$41\>`, `<\gt\>`
*   **Examples:**
    ```xfer
    </ Compact syntax - decimal codepoint />
    letterA \65

    </ Compact syntax - hex codepoint />
    letterB \$42

    </ Compact syntax - binary codepoint />
    letterC \%01000011

    </ Compact syntax - keyword />
    tabChar \tab
    newlineChar \lf

    </ Explicit syntax />
    specialChar <\$2665\>  </ Heart symbol />
    ```

#### Integer Element
An integer element represents a 32-bit signed integer. The value may be written in decimal, hex (`$`), or binary (`%`). The specifier is optional if the value is unambiguous (implicit syntax).

*   **Specifier:** `#` (Number Sign)
*   **Syntax:**
    *   **Implicit:** `42`, `-123`
    *   **Compact:** `#42`, `#$2A`, `#%-123`, `#%00101010`
    *   **Explicit:** `<#42#>`, `<#$2A#>`, `<#%00101010#>`
*   **Range:** -2,147,483,648 to 2,147,483,647
*   **Examples:**
    ```xfer
    </ Implicit syntax (most common) />
    age 30
    count 1000
    negative -42

    </ Compact syntax />
    port #8080
    timeout #30
    colorRed #$FF
    permissions #%11110000

    </ Explicit syntax />
    maxValue <#2147483647#>
    hexValue <#$DEADBEEF#>
    binaryFlags <#%11110000#>
    ```

#### Long Element
A long element represents a 64-bit signed integer. The value may be written in decimal, hex (`$`), or binary (`%`).

*   **Specifier:** `&` (Ampersand)
*   **Syntax:**
    *   **Compact:** `&5000000000`, `&$12A05F200`, `&%1001010100000010111110010000000000`
    *   **Explicit:** `<&5000000000&>`, `<&$12A05F200&>`, `<&%1001010100000010111110010000000000&>`
*   **Range:** -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
*   **Examples:**
    ```xfer
    </ Compact syntax />
    population &7800000000
    fileSize &5368709120
    timestamp &1672531200000

    </ Compact syntax - hexadecimal />
    userId &$1A2B3C4D5E6F
    memoryOffset &$7FF6C2E40000

    </ Compact syntax - binary />
    featureFlags &%1111000011110000111100001111

    </ Explicit syntax />
    maxLong <&9223372036854775807&>
    hexAddress <&$7FFFFFFFFFFFFFFF&>
    binaryMask <&%1111111111111111111111111111111111111111111111111111111111111111&>
    ```

#### Double Element
A double element represents a 64-bit floating-point number.

*   **Specifier:** `^` (Caret)
*   **Syntax:**
    *   **Compact:** `^3.14159`, `^-2.5`
    *   **Explicit:** `<^3.14159^>`, `<^-2.5^>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    pi ^3.14159
    temperature ^-2.5
    radius ^12.75

    </ Explicit syntax />
    preciseValue <^3.141592653589793^>
    measurement <^123.456789^>
    ratio <^0.618033988749^>
    ```

#### Decimal Element
A decimal element represents a high-precision decimal value.

*   **Specifier:** `*` (Asterisk)
*   **Syntax:**
    *   **Compact:** `*123.45`, `*-456.789`, `*0.000001`
    *   **Explicit:** `<*123.45*>`, `<*-456.789*>`, `<*0.000001*>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    price *123.45
    balance *-456.789
    precision *0.000001

    </ Explicit syntax />
    currency <*1234567.89*>
    percentage <*99.999*>
    calculation <*0.123456789012345*>
    ```

#### Boolean Element
A Boolean element represents a `true` or `false` value.

*   **Specifier:** `~` (Tilde)
   **Description:**
*   **Syntax:**
    *   **Compact:** `~true`, `~false`
    *   **Explicit:** `<~true~>`, `<~false~>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    isActive ~true
    isDeleted ~false
    hasPermission ~true

    </ Explicit syntax />
    confirmed <~true~>
    disabled <~false~>
    verified <~true~>
    ```

#### Date/Time Element
A date/time element represents a date and time value in ISO 8601 format.

*   **Specifier:** `@` (At Sign)
*   **Syntax:**
    *   **Compact:** `@2025-07-23T10:00:00@`, `@2023-12-25@`, `@2023-01-01T00:00:00Z@`
    *   **Explicit:** `<@2025-07-23T10:00:00@>`, `<@2023-12-25@>`, `<@2023-01-01T00:00:00Z@>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    created @2023-12-01T10:30:00@
    birthDate @1990-05-15@
    lastLogin @2023-12-25T09:30:00Z@

    </ Explicit syntax />
    timestamp <@2025-07-23T10:00:00@>
    scheduledDate <@2024-01-01T00:00:00Z@>
    eventTime <@2023-12-31T23:59:59.999@>
    ```

#### Null Element
A null element represents a null value.

*   **Specifier:** `?` (Question Mark)
*   **Syntax:**
    *   **Compact:** `?`
    *   **Explicit:** `<??>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    optionalValue ?
    middleName ?
    description ?

    </ Explicit syntax />
    nullField <??>
    missingInfo <??>
    ```

### Hexadecimal and Binary Formatting

Integer and Long elements support alternative numeric representations for improved readability in specific contexts:

**Hexadecimal Format**
*   **Syntax:** `$` prefix followed by hexadecimal digits (e.g., `#$2A`, `&$12A05F200`)
*   **Use Cases:** Memory addresses, color values, bitmasks, low-level programming
*   **Parsing:** Case-insensitive (`#$2A` equals `#$2a`)
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Hexadecimal, MinDigits = 4)]` for zero-padding

**Binary Format**
*   **Syntax:** `%` prefix followed by binary digits (e.g., `#%101010`, `&%1001010100000010111110010000000000`)
*   **Use Cases:** Bit manipulation, feature flags
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Binary, MinBits = 8)]` for zero-padding

**Examples:**
```xfer
{
    </ Decimal integer 42 in different formats />
    decimal 42
    hex #$2A
    binary #%101010
    padded_hex #$002A
    padded_binary #%00101010
}
```

Hexadecimal and binary formatting are only supported for character, integer, and long element types. Decimal and double types preserve fractional precision and do not support these formats.

### Collection Elements

#### Object Element
An object is a collection of key/value pairs. Keys are keyword elements, and values may be any XferLang element.

*   **Specifiers:** `{` and `}` (Curly Brackets)
*   **Syntax:**
    *   **Compact:** `{ name "Alice" age 30 }`
    *   **Explicit:** `<{ name "Alice" age 30 }>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    user { name "Alice" age 30 active ~true }
    config { host "localhost" port 8080 ssl ~true }

    </ Explicit syntax />
    metadata <{ version "1.0" author "John Doe" }>
    settings <{ theme "dark" notifications ~true }>
    ```

#### Array Element
An array is a collection of elements of the same type (e.g., all integers, all strings, all objects).

*   **Specifiers:** `[` and `]` (Square Brackets)
*   **Syntax:**
    *   **Compact:** `[ 1 2 3 ]`, `[ "a" "b" "c" ]`
    *   **Explicit:** `<[ 1 2 3 ]>`, `<[ "a" "b" "c" ]>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    numbers [ 1 2 3 4 5 ]
    names [ "Alice" "Bob" "Charlie" ]
    booleans [ ~true ~false ~true ]

    </ Explicit syntax />
    ports <[ #80 #443 #8080 ]>
    colors <[ "red" "green" "blue" ]>
    flags <[ ~true ~true ~false ]>
    ```

#### Tuple Element
A tuple is an ordered collection of elements that may be of **any type**, similar to arrays in JSON. Tuples are ideal for containing heterogeneous data like coordinates, records, or mixed-type sequences.

*   **Specifiers:** `(` and `)` (Parentheses)
*   **Syntax:**
    *   **Compact:** `( "Alice" 30 ~true )`
    *   **Explicit:** `<( "Alice" 30 ~true )>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    userRecord ( "John Doe" 30 ~true )
    coordinates ( *42.3601 *-71.0589 )
    mixedData ( "Sample" @2023-12-25@ *99.5 )

    </ Explicit syntax />
    complexTuple <( "Alice" 30 ~true [ "admin" "user" ] )>
    dataPoint <( "Experiment A" @2023-12-01T10:00:00@ *98.7 )>
    ```

### Special-Purpose Types

#### Keyword Element and Key/Value Pairs
The key/value pair is the fundamental building block of XferLang objects. The key must be a keyword element, and the value may be any XferLang element. Key-value pairs form the basis of structured data in objects.

*   **Specifier:** `=` (Equal Sign)
*   **Syntax:**
    *   **Implicit:** `name`, `user_id`, `isActive` (letters, numbers, underscores only - valid only as keys)
    *   **Compact:** `=first-name=`, `=email-address=`, `=API-Key=` (keywords as keys with `=` specifier)
    *   **Explicit:** `<=first name=>`, `<=email address=>`, `<=API Key=>` (keywords as keys with explicit syntax)
*   **Examples:**
```xfer
{
    </ Implicit syntax - only valid as keys />
    name "Paul"
    age 30
    user_id 12345
    isActive ~true

    </ Compact syntax for keywords as keys />
    =first-name= "Alice"
    =last-name= "Johnson"
    =email-address= "user@example.com"

    </ Explicit syntax for keywords as keys />
    <=first name=> "Alice"
    <=email address=> "user@example.com"
    <=API Key=> "secret123"
    <=content type=> "application/json"
}
```

Key/value pairs may recurse; that is, the value in the key value pair may itself be a key/value pair.

```xfer
{
    key1 key2 "key1's value is a key/value pair"
}
```

#### Processing Instruction Element
Contains processing instructions for the document, such as the `document` PI which stores document metadata.

*   **Specifier:** `!` (Exclamation Mark)
*   **Syntax:**
    *   **Compact:** `! document { version "1.0" } !`
    *   **Explicit:** `<! document { version "1.0" } !>`
*   **Examples:**
```xfer
</ Compact syntax />
! document { version "1.0" author "John Doe" } !
! id "user-config" !
! chardef { bullet \$2022 arrow \$2192 } !

</ Explicit syntax />
<! document { version "1.0" description "Sample document" } !>
<! dynamicSource { username env "USER" } !>
```

#### Comment Element
Comments provide documentation and annotations within XferLang documents. They are ignored during parsing and may be used for explanations, notes, or temporarily disabling content.

*   **Specifier:** `/` (Slash)
*   **Syntax:**
    *   **Explicit:** `</ comment />`, `<// nested </comment/> via delimiter repetition //>`
*   **Examples:**
```xfer
</ Basic comments />
</ This is a simple comment />
</ Multi-line comment
    spanning several lines />

</ Delimiter repetition for nested content />
<// This comment contains / characters and nested </comments/> //>
</// Multi-level nesting for complex content ///>
```

#### Dynamic Element
Represents a value resolved via the dynamic source pipeline (dynamicSource PI mapping → custom handler → environment fallback).

*   **Specifier:** `|` (Pipe)
*   **Syntax:**
    *   **Compact:** `|USERNAME|`, `|DB_PASSWORD|`
    *   **Explicit:** `<|USERNAME|>`
*   **Examples:**
```xfer
</ Compact syntax />
username |USER|
password |DB_PASSWORD|

</ Within interpolated strings />
message 'Hello <|USER|>!'

**Configuration via Processing Instructions:**
```xfer
<! dynamicSource {
    greeting const "Welcome to XferLang" </ Constant string/>
    username env "USER" </ Environment variable />
    config file "settings.xfer" </ File />
} !>
{
    message '<|greeting|>' </ Constant string replaces <|greeting|>. />
    user '<|username|>' </ USER environment-variable value replaces <|username|>. />
    settings '<|config|>' </ File contents replace <|config|>. />
}
```

#### Interpolated String Element
Interpolated elements render the contents of embedded elements into a string. Otherwise, they are deserialized to a string type just like the string element. Embedded elements must use explicit syntax in order to be evaluated and rendered.

*   **Specifier:** `'` (Apostrophe)
*   **Syntax:**
    *   **Compact:** `'The value is <#42#>'`, `'Hello, <|NAME|>!'`
    *   **Explicit:** `<'The value is <#42#>'>`, `<'Hello, <|NAME|>!'>`
*   **Examples:**
```xfer
</ Compact syntax />
message 'The value is <#42#>'
greeting 'Hello, <|USERNAME|>!'
template 'User <"Alice"> has <#5#> items'

</ Explicit syntax />
complexMessage <'The result is <*99.5*> and status is <~true~>'>
dynamicContent <'Welcome to <|APP_NAME|> version <"1.0">'>
```

It is a good practice to use explicit syntax (`<' ... '>`) when you are unsure whether interpolated values may contain single quotes.

#### Reference Element
When the keyword inside a reference element has been bound to an element using the `let` processing instruction or a `let` operator inside a `script` processing instruction, the parser replaces the entire reference element with a clone of the bound element.

* **Specifier:** `_` (Underscore)
* **Syntax:**
    *   **Compact:** `_bindingName`
    *   **Explicit:** `<_bindingName_>`
* **Examples:**
```xfer
<! let host "localhost" !>
{ apiHost _host message 'Host: <_host_>' }
```

### Common Document Patterns

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
    =cache-timeout= 3600
    =max-connections= 100
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

---
## Complete Examples

Here are comprehensive examples showing XferLang in real-world scenarios:

### Configuration File Example

```xfer
<! document {
    version "1.2"
    author "DevOps Team"
    created @2023-12-01T10:30:00@
    description "Production API configuration"
} !>

{
    server {
        host "api.example.com"
        port 8443
        ssl ~true
        timeout 30
    }

    database {
        primary {
            host "db1.example.com"
            port 5432
            name "production_db"
            ssl ~true
            poolSize 20
        }
        replica {
            host "db2.example.com"
            port 5432
            readOnly ~true
        }
    }

    cache {
        redis {
            nodes [
                { host "cache1.example.com" port 6379 }
                { host "cache2.example.com" port 6379 }
                { host "cache3.example.com" port 6379 }
            ]
            ttl 3600
        }
    }

    logging {
        level "info"
        destinations [ "console" "file" "syslog" ]
    }

    features {
        rateLimiting ~true
        metrics ~true
        debugging ~false
    }
}
```

### User Profile Example

```xfer
{
    user {
        id &12345678901234
        username "alice_smith"
        email "alice@example.com"

        profile {
            firstName "Alice"
            lastName "Smith"
            birthDate @1990-05-15@
            bio 'Software engineer who loves <\$2615\> and <\$1F4BB\>' </ coffee and laptop emojis />
        }

        preferences {
            theme "dark"
            language "en-US"
            timezone "America/New_York"
            notifications {
                email ~true
                push ~false
                sms ~true
            }
        }

        activity {
            lastLogin @2023-12-25T09:30:00Z@
            loginCount #247
            sessions [
                {
                    id "sess_abc123"
                    startTime @2023-12-25T09:30:00Z@
                    ipAddress "192.168.1.100"
                    userAgent "Mozilla/5.0..."
                }
            ]
        }
    }

    addresses [
        {
            type "home"
            street "123 Main Street"
            city "Springfield"
            state "IL"
            zipCode "62702"
            country "US"
            isPrimary ~true
        }
        {
            type "work"
            street "456 Business Ave"
            city "Springfield"
            state "IL"
            zipCode "62702"
            country "US"
            isPrimary ~false
        }
    ]

    lastLogin @2023-12-25T09:30:00Z@
    loginCount #247
}
```

---
## The .NET XferLang Library

The primary implementation of XferLang is the `ParksComputing.Xfer.Lang` library for .NET. It provides a comprehensive object model, a robust parser, and a powerful serialization/deserialization utility class, `XferConvert`.

### Getting Started with .NET

The XferLang .NET library provides a robust implementation for parsing, generating, and working with XferLang documents programmatically.

#### Installation

Install the [NuGet package](https://www.nuget.org/packages/ParksComputing.Xfer.Lang) in your .NET project:

```bash
dotnet add package ParksComputing.Xfer.Lang
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package ParksComputing.Xfer.Lang
```

### Basic Serialization and Deserialization

```csharp
using ParksComputing.Xfer.Lang;

// Define a simple class
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
}

// Serialize to XferLang
var person = new Person { Name = "Alice", Age = 30, IsActive = true };
string xfer = XferConvert.Serialize(person, Formatting.Indented);

Console.WriteLine(xfer);
// Output:
// {
//     Name "Alice"
//     Age 30
//     IsActive ~true
// }

// Deserialize from XferLang
var deserializedPerson = XferConvert.Deserialize<Person>(xfer);
Console.WriteLine($"Name: {deserializedPerson.Name}, Age: {deserializedPerson.Age}");
```

### Working with Collections

```csharp
// Arrays and Lists
var numbers = new List<int> { 1, 2, 3, 4, 5 };
string xferArray = XferConvert.Serialize(numbers);
// Result: [ 1 2 3 4 5 ]

// Objects and Dictionaries
var config = new Dictionary<string, object>
{
    ["host"] = "localhost",
    ["port"] = 8080,
    ["ssl"] = true
};
string xferObject = XferConvert.Serialize(config);
// Result: { host "localhost" port 8080 ssl ~true }
```

### Advanced Usage with `XferSerializerSettings`

For more control, you may pass an instance of `XferSerializerSettings` to the `Serialize` and `Deserialize` methods.

#### Null Value Handling
Properties with `null` values are included. You may set `NullValueHandling` to `Ignore` to omit them.

```csharp
var settings = new XferSerializerSettings {
    NullValueHandling = NullValueHandling.Ignore
};
```

#### Customizing Property Names with `IContractResolver`
You may change how property names are serialized by creating a custom contract resolver. For example, to make all property names lowercase:

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
For complete control over how a specific type is handled, you may create a custom converter. This is useful for types that don't map well to standard object serialization or for creating a more compact representation.

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

---
## Serializer Settings

For more control over serialization and deserialization, you may use the `XferSerializerSettings` class. This allows you to configure element styles, null handling, contract resolvers, and custom converters.

### Element Style Preferences

Control how elements are serialized using the `StylePreference` property:

- **Explicit** - Maximum safety, uses angle brackets: `<"value">`
- **CompactWhenSafe** - Compact when safe, explicit when necessary: `"value"` (default)
- **MinimalWhenSafe** - Most compact form, including implicit syntax for integers
- **ForceCompact** - Always compact (use with caution)

### Configuration Example

```csharp
var settings = new XferSerializerSettings
{
    StylePreference = ElementStylePreference.CompactWhenSafe,
    PreferImplicitSyntax = true,  // Use implicit syntax for integers when safe
    NullValueHandling = NullValueHandling.Ignore,
    ContractResolver = new CustomContractResolver(),
    // ... other settings
};

string xferString = XferConvert.Serialize(user, Formatting.None, settings);
```

### Available Settings

- `StylePreference` - Controls element serialization style
- `PreferImplicitSyntax` - Use implicit syntax for integers when possible
- `NullValueHandling` - How to handle null values (Include/Ignore)
- `ContractResolver` - Custom property name resolution
- `Converters` - Collection of custom type converters

---
## Property Attributes

The library provides several attributes to control how properties are serialized and deserialized.

### XferPropertyAttribute

The `XferPropertyAttribute` allows you to customize the property name used in the Xfer document, similar to `JsonPropertyName` in System.Text.Json.

```csharp
public class User
{
    [XferProperty("user_name")]
    public string UserName { get; set; }

    [XferProperty("is_active")]
    public bool IsActive { get; set; }

    [XferProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}

var user = new User
{
    UserName = "alice",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

string xfer = XferConvert.Serialize(user);
// Result: { user_name "alice" is_active ~true created_at @2023-12-25T10:30:00Z@ }
```

### XferNumericFormatAttribute

The `XferNumericFormatAttribute` enables custom formatting for integer and long properties, allowing hexadecimal and binary representations with optional padding.

```csharp
public class ConfigurationData
{
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

### XferDecimalPrecisionAttribute

The `XferDecimalPrecisionAttribute` controls the precision and formatting of decimal and double values during serialization, allowing you to specify the maximum number of decimal places and whether to remove trailing zeros.

```csharp
public class FinancialData
{
    [XferDecimalPrecision(2)]
    public decimal Price { get; set; } = 123.456789m;

    [XferDecimalPrecision(4, RemoveTrailingZeros = false)]
    public decimal Interest { get; set; } = 5.25m;

    [XferDecimalPrecision(1)]
    public double Temperature { get; set; } = 98.76543;

    [XferDecimalPrecision(0)]
    public decimal Quantity { get; set; } = 150.999m;

    // Without attribute - uses default precision
    public decimal Cost { get; set; } = 99.99999m;
}

var data = new FinancialData();
string xfer = XferConvert.Serialize(data);
// Result: {Price *123.46 Interest *5.2500 Temperature ^98.8 Quantity *151 Cost *99.99999}
```

**Key Features:**
- **Precision Control**: Specify maximum decimal places (0 or greater)
- **Trailing Zero Handling**: Choose whether to remove trailing zeros (default: true)
- **Type Support**: Works with both `decimal` and `double` properties
- **Formatting Preservation**: Maintains the appropriate XferLang type specifier (`*` for decimal, `^` for double)

**Use Cases:**
- Financial applications requiring specific decimal precision
- Scientific data with controlled significant figures
- Display formatting for user interfaces
- Data export with consistent decimal representation

### Safety Notes

- Numeric formatting attributes (`XferNumericFormatAttribute`) are only applied to `int` and `long` properties
- Decimal precision attributes (`XferDecimalPrecisionAttribute`) are only applied to `decimal` and `double` properties
- `decimal` and `double` types ignore numeric formatting attributes to preserve fractional precision
- Custom formatting respects the configured `ElementStylePreference` for syntax style

### Processing Instructions and Dynamic Content

XferLang supports Processing Instructions (PIs) that provide metadata and configuration for documents. PIs are special elements that control parsing behavior, define document metadata, and enable powerful dynamic content features. The processing-instruction system is fully extensible, allowing you to create custom instructions for specialized use cases.

### Built-in Processing Instructions

XferLang includes several built-in PIs that address common document needs:

#### Document Metadata - `document`

The `document` processing instruction stores metadata about the XferLang document itself and must appear first if present:

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

#### Dynamic-Element Data Sources - `dynamicSource`

The `dynamicSource` processing instruction configures how dynamic elements `<|key|>` are resolved, providing flexible runtime value substitution:

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

#### Custom Character Definitions - `chardef`

The `chardef` processing instruction allows you to define custom character aliases for use in character elements:

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

#### Element ID - `id`

The `id` processing instruction assigns identifiers to elements for referencing and linking:

```xfer
{
    <! id "user-config" !>
    section {
        name "User Settings"
        enabled ~true
    }
}
```

#### Element Tagging - `tag`

The `tag` processing instruction attaches free‑form classification metadata to the immediately following element. Multiple `tag` processing instructions may be stacked; tags are preserved in element metadata but have no built‑in semantic effect.

```xfer
<! tag "experimental" !>
<! tag "search-index" !>
feature { enabled ~true }
```

#### Conditional Element Parsing and Output - `if defined`

The `if defined` processing instruction evaluates whether its value element yields a meaningful value (non‑null / non‑empty). When the value is defined, the associated content is parsed and output; otherwise, it is skipped.

```xfer
<! let debug ~false !>
<! if defined _debug !>
{ note 'debug binding exists' }

<! dynamicSource { optFlag env "OPTIONAL_FLAG" } !>
<! if defined <|optFlag|> !>
{ note 'OPTIONAL_FLAG present' }
```

Evaluation occurs during processing-instruction processing; the processing instruction itself is suppressed from serialization.

### Dynamic Elements and Source Resolution

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

You may extend the dynamic-source handler with custom source types for databases, web services, or other data sources:

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

#### PI Registration and Lifecycle

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

**Processing-Instruction Lifecycle:**
1. **Discovery**: PIs are identified during metadata parsing
2. **Creation**: Registered processors create PI instances
3. **Processing**: `ProcessingInstructionHandler()` is called for document-level setup
4. **Element Processing**: `ElementHandler()` is called for each relevant element
5. **Cleanup**: PIs may register cleanup logic if needed

**Advanced Features:**
- **Scoped PIs**: Apply to specific document sections
- **Cascading PIs**: Inherit behavior from parent elements
- **Conditional PIs**: Activate based on document content or external conditions
- **Multi-phase PIs**: Process elements in multiple passes

This extensible PI system makes XferLang highly adaptable for domain-specific needs, from configuration management to data validation and transformation pipelines.

### Writing Custom Processing Instructions

You may add new Processing Instruction (PI) keywords without modifying the core parser by registering a factory that creates a `ProcessingInstruction` subclass. This lets you introduce custom metadata, validation passes, conditional logic, or side‑effects at parse time.

### When to Create a PI
Create a PI when you need to:
* Run setup logic before the next element is parsed (`ProcessingInstructionHandler`).
* Optionally suppress or transform the immediately following element (`ElementHandler`).
* Carry operational intent that should not appear in serialized output (set `SuppressSerialization = true`).

Do NOT create a PI just to change how `|dynamic|` values resolve—use a custom `IDynamicSourceResolver` for that (see "Dynamic Elements and Source Resolution").

### Parser Registration Model
The parser exposes two overloads of `RegisterPIProcessor`:
```csharp
// (1) Low-level: attach raw callbacks for existing PIs
void RegisterPIProcessor(string key, Action<KeyValuePairElement> processor);

// (2) Factory: create a strongly-typed ProcessingInstruction instance
void RegisterPIProcessor(string key, Parser.PIProcessor factory); // factory: (kvp, parser) => ProcessingInstruction
```
Built‑ins are installed via the factory form inside `RegisterBuiltInPIProcessors()`.

### Minimal Custom PI Example (`trace`)
```csharp
public sealed class TraceProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "trace";
    public TraceProcessingInstruction(Element value) : base(value, Keyword) {
        SuppressSerialization = true; // operational only
    }
    public override void ProcessingInstructionHandler() {
        Console.WriteLine($"[TRACE-PI] {Value}");
    }
}

static ProcessingInstruction CreateTracePI(KeyValuePairElement kvp, Parser p) =>
    new TraceProcessingInstruction(kvp.Value);

var parser = new Parser();
parser.RegisterPIProcessor(TraceProcessingInstruction.Keyword, CreateTracePI);
```
Usage:
```xfer
<! trace "Starting build" !>
{ pipeline { stage "compile" } }
```

### Conditioning the Next Element
To influence the element that immediately follows the PI, override `ElementHandler`. Pattern (see `IfProcessingInstruction` in source):
```csharp
public override void ElementHandler(Element element) {
    if (!ShouldKeep(element)) {
        // Throw the same suppression exception style used by built-ins if you want to remove it
        throw new ConditionalElementException("Suppressed by custom PI");
    }
}
```

### Validation PI Skeleton
```csharp
public sealed class ValidationProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "validation";
    public ValidationProcessingInstruction(ObjectElement value) : base(value, Keyword) {
        SuppressSerialization = true;
    }
    public override void ProcessingInstructionHandler() {
        if (Value is not ObjectElement obj) throw new InvalidOperationException("validation PI expects object value");
        foreach (var kv in obj.Dictionary.Values) {
            var field = kv.Key;
            var ruleSpec = kv.Value;
            ValidationRegistry.Register(field, ruleSpec);
        }
    }
    public override void ElementHandler(Element element) => ValidationRegistry.Validate(element);
}

static ProcessingInstruction CreateValidationPI(KeyValuePairElement kvp, Parser p) =>
    new ValidationProcessingInstruction((ObjectElement)kvp.Value);

parser.RegisterPIProcessor(ValidationProcessingInstruction.Keyword, CreateValidationPI);
```

### Extending Dynamic Resolution (No New PI Required)
If you just need custom resolution for `|key|`, implement a resolver:
```csharp
public class MyDynamicSourceResolver : DefaultDynamicSourceResolver {
    public override string? Resolve(string key) => key == "special" ? "custom-value" : base.Resolve(key);
}
var settings = new XferSerializerSettings { DynamicSourceResolver = new MyDynamicSourceResolver() };
```

### Checklist for a New PI
1. Define a constant `Keyword`.
2. Subclass `ProcessingInstruction`.
3. (Optional) Set `SuppressSerialization` in constructor.
4. Override `ProcessingInstructionHandler` for one-time setup.
5. Override `ElementHandler` if you must inspect or suppress the following element.
6. Register a factory with `parser.RegisterPIProcessor(Keyword, Factory)`.
7. Add tests covering: creation, handler execution order, suppression (if any), serialization visibility.

### Lifecycle Summary
1. Parser encounters `<! keyword value !>`.
2. Registered factory creates PI instance.
3. Parser calls `ProcessingInstructionHandler()` immediately.
4. When the next element is parsed, `ElementHandler()` runs (if overridden).
5. PI may be omitted from serialization if `SuppressSerialization` is true.

This model keeps the core grammar stable while enabling domain‑specific behaviors.

---
## Building XferLang

Information for developers who want to build XferLang from source or contribute to the project.

### Prerequisites

- **.NET SDK 8.0 or later** - [Download from Microsoft](https://dotnet.microsoft.com/download)
- **Git** - For cloning the repository

### Quick Start

```bash
# Clone the repository
git clone https://github.com/paulmooreparks/Xfer.git
cd Xfer

# Build the entire solution
dotnet build

# Run the tests
dotnet test

# Create NuGet packages (optional)
dotnet pack --configuration Release
```

### Project Structure

The XferLang solution contains several projects and folders:

- **ParksComputing.Xfer.Lang** - Main library
- **ParksComputing.Xfer.Lang.Tests** - Unit tests and integration tests
- **XferService** - Example web service implementation (optional)
- **XferDocBuilder** - Custom tool for generating documentation
- **examples** - Command-line examples demonstrating various uses of XferLang
- **tools** - Development tools and utilities

### Development Commands

```bash
# Build in Debug mode
dotnet build --configuration Debug

# Build in Release mode
dotnet build --configuration Release

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---
## Community and Resources

Join the XferLang community and access helpful resources for learning and development.

### Learning Resources

- 📖 **Documentation:** [This comprehensive guide](https://xferlang.org/)
- 🎯 **Examples:** [Sample XferLang applications](https://github.com/paulmooreparks/Xfer/tree/master/examples) in the repository
- 💡 **Tests:** [Unit Tests](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer.Lang.Tests) also show how to use the library and provide test coverage
- 📄 **Sample Documents:** [*.xfer files](https://github.com/paulmooreparks/Xfer/tree/master) in the repository

### Getting Help

- ❓ **Questions:** Open a [GitHub Discussion](https://github.com/paulmooreparks/Xfer/discussions)
- 🐛 **Bug Reports:** Create an [Issue](https://github.com/paulmooreparks/Xfer/issues) with details
- 💡 **Feature Requests:** Suggest improvements via [GitHub Issues](https://github.com/paulmooreparks/Xfer/issues)
- 📧 **Direct Contact:** Reach out via GitHub for complex questions

---
## Contributing

This is an open-source project, and contributions are always welcome! If you are interested in helping, please feel free to open an issue or a pull request on [GitHub](https://github.com/paulmooreparks/Xfer). You may also reach out to me directly via email at [paul@parkscomputing.com](mailto:paul@parkscomputing.com).

---
## Grammar

The formal Backus–Naur form (BNF) grammar for XferLang can be found in the XferLang GitHub repository: [xfer.bnf](https://github.com/paulmooreparks/Xfer/blob/master/xfer.bnf).

