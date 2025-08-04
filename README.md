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
  - [XferLang Elements](#xferlang-elements)
    - [Array](#array)
    - [Key/Value Pair](#keyvalue-pair)
    - [Object](#object)
    - [Tuple](#tuple)
    - [Comment](#comment)
    - [Dynamic](#dynamic)
    - [Identifier](#identifier)
    - [Integer](#integer)
    - [Long](#long)
  - [Complete Examples](#complete-examples)
    - [Configuration File Example](#configuration-file-example)
    - [User Profile Example](#user-profile-example)
  - [Grammar Reference](#grammar-reference)
  - [Getting Started with .NET](#getting-started-with-net)
    - [Installation](#installation)
    - [Quick Start Example](#quick-start-example)
    - [Working with Collections](#working-with-collections)
  - [The `.NET XferLang Library`](#the-net-xferlang-library)
    - [Basic Serialization and Deserialization](#basic-serialization-and-deserialization)
    - [Advanced Usage with `XferSerializerSettings`](#advanced-usage-with-xferserializersettings)
      - [Null Value Handling](#null-value-handling)
      - [Customizing Property Names with `IContractResolver`](#customizing-property-names-with-icontractresolver)
      - [Custom Type Converters with `IXferConverter`](#custom-type-converters-with-ixferconverter)
      - [Numeric Formatting with Attributes](#numeric-formatting-with-attributes)
  - [Serializer Settings](#serializer-settings)
    - [Element Style Preferences](#element-style-preferences)
    - [Configuration Example](#configuration-example)
    - [Available Settings](#available-settings)
  - [Property Attributes](#property-attributes)
    - [XferPropertyAttribute](#xferpropertyattribute)
    - [XferNumericFormatAttribute](#xfernumericformatattribute)
    - [Safety Notes](#safety-notes)
  - [Processing Instructions and Dynamic Content](#processing-instructions-and-dynamic-content)
    - [Built-in Processing Instructions](#built-in-processing-instructions)
      - [Document Metadata PI](#document-metadata-pi)
      - [DynamicSource PI](#dynamicsource-pi)
      - [CharDef PI](#chardef-pi)
      - [ID PI](#id-pi)
    - [Dynamic Elements and Source Resolution](#dynamic-elements-and-source-resolution)
      - [Built-in Source Types](#built-in-source-types)
      - [Custom Source Handler Registration](#custom-source-handler-registration)
    - [Extending Processing Instructions](#extending-processing-instructions)
      - [Creating Custom PIs](#creating-custom-pis)
      - [PI Registration and Lifecycle](#pi-registration-and-lifecycle)
  - [Writing Custom Processing Instruction (PI) Processors](#writing-custom-processing-instruction-pi-processors)
    - [Conceptual Overview](#conceptual-overview)
    - [.NET API Usage](#net-api-usage)
  - [Building XferLang](#building-xferlang)
    - [Prerequisites](#prerequisites)
    - [Quick Start](#quick-start)
    - [Project Structure](#project-structure)
    - [Development Commands](#development-commands)
    - [Using the Command-Line Tools](#using-the-command-line-tools)
  - [Community and Resources](#community-and-resources)
    - [Learning Resources](#learning-resources)
    - [Getting Help](#getting-help)
    - [Development Status](#development-status)
    - [Language Comparison](#language-comparison)
  - [Project Status and Roadmap](#project-status-and-roadmap)
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

An XferLang document consists of two main parts: optional **Processing Instructions** followed by a **Root Collection Element**.

*   **Metadata Processing Instruction (`document`)**: If present, the `document` processing instruction must be the very first non-comment element in the document. It defines the document's metadata, such as the XferLang version.
*   **Additional Processing Instructions**: Other processing instructions besides the `document` processing instruction may be placed before the root element.
*   **Root Collection Element**: The main content of the document must be contained within a single collection element (Object, Array, or Tuple). This ensures the document has a well-defined structure and prevents ambiguity in parsing.

Comments (`</ ... />`) may be placed anywhere in the document.

```xfer
</ A document processing instruction, which is signified with the reserved keyword `document`
is optional, but if present it must be the first non-comment element in the document. />
<! document { version "1.0" } !>

</ Other processing instructions may be placed into the document before the root node. />
<! id "root" !>

</ The document root node must be a collection element. />
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
For integers and keywords, no special characters are needed when the context is unambiguous. Keywords appearing as keys in key/value pairs must use the `=` specifier followed by a trailing specifier.

```xfer
123                 </ An integer />
name "Alice"        </ A key/value pair with implicit keyword 'name' and a string value />
=first-name= "Alice" </ A key/value pair with compact keyword using = specifiers />
```

#### Compact Syntax
Most elements use a single specifier character (or a pair for collections) to denote the type. This is the most common syntax. Keywords require the `=` specifier with trailing specifiers, while identifiers require the `:` specifier.

```xfer
~true               </ A boolean />
*123.45             </ A decimal />
"Hello, World!"     </ A string />
[ 1 2 3 ]           </ An array of integers />
:identifier:        </ An identifier using : specifiers />
```

#### Explicit Syntax
When an element's content might be ambiguous (e.g., a string containing a quote), you can wrap the compact form in angle brackets (`<` and `>`). This is the most verbose but also the most powerful form, as it allows for delimiter repetition to avoid any collision.

```xfer
<"Alice said, "Boo!"">
<// A comment containing </another comment/> //>
<=first-name=> "Alice" </ An explicit keyword key with = specifiers />
```

### Element Reference

This section provides a detailed reference for each XferLang element type.

#### Primitive Types

**String Element**
*   **Specifier:** `"` (Quotation Mark)
*   **Description:** Contains text data. The content is stored verbatim. To include a `"` character that would conflict with the closing delimiter, repeat the specifier (e.g., `""...""`) or use explicit syntax (`<"..."">`).
*   **Syntax:**
    *   **Compact:** `"Hello, World!"`
    *   **Explicit:** `<"Alice said, "Boo!"">` or `<""A quote is a " character."">` (delimiter repetition)
*   **Examples:**
    ```xfer
    </ Compact syntax />
    message "Hello, World!"

    </ Explicit syntax with quotes />
    quote <"Alice said, "Boo!"">

    </ Delimiter repetition />
    description ""A quote is a " character.""
    ```

**Character Element**
*   **Specifier:** `\` (Backslash)
*   **Description:** Represents a single character, specified by its Unicode codepoint in decimal, hex (`$`), or binary (`%`), or by a predefined keyword (e.g., `tab`, `lf`, `gt`).
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

**Integer Element**
*   **Specifier:** `#` (Number Sign)
*   **Description:** A 32-bit signed integer. Can be written in decimal, hex (`$`), or binary (`%`). The specifier is optional if the syntax is unambiguous (implicit syntax).
*   **Syntax:**
    *   **Implicit:** `42`, `-123` (when unambiguous)
    *   **Compact:** `#42`, `#$2A`, `#%-123`, `#%00101010`
    *   **Explicit:** `<#42#>`, `<#$2A#>`, `<#%00101010#>`
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

**Long Element**
*   **Specifier:** `&` (Ampersand)
*   **Description:** A 64-bit signed integer. Can be written in decimal, hex (`$`), or binary (`%`).
*   **Syntax:**
    *   **Compact:** `&5000000000`, `&$12A05F200`, `&%1001010100000010111110010000000000`
    *   **Explicit:** `<&5000000000&>`, `<&$12A05F200&>`, `<&%1001010100000010111110010000000000&>`
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

**Double Element**
*   **Specifier:** `^` (Caret)
*   **Description:** A 64-bit floating-point number.
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

**Decimal Element**
*   **Specifier:** `*` (Asterisk)
*   **Description:** A high-precision decimal value.
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

**Boolean Element**
*   **Specifier:** `~` (Tilde)
*   **Description:** Represents a `true` or `false` value.
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

**Date/Time Element**
*   **Specifier:** `@` (At Sign)
*   **Description:** Represents a date and time value in ISO 8601 format.
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

**Null Element**
*   **Specifier:** `?` (Question Mark)
*   **Description:** Represents a null value.
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

#### Hexadecimal and Binary Formatting

Integer and Long elements support alternative numeric representations for improved readability in specific contexts:

**Hexadecimal Format**
*   **Syntax:** `$` prefix followed by hexadecimal digits (e.g., `#$2A`, `&$12A05F200`)
*   **Use Cases:** Memory addresses, color values, bitmasks, low-level programming
*   **Parsing:** Case-insensitive (`#$2A` equals `#$2a`)
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Hexadecimal, MinDigits = 4)]` for zero-padding

**Binary Format**
*   **Syntax:** `%` prefix followed by binary digits (e.g., `#%101010`, `&%1001010100000010111110010000000000`)
*   **Use Cases:** Bit manipulation, flags, educational purposes, embedded systems
*   **Attributes:** Use `[XferNumericFormat(XferNumericFormat.Binary, MinBits = 8)]` for zero-padding

**Examples:**
```xfer
{
    </ Decimal 42 in different formats />
    decimal 42
    hex #$2A
    binary #%101010
    padded_hex #$002A    </ MinDigits = 4 />
    padded_binary #%00101010  </ MinBits = 8 />
}
```

**Safety Note:** Hex and binary formatting are only supported for integer types (`int`, `long`). Decimal and double types preserve fractional precision and do not support these formats.

#### Structural Types

**Object Element**
*   **Specifiers:** `{` and `}` (Curly Brackets)
*   **Description:** A collection of key/value pairs. Keys are typically implicit keywords, and values can be any XferLang element.
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

**Array Element**
*   **Specifiers:** `[` and `]` (Square Brackets)
*   **Description:** A collection of elements of the **same type**.
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

**Tuple Element**
*   **Specifiers:** `(` and `)` (Parentheses)
*   **Description:** A collection of elements of **any type**, similar to a JSON array.
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

**Keyword Element and Key/Value Pairs**
*   **Description:** A keyword is the key in a key/value pair. If it contains only letters, numbers, and underscores, it can be written implicitly. Keywords used as keys require the `=` specifier. Identifiers (non-key keywords) use the `:` specifier.
*   **Syntax:**
    *   **Implicit:** `name`, `user_id`, `isActive` (letters, numbers, underscores only - valid only as keys)
    *   **Compact:** `=first-name=`, `=email-address=`, `=API-Key=` (keywords as keys with `=` specifier)
    *   **Explicit:** `<=first-name=>`, `<=email-address=>`, `<=API-Key=>` (keywords as keys with explicit syntax)
    *   **Identifier:** `:identifier:`, `<:identifier:>` (identifiers with `:` specifier)
*   **Examples:**
    ```xfer
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
    <=first-name=> "Alice"
    <=email-address=> "user@example.com"
    <=API-Key=> "secret123"
    <=content-type=> "application/json"

    </ Identifiers (not keys) />
    type :user:
    category :admin:
    status :active:
    ```

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

#### Special-Purpose Types

**Processing Instruction Element**
*   **Specifier:** `!` (Exclamation Mark)
*   **Description:** Contains processing instructions for the document, such as the `document` PI which stores document metadata.
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

**Comment Element**
*   **Specifier:** `/` (Slash)
*   **Description:** A comment that is ignored by the parser. It always requires explicit syntax.
*   **Syntax:**
    *   **Explicit:** `</ comment />`, `<// nested </comment/> //>` (delimiter repetition)
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

**Dynamic Element**
*   **Specifier:** `|` (Pipe)
*   **Description:** Represents a value to be substituted at runtime, by default from an environment variable. You can override the default dynamic value resolution by subclassing `DefaultDynamicSourceResolver` or by implementing the `IDynamicSourceResolver` interface. This allows you to provide custom logic for resolving dynamic values in your XferLang documents. Inside dynamic elements, all nested elements must use explicit syntax.
*   **Syntax:**
    *   **Compact:** `|USERNAME|`, `|DB_PASSWORD|`
    *   **Explicit:** `<|USERNAME|>`, `<|DB_PASSWORD|>`
*   **Examples:**
    ```xfer
    </ Compact syntax />
    username |USER|
    password |DB_PASSWORD|
    apiKey |vault:api-key|

    </ Explicit syntax />
    greeting <|USERNAME|>
    config <|file:app.config|>

    </ Within interpolated strings />
    message 'Hello, <|USERNAME|>!'

    </ Complex dynamic content with explicit syntax required inside />
    template |<"Hello, "><|USER|><"!">|
    ```

**Interpolated Text Element**
*   **Specifier:** `'` (Apostrophe)
*   **Description:** Similar to a string, but embedded elements are evaluated and their values are interpolated into the final text.
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

### Document Validation and Common Mistakes

## XferLang Elements

XferLang supports a rich set of data types designed for clarity, explicitness, and flexibility. Each type is chosen to represent a distinct kind of value, making data both human-readable and machine-precise. The main categories are:

- **Numbers**: Integers, longs, decimals, and doubles for numeric values.
- **Booleans**: True/false values for logic and state.
- **Characters**: Character data with a large set of pre-defined character keywords and facilities to define custom keywords.
- **Strings**: Textual data, including interpolated strings.
- **Dates and Times**: ISO Date/time values.
- **Nulls**: Explicit representation of missing or undefined values.
- **Objects**: Named key-value pairs for structured records.
- **Arrays and Tuples**: Collections, either typed (arrays) or mixed (tuples).
- **Dynamic**: Values that can be resolved at runtime, such as environment variables or secrets.
- **Processing Instructions**: Document-level processing instructions for configuration and extensibility.

### Array

Represents a typed array of elements. Arrays are enclosed in square brackets `[ ... ]` and all elements must be of the same type.

**Syntax:** `[ element1 element2 element3 ... ]`

**Examples:**
```xfer
numbers [1 2 3 4 5]
names ["Alice" "Bob" "Charlie"]
decimals [*12.3 *45.6 *78.9]
booleans [~true ~false ~true]
chars [\A \B \C]
```

Arrays can be nested or contain objects:
```xfer
matrix [
    [1 2 3]
    [4 5 6]
    [7 8 9]
]
users [
    { name "Alice" age 30 }
    { name "Bob" age 25 }
]
```

### Key/Value Pair

Represents the fundamental building block of XferLang objects - a key-value association. The key must be a keyword element (not an identifier), and the value can be any XferLang element. Key-value pairs form the basis of structured data in objects.

**Syntax:** `keyword value`

**Key Types:**
- Implicit keyword: `name "Alice"` (letters, numbers, underscores only)
- Compact keyword: `=first-name= "Alice"` (uses `=` specifier)
- Explicit keyword: `<=first-name=> "Alice"` (explicit syntax with `=` specifier)

**Examples:**
```xfer
</ Basic key-value pairs with implicit keywords />
name "Alice"
age 30
isActive ~true
score *99.5
lastLogin @2023-12-25T10:30:00Z@

</ Keys with special characters using = specifier />
=first-name= "John"
=email-address= "user@example.com"
=content-type= "application/json"
=user-id= 12345

</ Explicit syntax for complex keys />
<=first-name=> "Alice"
<=API-Key=> "secret123"
<=cache-control=> "no-cache"

</ Complex values />
profile {
    personal {
        name "Alice Smith"
        birth_date @1990-05-15@
    }
    settings {
        theme "dark"
        notifications ~true
    }
}

</ Arrays as values />
roles [ "admin" "user" "moderator" ]
scores [ *85.5 *92.0 *78.3 ]

</ Mixed data types />
metadata {
    created @2023-12-25T10:30:00Z@
    version "1.0"
    enabled ~true
    priority 5
    tags [ "important" "urgent" ]
    config ?
}
```

**Rules:**
- Keys must be keywords (using `=` specifier or implicit syntax), not identifiers (`:` specifier)
- Keys must be unique within the same object
- Whitespace separates the key from its value
- Only keywords can be used as keys; identifiers cannot be keys

### Object

Represents a collection of key-value pairs enclosed in curly braces. Objects are the most common structural element in XferLang, equivalent to objects in JSON or dictionaries in Python.

**Syntax:** `{ key1 value1 key2 value2 ... }`

**Examples:**
```xfer
</ Simple object />
user {
    name "Alice"
    age 30
    active ~true
}

</ Nested objects />
config {
    database {
        host "localhost"
        port 5432
        ssl ~true
    }
    cache {
        enabled ~true
        ttl 3600
    }
}

</ Compact form />
profile { name "Bob" role "admin" verified ~true }
```

### Tuple

Represents an ordered collection of elements that can be of **any type**, similar to arrays in JSON. Tuples are enclosed in parentheses and are perfect for heterogeneous data like coordinates, records, or mixed-type sequences.

**Syntax:** `( element1 element2 element3 ... )`

**Examples:**
```xfer
</ Geographic coordinates (latitude, longitude) />
location (*42.3601 *-71.0589)

</ Mixed types: name, age, active status />
userRecord ("John Doe" 30 ~true)

</ Complex tuple with various types />
dataPoint (
    "Sample A"
    @2023-12-25T10:30:00@
    *98.7
    ~true
    [ "tag1" "tag2" "tag3" ]
    { metadata "experimental" }
)

</ Compact form />
rgb (#255 #128 #64)
```

**Key Difference:** Tuples allow mixed types, while Arrays require all elements to be the same type.

### Comment

Comments provide documentation and annotations within XferLang documents. They are ignored during parsing and can be used for explanations, notes, or temporarily disabling content.

**Syntax:** Always requires explicit form: `</ comment text />`

**Delimiter Repetition:** For comments containing `/` characters, repeat the delimiter:
```xfer
</ This is a simple comment />

<// This comment contains / characters and nested </comments/> //>

</// Multi-level nesting for complex content ///>

</
Multi-line comments
can span several lines
and include any content
/>
```

**Inline Usage:**
```xfer
name "Alice" </ User's display name />
port 8080     </ Development server port />
```

### Dynamic

Dynamic elements represent values that are resolved at parse-time or runtime, typically from environment variables, configuration files, or custom sources. They provide powerful templating and configuration capabilities. Inside dynamic elements, all nested elements must use explicit syntax.

**Syntax:** `|value_key|` or `<|value_key|>`

**Examples:**
```xfer
</ Environment variable substitution />
username |USER|
password |DB_PASSWORD|

</ Within interpolated strings />
greeting 'Hello, <|USERNAME|>!'
message 'Server running on port <|PORT|>'

</ Complex dynamic content - explicit syntax required inside />
dynamicObject |<{<=name=><"Alice"><=age=><#30#>}>|
dynamicArray |<[<"item1"><"item2"><"item3">]>|
```

**Configuration via Processing Instructions:**
```xfer
<! dynamicSource {
    greeting const "Welcome to XferLang"
    username env "USER"
    config file "settings.json"
} !>
{
    message '<|greeting|>'
    user '<|username|>'
    settings '<|config|>'
}
```

**Rules:**
- Inside dynamic elements, all nested elements must use explicit syntax
- This ensures unambiguous parsing when dynamic content is resolved

### Identifier

Identifiers represent symbolic names which are not necessarily string values, semantically. Identifiers use the `:` specifier and cannot be used as keys in key/value pairs.

**Compact Syntax:** `:identifier:`
**Explicit Syntax:** `<:identifier:>`

**Examples:**
```xfer
</ Simple identifiers (compact) />
option :formatted:
varname :integer:
type :user:

</ Explicit syntax />
category <:admin:>
status <:active:>

</ In objects as values (not keys) />
config {
    logLevel :warning:
    cacheMode :enabled:
    userType :premium:
}
```

**Rules:**
- Identifiers always require the `:` specifier with trailing delimiters
- Identifiers cannot be used as keys in key/value pairs (only keywords can be keys)
- Use explicit syntax when the identifier content might be ambiguous

### Integer

Represents a 32-bit signed integer value. Integers can be written in decimal, hexadecimal, or binary formats and support implicit syntax when the context is unambiguous.

**Syntax:**
- Implicit: `42` (when unambiguous)
- Compact: `#42`
- Explicit: `<#42#>`
- Hexadecimal: `#$2A`
- Binary: `#%101010`

**Examples:**
```xfer
</ Implicit form (most common) />
age 30
count 1000
negative -42

</ Compact form />
port #8080
timeout #30

</ Explicit form  />
<'https://<|address|>:<#80#>/'>

</ Alternative number bases />
colorRed #$FF
permissions #%11110000
memoryAddress #$DEADBEEF

</ In collections />
numbers [ 1 2 3 4 5 ]
ports [ #80 #443 #8080 ]
```

**Range:** -2,147,483,648 to 2,147,483,647

### Long

Represents a 64-bit signed integer value for larger numbers that exceed the 32-bit integer range. Always requires the `&` prefix to distinguish from regular integers.

**Syntax:**
- Decimal: `&5000000000`
- Hexadecimal: `&$12A05F200`
- Binary: `&%1001010100000010111110010000000000`

**Examples:**
```xfer
</ Large numbers />
population &7800000000
fileSize &5368709120
timestamp &1672531200000

</ Hexadecimal (common for IDs, addresses) />
userId &$1A2B3C4D5E6F
memoryOffset &$7FF6C2E40000

</ Binary (for flags, masks) />
featureFlags &%1111000011110000111100001111

</ In configuration />
limits {
    maxFileSize &2147483648
    maxUsers &1000000
}
```

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
        format "{timestamp} [{level}] {message}"
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

## Grammar Reference

For those who want to implement parsers or need the complete technical specification, here's the formal grammar for XferLang:

```bnf
/* XferLang Grammar (BNF)
 *
 * Key Requirements:
 * - All XferLang documents must have exactly one root collection element (Object, Array, or Tuple)
 * - Optional processing instructions may precede the root collection
 * - Processing instructions provide document metadata and configuration
 *
 * Examples of valid documents:
 * - { name "Alice" age 30 }                                    (Object root)
 * - [ 1 2 3 ]                                                 (Array root)
 * - ( "title" 42 ~true )                                      (Tuple root)
 * - <! document { version "1.0" } !> { config "value" }       (With processing instructions)
 */

<document> ::= <opt_whitespace> <processing_instructions>? <opt_whitespace> <collection_element> <opt_whitespace>

<collection_element> ::= <object_element> | <array_element> | <tuple_element>

<processing_instructions> ::= <processing_instruction>+

<processing_instruction> ::= <processing_instruction_explicit> | <processing_instruction_compact>
<processing_instruction_explicit> ::= <element_open> <pi_specifier> <opt_whitespace> <key_value_pair>+ <opt_whitespace> <pi_specifier> <element_close>
<processing_instruction_compact> ::= <pi_specifier> <opt_whitespace> <key_value_pair>+ <opt_whitespace> <pi_specifier>

<body_element> ::= <opt_whitespace> (
      <key_value_pair>
    | <string_element>
    | <character_element>
    | <integer_element>
    | <long_element>
    | <double_element>
    | <decimal_element>
    | <boolean_element>
    | <datetime_element>
    | <date_element>
    | <time_element>
    | <timespan_element>
    | <null_element>
    | <object_element>
    | <array_element>
    | <tuple_element>
    | <dynamic_element>
    | <interpolated_element>
    | <comment_element>
    | <identifier_element>
) <opt_whitespace>

<key_value_pair> ::= <identifier_element> <whitespace> <body_element>

/* Collection Elements */
<object_element> ::= <object_element_explicit> | <object_element_compact>
<object_element_explicit> ::= <element_open> <object_open> <opt_whitespace> <key_value_pair>* <opt_whitespace> <object_close> <element_close>
<object_element_compact> ::= <object_open> <opt_whitespace> <key_value_pair>* <opt_whitespace> <object_close>

<array_element> ::= <array_element_explicit> | <array_element_compact>
<array_element_explicit> ::= <element_open> <array_open> <opt_whitespace> <body_element>* <opt_whitespace> <array_close> <element_close>
<array_element_compact> ::= <array_open> <opt_whitespace> <body_element>* <opt_whitespace> <array_close>

<tuple_element> ::= <tuple_element_explicit> | <tuple_element_compact>
<tuple_element_explicit> ::= <element_open> <tuple_open> <opt_whitespace> <body_element>* <opt_whitespace> <tuple_close> <element_close>
<tuple_element_compact> ::= <tuple_open> <opt_whitespace> <body_element>* <opt_whitespace> <tuple_close>

/* Basic Data Elements */
<string_element> ::= <string_element_explicit> | <string_element_compact>
<string_element_explicit> ::= <element_open> <string_specifier>+ <string_content> <string_specifier>+ <element_close>
<string_element_compact> ::= <string_specifier>+ <string_content> <string_specifier>+

<character_element> ::= <character_element_explicit> | <character_element_compact>
<character_element_explicit> ::= <element_open> <character_specifier> <character_content> <element_close>
<character_element_compact> ::= <character_specifier> <character_content>

<integer_element> ::= <integer_element_explicit> | <integer_element_compact> | <integer_element_implicit>
<integer_element_explicit> ::= <element_open> <integer_specifier> <integer_content> <element_close>
<integer_element_compact> ::= <integer_specifier> <integer_content>
<integer_element_implicit> ::= <integer_content>

<long_element> ::= <long_element_explicit> | <long_element_compact>
<long_element_explicit> ::= <element_open> <long_specifier> <long_content> <element_close>
<long_element_compact> ::= <long_specifier> <long_content>

<double_element> ::= <double_element_explicit> | <double_element_compact>
<double_element_explicit> ::= <element_open> <double_specifier> <double_content> <element_close>
<double_element_compact> ::= <double_specifier> <double_content>

<decimal_element> ::= <decimal_element_explicit> | <decimal_element_compact>
<decimal_element_explicit> ::= <element_open> <decimal_specifier> <decimal_content> <element_close>
<decimal_element_compact> ::= <decimal_specifier> <decimal_content>

<boolean_element> ::= <boolean_element_explicit> | <boolean_element_compact>
<boolean_element_explicit> ::= <element_open> <boolean_specifier> <boolean_content> <element_close>
<boolean_element_compact> ::= <boolean_specifier> <boolean_content>

<datetime_element> ::= <datetime_element_explicit> | <datetime_element_compact>
<datetime_element_explicit> ::= <element_open> <datetime_specifier> <datetime_content> <datetime_specifier> <element_close>
<datetime_element_compact> ::= <datetime_specifier> <datetime_content> <datetime_specifier>

<null_element> ::= <null_element_explicit> | <null_element_compact>
<null_element_explicit> ::= <element_open> <null_specifier>+ <element_close>
<null_element_compact> ::= <null_specifier>+

/* Special Elements */
<dynamic_element> ::= <dynamic_element_explicit> | <dynamic_element_compact>
<dynamic_element_explicit> ::= <element_open> <dynamic_specifier> <dynamic_content> <dynamic_specifier> <element_close>
<dynamic_element_compact> ::= <dynamic_specifier> <dynamic_content> <dynamic_specifier>

<interpolated_element> ::= <interpolated_element_explicit> | <interpolated_element_compact>
<interpolated_element_explicit> ::= <element_open> <interpolated_specifier>+ <interpolated_content> <interpolated_specifier>+ <element_close>
<interpolated_element_compact> ::= <interpolated_specifier>+ <interpolated_content> <interpolated_specifier>+

<comment_element> ::= <element_open> <comment_specifier>+ <comment_content> <comment_specifier>+ <element_close>

<identifier_element> ::= <identifier_element_explicit> | <identifier_element_implicit>
<identifier_element_explicit> ::= <identifier_specifier> <identifier_content> <identifier_specifier>
<identifier_element_implicit> ::= <identifier_content>

/* Specifiers and Delimiters */
<element_open> ::= "<"
<element_close> ::= ">"

<object_open> ::= "{"
<object_close> ::= "}"
<array_open> ::= "["
<array_close> ::= "]"
<tuple_open> ::= "("
<tuple_close> ::= ")"

<string_specifier> ::= "\""
<character_specifier> ::= "\\"
<integer_specifier> ::= "#"
<long_specifier> ::= "&"
<double_specifier> ::= "^"
<decimal_specifier> ::= "*"
<boolean_specifier> ::= "~"
<datetime_specifier> ::= "@"
<null_specifier> ::= "?"
<dynamic_specifier> ::= "|"
<interpolated_specifier> ::= "'"
<comment_specifier> ::= "/"
<identifier_specifier> ::= ":"
<pi_specifier> ::= "!"

/* Content Patterns */
<string_content> ::= <any_character_except_delimiter>*
<character_content> ::= <unicode_codepoint> | <character_keyword>
<integer_content> ::= <decimal_digits> | <hex_digits> | <binary_digits>
<long_content> ::= <decimal_digits> | <hex_digits> | <binary_digits>
<double_content> ::= <floating_point_number>
<decimal_content> ::= <decimal_number>
<boolean_content> ::= "true" | "false"
<datetime_content> ::= <iso8601_datetime>
<dynamic_content> ::= <identifier_chars>
<interpolated_content> ::= <any_character_or_element>*
<comment_content> ::= <any_character_except_delimiter>*
<identifier_content> ::= <identifier_chars>

/* Whitespace */
<whitespace> ::= <space> | <tab> | <newline> | <carriage_return>
<opt_whitespace> ::= <whitespace>*

/* Character Classes */
<decimal_digits> ::= ["-"]? [0-9]+
<hex_digits> ::= "$" [0-9A-Fa-f]+
<binary_digits> ::= "%" [01]+
<floating_point_number> ::= ["-"]? [0-9]+ "." [0-9]+ (("e" | "E") ["-"]? [0-9]+)?
<decimal_number> ::= ["-"]? [0-9]+ ("." [0-9]+)?
<iso8601_datetime> ::= [0-9]{4} "-" [0-9]{2} "-" [0-9]{2} ("T" [0-9]{2} ":" [0-9]{2} ":" [0-9]{2} ("." [0-9]+)? ("Z" | [+-] [0-9]{2} ":" [0-9]{2})?)?
<identifier_chars> ::= [a-zA-Z_] [a-zA-Z0-9_]*
<unicode_codepoint> ::= [0-9]+ | "$" [0-9A-Fa-f]+ | "%" [01]+
<character_keyword> ::= "tab" | "space" | "newline" | "cr" | "lf" | "crlf" | "null" | "bell" | ...
<any_character_except_delimiter> ::= <any_unicode_character_except_current_delimiter>
<any_character_or_element> ::= <any_unicode_character> | <body_element>
<space> ::= " "
<tab> ::= "\t"
<newline> ::= "\n"
<carriage_return> ::= "\r"
```

## Getting Started with .NET

The XferLang .NET library provides a robust implementation for parsing, generating, and working with XferLang documents programmatically.

### Installation

Install the NuGet package in your .NET project:

```bash
dotnet add package ParksComputing.Xfer.Lang
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package ParksComputing.Xfer.Lang
```

### Quick Start Example

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

## The `.NET XferLang Library`

The primary implementation of XferLang is the `ParksComputing.Xfer.Lang` library for .NET. It provides a comprehensive object model, a robust parser, and a powerful serialization/deserialization utility class, `XferConvert`.

### Basic Serialization and Deserialization

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

## Serializer Settings

For more control over serialization and deserialization, you can use the `XferSerializerSettings` class. This allows you to configure element styles, null handling, contract resolvers, and custom converters.

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

### Safety Notes

- Numeric formatting attributes are only applied to `int` and `long` properties
- `decimal` and `double` types ignore formatting attributes to preserve fractional precision
- Custom formatting respects the configured `ElementStylePreference` for syntax style

## Processing Instructions and Dynamic Content

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

## Writing Custom Processing Instruction (PI) Processors

XferLang.NET allows you to extend the parser and deserializer by implementing custom logic for Processing Instructions (PIs). This is useful for advanced configuration, runtime directives, schema association, and dynamic value resolution.

### Conceptual Overview

- PIs are metadata blocks that can control parsing, validation, and runtime behavior.
- Custom PI processors let you interpret and act on PIs in your own way.
- Common use cases: environment overrides, feature flags, schema binding, dynamic sources.

### .NET API Usage

To handle custom PIs, implement or extend the relevant resolver or processor interface. For example, to handle `dynamicSource` PIs, implement `IDynamicSourceResolver` or subclass `DefaultDynamicSourceResolver`:

```csharp
public class MyDynamicSourceResolver : DefaultDynamicSourceResolver
{
    public override string? Resolve(string key)
    {
        // Custom logic for dynamic keys
        if (key == "special")
            return "custom-value";
        // Fallback to default
        return base.Resolve(key);
    }
}

// Usage:
var settings = new XferSerializerSettings
{
    DynamicSourceResolver = new MyDynamicSourceResolver()
};

var obj = XferConvert.Deserialize<MyConfig>(xferString, settings);
```

## Building XferLang

Information for developers who want to build XferLang from source or contribute to the project.

### Prerequisites

- **.NET SDK 8.0 or later** - [Download from Microsoft](https://dotnet.microsoft.com/download)
- **Git** - For cloning the repository
- **IDE (Optional):** Visual Studio, VS Code, or Rider

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

The XferLang solution contains several projects:

- **ParksComputing.Xfer.Lang** - Main library
- **XferTest** - Unit tests and integration tests
- **xferc** - Command-line tools and REPL
- **XferService** - Web service implementation (optional)

### Development Commands

```bash
# Build in Debug mode
dotnet build --configuration Debug

# Build in Release mode
dotnet build --configuration Release

# Run specific test project
dotnet test XferTest/

# Run with verbose output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Package for NuGet distribution
dotnet pack ParksComputing.Xfer.Lang/ --configuration Release --output ./nupkg
```

### Using the Command-Line Tools

The `xferc` project provides command-line tools for working with XferLang:

```bash
# Build the command-line tools
dotnet build xferc/

# Run the XferLang REPL
dotnet run --project xferc/

# Convert JSON to XferLang
dotnet run --project xferc/ -- convert input.json output.xfer

# Validate XferLang documents
dotnet run --project xferc/ -- validate document.xfer

# Pretty-print XferLang documents
dotnet run --project xferc/ -- format document.xfer
```

## Community and Resources

Join the XferLang community and access helpful resources for learning and development.

### Learning Resources

- 📖 **Documentation:** [This comprehensive guide](https://github.com/paulmooreparks/Xfer)
- 🎯 **Examples:** [Sample XferLang applications](https://github.com/paulmooreparks/Xfer/tree/master/examples) in the repository
- 💡 **Tests:** [Unit Tests](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer.Lang.Tests) also show how to use the library and provide test coverage
- 📄 **Sample Documents:** [*.xfer files](https://github.com/paulmooreparks/Xfer/tree/master) in the repository

### Getting Help

- ❓ **Questions:** Open a [GitHub Discussion](https://github.com/paulmooreparks/Xfer/discussions)
- 🐛 **Bug Reports:** Create an [Issue](https://github.com/paulmooreparks/Xfer/issues) with details
- 💡 **Feature Requests:** Suggest improvements via [GitHub Issues](https://github.com/paulmooreparks/Xfer/issues)
- 📧 **Direct Contact:** Reach out via GitHub for complex questions

### Development Status

XferLang is actively developed with regular updates and improvements:

- ✅ **Stable Core:** Basic serialization/deserialization is production-ready
- 🚧 **Active Development:** Advanced features and optimizations ongoing
- 🔄 **Regular Releases:** Check [releases](https://github.com/paulmooreparks/Xfer/releases) for updates
- 📊 **CI/CD:** Automated testing and quality checks via GitHub Actions

### Language Comparison

See how XferLang compares to other data formats:

| Feature | XferLang | JSON | XML | YAML |
|---------|----------|------|-----|------|
| **Explicit Types** | ✅ Built-in | ❌ Limited | ⚠️ Via attributes | ⚠️ Via tags |
| **Human Readable** | ✅ Yes | ✅ Yes | ⚠️ Verbose | ✅ Yes |
| **Comments** | ✅ Native | ❌ No | ✅ Native | ✅ Native |
| **Safety** | ✅ High | ⚠️ Medium | ✅ High | ⚠️ Medium |
| **Delimiter Escaping** | ✅ Not needed | ❌ Required | ❌ Required | ⚠️ Context-dependent |
| **Type Ambiguity** | ✅ None | ❌ High | ⚠️ Medium | ❌ High |
| **Compact Size** | ✅ Good | ✅ Excellent | ❌ Poor | ✅ Good |
| **Parsing Speed** | ✅ Fast | ✅ Fast | ⚠️ Moderate | ⚠️ Moderate |

## Project Status and Roadmap

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
