![XferLogo](https://raw.githubusercontent.com/paulmooreparks/Xfer/master/logo/XferLang-sm.png)

# The XferLang Data-Interchange Format

[![GitHub last commit](https://img.shields.io/github/last-commit/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer)
[![.NET Build Status](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/paulmooreparks/Xfer/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/vpre/ParksComputing.Xfer.Lang.svg)](https://www.nuget.org/packages/ParksComputing.Xfer.Lang)
[![GitHub issues](https://img.shields.io/github/issues/paulmooreparks/Xfer)](https://github.com/paulmooreparks/Xfer/issues)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Visual Studio Code Extension

A Visual Studio Code extension for XferLang is now available in the Visual Studio marketplace:

https://marketplace.visualstudio.com/items?itemName=paulmooreparks.xferlang

Open any `.xfer` file to activate. See the extension's README for details and changelog.

## Table of Contents
- [The XferLang Data-Interchange Format](#the-xferlang-data-interchange-format)
  - [Visual Studio Code Extension](#visual-studio-code-extension)
  - [Table of Contents](#table-of-contents)
  - [Introduction and Philosophy](#introduction-and-philosophy)
  - [XferLang by Example](#xferlang-by-example)
    - [A Direct Comparison](#a-direct-comparison)
    - [Core XferPath](#core-xferpath)
    - [Delimiter / Specifier Duplication (Uniform, Escape‑Free Rule)](#delimiter--specifier-duplication-uniform-escapefree-rule)
    - [Adding Document Metadata (Document PI)](#adding-document-metadata-document-pi)
    - [Dynamic Elements (Late Binding) \& Source Overrides](#dynamic-elements-late-binding--source-overrides)
      - [File, Env, and Const Together](#file-env-and-const-together)
    - [A Quick Tour of Processing Instructions](#a-quick-tour-of-processing-instructions)
    - [Interpolated Text (Structured, Not Escaped)](#interpolated-text-structured-not-escaped)
    - [Local Bindings with `let` (Dereference for Reuse)](#local-bindings-with-let-dereference-for-reuse)
    - [Grouped Let Bindings (`script` PI)](#grouped-let-bindings-script-pi)
    - [Conditional Inclusion (`if` PI)](#conditional-inclusion-if-pi)
    - [Character Definitions](#character-definitions)
    - [What You Just Saw (Feature Progression)](#what-you-just-saw-feature-progression)
  - [Core Concepts](#core-concepts)
    - [Elements and Syntax Forms](#elements-and-syntax-forms)
    - [Collections](#collections)
    - [Keywords vs Identifiers](#keywords-vs-identifiers)
    - [Numbers](#numbers)
    - [Text and Characters](#text-and-characters)
    - [Date / Time](#date--time)
    - [Null](#null)
    - [Binding and Dereference](#binding-and-dereference)
    - [Interpolated Text](#interpolated-text)
    - [Dynamic Elements](#dynamic-elements)
    - [Processing Instructions (PIs)](#processing-instructions-pis)
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
    - [XferDecimalPrecisionAttribute](#xferdecimalprecisionattribute)
    - [Safety Notes](#safety-notes)
  - [Processing Instructions and Dynamic Content](#processing-instructions-and-dynamic-content)
    - [Built-in Processing Instructions](#built-in-processing-instructions)
      - [Document Metadata PI](#document-metadata-pi)
      - [DynamicSource PI](#dynamicsource-pi)
      - [CharDef PI](#chardef-pi)
      - [ID PI](#id-pi)
      - [Tag PI](#tag-pi)
      - [Defined PI](#defined-pi)
    - [Dynamic Elements and Source Resolution](#dynamic-elements-and-source-resolution)
      - [Built-in Source Types](#built-in-source-types)
      - [Custom Source Handler Registration](#custom-source-handler-registration)
    - [Extending Processing Instructions](#extending-processing-instructions)
      - [Creating Custom PIs](#creating-custom-pis)
      - [PI Registration and Lifecycle](#pi-registration-and-lifecycle)
  - [Writing Custom Processing Instructions](#writing-custom-processing-instructions)
    - [When to Create a PI](#when-to-create-a-pi)
    - [Parser Registration Model](#parser-registration-model)
    - [Minimal Custom PI Example (`trace`)](#minimal-custom-pi-example-trace)
    - [Conditioning the Next Element](#conditioning-the-next-element)
    - [Validation PI Skeleton](#validation-pi-skeleton)
    - [Extending Dynamic Resolution (No New PI Required)](#extending-dynamic-resolution-no-new-pi-required)
    - [Checklist for a New PI](#checklist-for-a-new-pi)
    - [Lifecycle Summary](#lifecycle-summary)
  - [Building XferLang](#building-xferlang)
    - [Prerequisites](#prerequisites)
    - [Quick Start](#quick-start)
    - [Project Structure](#project-structure)
    - [Development Commands](#development-commands)
  - [Community and Resources](#community-and-resources)
    - [Learning Resources](#learning-resources)
    - [Getting Help](#getting-help)
  - [Project Status and Roadmap](#project-status-and-roadmap)
  - [Contributing](#contributing)
  - [Grammar](#grammar)

## Introduction and Philosophy

XferLang is a strongly‑typed, delimiter‑driven data‑interchange format for configuration, structured content, and templated runtime inputs. It aims to feel instantly familiar to anyone who knows JSON—then give you the things you keep wishing JSON had: explicit types, whitespace flexibility, structured interpolation, safe embedding without escape sequences, late binding, and parse‑time directives.

Guiding principles:

* **Readable by default** – Minimal punctuation; whitespace separates elements when that’s unambiguous. You can pretty‑print or collapse aggressively without changing meaning.
* **Explicit types** – Every value carries an unambiguous type marker (boolean, decimal, date/time, etc.). Ambiguous heuristics disappear; downstream code stops guessing.
* **No escape tax** – Instead of backslash escaping inside strings (or other forms), XferLang lets you lengthen the opening/closing specifier run. Need an embedded quote? Double the quotes. Need a pipe inside a dynamic element? Use a longer pipe run. This works uniformly across element types.
* **Safe arbitrary embedding** – Because specifiers can lengthen, you can drop in generated fragments, templated chunks, or foreign content without pre‑processing or opaque escaping layers.
* **Single structured root** – Every document has exactly one root collection (object, array, or tuple). This removes edge ambiguity, enables streaming readers, and aligns cleanly with schema / validation tooling.
* **Parse‑time intent** – Processing Instructions (PIs) let you bind values, declare dynamic sources, conditionally include elements, and define symbolic characters—without blurring the core data model.

## XferLang by Example
Let's build intuition first. Each snippet introduces one dimension of capability; together they show how you get power without giving up clarity.

### A Direct Comparison

Simple XferLang document:

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
}
```

Equivalent JSON:

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

Newlines are optional in XferLang, and whitespace can be reduced to only the minimum necessary to separate elements. Therefore, the same XferLang document shown above can be written very compactly:

```xfer
{name"Alice"age 30 isMember~true scores[*85 *90 *78.5]profile{email"alice@example.com"joinedDate@2023-01-15T12:00:00@}}
```

---

### Core XferPath

Explicit typing in XferLang clarifies intent and removes ambiguity while remaining terse:

```xfer
sample {
    title "Demo"
    retries 3          </ implicit integer />
    ratio *0.8125      </ high‑precision decimal />
    threshold ^0.5     </ double />
    active ~true       </ boolean />
    launched @2025-08-01T09:30:00Z@
    tags [ "alpha" "preview" ]
    point ( *42.3601 *-71.0589 )
    notes ""No escaping needed: "quotes" stay simple.""
    optional ?
}
```

Arrays are homogeneous; tuples are heterogeneous. Angle‑bracket explicit forms (not shown here) are optional unless needed to prevent ambiguity.

### Delimiter / Specifier Duplication (Uniform, Escape‑Free Rule)

Instead of requiring escape sequences, XferLang lets you lengthen an element's delimiter sequence in order to allow special characters to be embedded in the element. The parser treats the entire contiguous run as the opening (and requires the same length run to close). Apply this anywhere a specifier pairs with itself or a matching closer:

* Strings (`"…"`): embed quotes by doubling: `""He said, "Hello".""`
* Dynamic elements (`|name|`): need a literal `|` inside? Use `||value with | symbol||`.
* Interpolated text (`'…'`): nest or include apostrophes by lengthening: `''Outer 'inner' still fine''`.
* Comments (`</ … />`): if interior contains `</`, lengthen the leading `/` run: `<// A comment with </inner/> safely //>`.
* Any explicit form (`<…>`): angle‑bracket wrapper isolates interior specifier runs when needed.

Examples:

```xfer
example {
    plain "Alpha"
    nestedQuotes ""He said, "Hello" then left.""
    dynamicWithPipe ||status|ok||        </ inner single | stays literal />
    commentDemo <// Outer </ inner /> still fine //>
    dateExample <@2025-08-01T12:00:00@>  </ explicit form keeps internal @ sequences unambiguous />
}
```

Guideline: choose the shortest delimiter length that avoids ambiguity; lengthen only when the content would otherwise prematurely terminate the element. This rule lets you embed arbitrarily complex content (including what look like closing tokens) without escapes.

### Adding Document Metadata (Document PI)

A Processing Instruction (PI) is a lightweight directive: exactly one key/value pair that the parser consumes before (optionally) influencing subsequent parsing. The `document` PI attaches version and other top‑level metadata:

```xfer
<! document { version "1.2" environment "prod" } !>
{
    service { host "api.example.com" port 8443 ssl ~true }
    maintenance ( @2025-01-15T02:00:00Z@ *2.5 ) </ start, hours />
}
```

### Dynamic Elements (Late Binding) & Source Overrides

Dynamic elements (`|key|`) defer value resolution until parse time. Resolution order:

1. A preceding `dynamicSource` PI mapping for `key` (e.g., `key file "path"`) invokes the registered handler for that source type.
2. If no mapping exists, the environment variable named `key` is read.
3. If still unresolved, the element becomes an empty string (parsed value `null`).

Content inside the pipes is raw text; nested elements are not parsed. For structural composition use interpolated text: `'Hello <|USER|>'`.

```xfer
{
    currentUser |USER|
    banner 'Hello <|USER|>'
}
```

#### File, Env, and Const Together

The `dynamicSource` processing instruction provides three built‑in source types: `const`, `env`, and `file`. Each key inside the `dynamicSource` object maps a dynamic element name to a configuration expressed as a key/value pair: `<name> <sourceType> <sourceValue>`. Resolution delegates to a handler registered for the source type.

```xfer
<! dynamicSource {
    apiKey    file "secrets/api-key.txt"   </ contents of file used as value />
    userName  env  "USER"                  </ environment variable />
    buildTag  const "2025.08.11"           </ literal constant />
} !>
{
    auth { key |apiKey| user |userName| tag |buildTag| }
    banner 'Deploy <|buildTag|> for <|userName|> (key length <#( len |apiKey| )#>)'
}
```

At this stage we've seen explicit types, metadata PI, and dynamic resolution. These features are already beyond JSON while remaining approachable.

### A Quick Tour of Processing Instructions

You have now met two PIs (`document`, `dynamicSource`). Additional built‑ins:
* `let` – bind a name to a value for later dereference
* `script` – batch multiple `let` bindings (let‑only today) so they all execute before the next element
* `if` – conditionally suppress the immediately following element (PI itself is always stripped)
* `defined` – test whether an element (binding / dynamic / literal) currently yields a meaningful value
* `chardef` – define symbolic character aliases
* `id` / `tag` – attach stable identifiers or free‑form tags to the following element

Shape: `<! name value !>` where `value` may be any element (often an object for multiple fields).

### Interpolated Text (Structured, Not Escaped)

Interpolated text (apostrophe‑delimited) embeds **real elements**, not ad‑hoc string fragments. Inside the body, wrap each embedded element in `< … >` using that element’s explicit form. The engine evaluates them structurally before producing the final string.

```xfer
<! dynamicSource { user env "USER" } !>
{
    banner 'User=<|user|> LoggedIn=<~true~> Since <@2025-08-01T09:30:00Z@>'
}
```

No string escape layer -— embedded parts are real elements, not ad‑hoc templating.

### Local Bindings with `let` (Dereference for Reuse)

Bind once, reuse structurally. A `let` PI introduces a name whose value you can later insert via a dereference element (leading underscore). Binding occurs before parsing subsequent siblings, so dereferences see earlier values.

```xfer
<! let base { host "localhost" port 8080 } !>
{
    primary _base
    secondary { host "localhost" port 8081 }
}
```

Bindings may be evaluated inside an interpolated string by using explicit syntax for the dereference element, `<_name_>`:

```xfer
<! let appName "XferDemo" !>
{ banner 'Launching <_appName_>...' }
```

### Grouped Let Bindings (`script` PI)

The `script` PI batches multiple `let` bindings so they all complete before the next element parses. (Today: only `let` forms are recognized; future operator expressions may be enabled.)

```xfer
<! script (
    let host "localhost"
    let port 8080
) !>
{
    serviceUrl 'https://<_host_>:<_port_>/'
}
```

Bindings are processed left‑to‑right. Later bindings may dereference earlier ones. Because the `script` PI contains only `let` bindings it is suppressed from serialized output.

### Conditional Inclusion (`if` PI)

The `if` PI evaluates a condition; if it’s false the immediately following element is never added to the document. The PI itself is always stripped regardless of outcome.

```xfer
<! let showDebug ~false !>
<! if _showDebug !>
{ debug { level "verbose" } }
```

Defined vs undefined (existence test) also matters:

```xfer
<! if defined _showDebug !> { note 'Binding exists even if false.' }
```

Behavior & serialization rules:

* If the condition evaluates to false, the target element (the immediately following sibling element) is completely suppressed; that is, it is never added to the parsed document model.
* If the condition evaluates to true, the target element is retained.
* Regardless of outcome (true, false, evaluation error, or unknown operator name) the `if` processing instruction itself is always stripped from serialization output. It acts only as a directive at parse time.
* An unknown operator name inside the condition currently acts as a no‑op (treated as truthy so the element is preserved) but the PI is still stripped. Future versions may surface a warning; do not rely on serialization visibility for diagnostics.
* Direct dereference conditions (`<! if _flag !>`) test the bound value's truthiness; use `defined` to distinguish between an undefined binding and a defined but falsy value.

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

### What You Just Saw (Feature Progression)

1. Plain explicit types (numbers, booleans, dates, tuples, arrays, objects, null, interpolated text)
2. Metadata via `document` PI
3. Dynamic values + overriding resolution with `dynamicSource`
4. Structured interpolation (no ad‑hoc escaping)
5. Local structural reuse with `let` and dereference (`_name` / `<_name_>`)
6. Batched let bindings via `script` (let-only today)
7. Conditional element inclusion with `if` (value truthiness & existence checks)
8. Custom symbolic characters with `chardef`
9. Existence evaluation with `defined`

These examples intentionally stop short of exhaustive syntax or full operator semantics; the following sections (Core Concepts → Language Specification) formalize everything just previewed.

The remainder of this document now drills into the core concepts, the formal language, and the .NET APIs.

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
Strings: `"..."` (repeat `"` to include the delimiter) or explicit `<"...">`.
Characters: `\65`, `\$41`, `\%01000001`, or predefined keywords (`\tab`).

### Date / Time
Date/time values use `@...@` with ISO‑8601 forms. (Date-only and time-only forms may be parsed where implemented.)

### Null
`?` represents a null value.

### Binding and Dereference
`<! let name <value> !>` binds `name` to `<value>`. Inside subsequent elements the value can be dereferenced with `_name` (or inside interpolation as `<_name_>`).

Batching bindings: The `script` processing instruction currently supports only the `let` operator. (Additional operators will be added prior to general release.) You can group several sequential `let` bindings inside a single tuple so they all execute before the next element parses:

```xfer
<! script ( let first "Alice" let greeting 'Hi <_first_>' let answer 42 ) !>
{ message _greeting number _answer }
```

All listed `let` bindings are evaluated in order; later bindings can reference earlier ones (as with `_first` inside the interpolated greeting) but self‑reference is prevented. Because the script PI here contains only `let` bindings it does not serialize into the output (it is suppressed after execution).

### Interpolated Text
Delimited by apostrophes: `'Hello <_name_>'`. Embedded elements inside must use explicit forms. Expressions are structural replacements; no character escaping is required.

### Dynamic Elements
`|identifier|` resolves through the configured dynamic source resolver (e.g., environment). Content is a single identifier; nested elements are not allowed inside the delimiters in the current implementation.

### Processing Instructions (PIs)
Compact: `! name <value> !`  Explicit: `<! name <value> !>`.
A PI consists of exactly one key/value pair; the value can be any element. Some PIs introduce bindings or affect subsequent parsing and may be suppressed from serialization after execution.

## Language Specification

### Document Structure

Order:
1. Zero or more processing instructions.
2. Exactly one root collection element (Object, Array, or Tuple).

Processing instructions each contain one key/value pair. For richer metadata group fields inside an object:

```xfer
<! document { version "1.0" author "Alice" } !>
<! let greeting "World" !>
{ message 'Hi <_greeting_>.' }
```

Comments (`</ ... />`) may appear anywhere and are ignored.

Root collection requirement avoids ambiguity and enables streaming parsers.

### Element Syntax Variations

XferLang elements have a flexible syntax with up to three variations. This allows you to choose the most readable and concise form for your data, only using more verbose syntax when necessary to resolve ambiguity.

#### Implicit Syntax
For integers and keywords, no special characters are needed when the context is unambiguous.

```xfer
123                 </ An integer />
name "Alice"        </ A key/value pair with implicit keyword 'name' and a string value />
```

#### Compact Syntax
Most elements use a single specifier character (or a pair for collections) to denote the type. This is the most common syntax. Keywords require the `=` specifier with trailing specifiers, while identifiers require the `:` specifier.

```xfer
~true                   </ A boolean />
*123.45                 </ A decimal />
"Hello, World!"         </ A string />
[ 1 2 3 ]               </ An array of integers />
=special keyword= 42    </ A keyword with an embedded space />
:identifier:            </ An identifier using : specifiers />
```

The specifier character may be repeated as many times as necessary to enable an element to contain that same specifier character.

```xfer
""This string contains a " character with impunity.""
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
    *   **Explicit:** `<"Alice said, "Boo!"">` or `<"A quote is a " character.">` or `<""XferLang supports <"strings">."">` (delimiter repetition)
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

**Dynamic Element**
*   **Specifier:** `|` (Pipe)
*   **Description:** Represents a value resolved via the dynamic source pipeline (dynamicSource PI mapping → custom handler → environment fallback). Content is plain text; no nested element evaluation occurs inside the pipes.
*   **Syntax:**
    *   **Compact:** `|USERNAME|`, `|DB_PASSWORD|`
    *   **Explicit:** `<|USERNAME|>` (rarely needed—delimiter lengthening works in compact form)
*   **Examples:**
    ```xfer
    </ Compact syntax />
    username |USER|
    password |DB_PASSWORD|

    </ Within interpolated strings />
    message 'Hello <|USER|>!'

    </ Delimiter lengthening for a literal pipe />
    literalPipe ||contains | literally||
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

It is a good practice to use explicit syntax (`<' ... '>`) when you are unsure whether interpolated values may contain single quotes.

**Dereference Element**
* **Specifier:** `_` leading underscores (empty closing delimiter)
* **Description:** Replaces the dereference token with a previously bound value (`let` or script `let`).
* **Syntax:** `_name`, `__name` (multiple underscores allowed to disambiguate, must match closing count in explicit form)
* **In Interpolated Text:** Use explicit form: `'Hello <_name_>'`.
* **Examples:**
    ```xfer
    <! let host "localhost" !>
    { apiHost _host message 'Host: <_host_>' }
    ```

Resolution happens as early as possible; unresolved references may be resolved in a later pass if a subsequent `let` appears earlier in structural order.

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
numbers [1 2 3 4 5]             </ All integer elements />
names ["Alice" "Bob" "Charlie"] </ All string elements />
decimals [*12.3 *45.6 *78.9]    </ All decimal elements />
booleans [~true ~false ~true]   </ All Boolean elements />
chars [\$41 \$42 \$43]          </ All character elements />
error [#42 &99]                 </ Error: Mixed types (integer and long) />
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
profile{name"Bob"role"admin"verified~true}
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
port 8080    </ Development server port />
```

### Dynamic

Dynamic elements represent values that are resolved at parse-time through the dynamic source pipeline (dynamicSource PI mapping → registered handler → environment fallback). The interior is an opaque text key; nested element syntax is not interpreted.

**Syntax:** `|key|` (explicit `<|key|>` form optional)

**Examples:**
```xfer
</ Environment variable substitution />
username |USER|
password |DB_PASSWORD|

</ Within interpolated strings />
greeting 'Hello <|USER|>!'
message 'Server running on port <|PORT|>'
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
- Content is raw text only; no nested parsing inside `|...|`
- Use interpolated text when you need to mix dynamic content with structured element outputs

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

## Getting Started with .NET

The XferLang .NET library provides a robust implementation for parsing, generating, and working with XferLang documents programmatically.

### Installation

Install the [NuGet package](https://www.nuget.org/packages/ParksComputing.Xfer.Lang) in your .NET project:

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

#### Tag PI

The `tag` PI attaches free‑form classification metadata to the immediately following element. Multiple `tag` PIs can be stacked; tags are preserved in element metadata but have no built‑in semantic effect.

```xfer
<! tag "experimental" !>
<! tag "search-index" !>
feature { enabled ~true }
```

#### Defined PI

The `defined` PI evaluates whether its value element yields a meaningful value (non‑null / non‑empty). It is commonly used in conjunction with `if` when you need pure existence tests.

```xfer
<! let debug ~false !>
<! defined _debug !>               </ true: binding exists even if value is false />
<! if defined _debug !> { note 'debug binding exists' }

<! dynamicSource { optFlag env "OPTIONAL_FLAG" } !>
<! if defined <|optFlag|> !> { note 'OPTIONAL_FLAG present' }
```

Evaluation occurs during PI processing; the PI itself is suppressed from serialization.

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

## Writing Custom Processing Instructions

You can add new Processing Instruction (PI) keywords without modifying the core parser by registering a factory that creates a `ProcessingInstruction` subclass. This lets you introduce custom metadata, validation passes, conditional logic, or side‑effects at parse time.

### When to Create a PI
Create a PI when you need to:
* Run setup logic before the next element is parsed (`ProcessingInstructionHandler`).
* Optionally suppress or transform the immediately following element (`ElementHandler`).
* Carry operational intent that should not appear in serialized output (set `SuppressSerialization = true`).

Do NOT create a PI just to change how `|dynamic|` values resolve—use a custom `IDynamicSourceResolver` for that (see “Dynamic Elements and Source Resolution”).

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

The XferLang solution contains several projects:

- **ParksComputing.Xfer.Lang** - Main library
- **ParksComputing.Xfer.Lang.Tests** - Unit tests and integration tests
- **xferc** - Command-line tools and REPL
- **XferService** - Example web service implementation (optional)

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

The formal Backus–Naur form (BNF) grammar for XferLang can be found in the XferLang GitHub repository: [xfer.bnf](https://github.com/paulmooreparks/Xfer/blob/master/xfer.bnf).
