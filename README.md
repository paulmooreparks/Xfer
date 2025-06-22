![XferLogo](logo/XferLang-sm.png)

# The XferLang Data-Interchange Format

<p>
  <a href="https://github.com/paulmooreparks/Xfer">
    <img alt="Xfer Version" src="https://img.shields.io/badge/Xfer-0.10.0-green">
  </a>
  <a href="https://github.com/paulmooreparks/Xfer">
    <img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/paulmooreparks/Xfer">
  </a>
  <a href="https://github.com/paulmooreparks/Xfer">
    <img alt="GitHub issues" src="https://img.shields.io/github/issues/paulmooreparks/Xfer">
  </a>
  <a href="https://opensource.org/licenses/MIT">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg">
  </a>
  <a href="https://www.nuget.org/packages/ParksComputing.Xfer.Lang">
    <img alt="NuGet" src="https://img.shields.io/nuget/v/ParksComputing.Xfer.Lang.svg">
  </a>
</p>

_Welcome to everyone who came here from [Hacker News](https://news.ycombinator.com/item?id=42114543). Thank you so much for all the great input and discussion!_

XferLang is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management. 

This project is still in its infancy and is quite experimental, even exploratory. The code you'll find in this repository is also experimental. So far, I've built an [object model](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer.Lang/Elements), a [parser](https://github.com/paulmooreparks/Xfer/blob/master/ParksComputing.Xfer.Lang/Services/Parser.cs), and a [serialization/deserialization class](https://github.com/paulmooreparks/Xfer/blob/master/ParksComputing.Xfer.Lang/XferConvert.cs) as part of my [.NET XferLang Library](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer.Lang), but at the moment this code is completely not ready for prime time. It can now be used safely from multiple threads. 

Please [let me know](mailto:paul@parkscomputing.com) what you think about the ideas you see here. I'm always open to feedback.

I am in the process of making the code professional-grade, and I want to add implementations in other languages (Rust, C++, Java, JavaScript, and TypeScript are on my list). If you want to contribute, please [let me know](mailto:paul@parkscomputing.com). I'd love to have your help.

## Design Goals
* **Explicit Types**: All values are explicitly typed.
* **No Commas**: XferLang allows for objects and arrays to be defined without any separator characters between elements.
* **No Escaping**: XferLang does not require escaping of special characters in values. Instead, values are enclosed in unique delimiters that eliminate the need for escaping.
* **Safer Embedding**: XferLang allows for embedding of elements without the need for escaping special characters. If a collision occurs, the specifier character can be repeated as many times as necessary to disambiguate the data.

## XferLang and JSON Compared

Here's a simple example of a JSON document:

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

Following is the equivalent XferLang document, using [compact syntax](#compact-syntax) and [implicit syntax](#implicit-syntax).

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

Here is the same XferLang document with all unnecessary whitespace removed.

```xfer
{name"Alice"age 30 isMember~true scores[*85*90*78.5]profile{email"alice@example.com"joinedDate@2023-05-05T20:00:00@}}
```

## XferLang Syntax

An XferLang document is composed of keywords and elements. When using [explicit syntax](#explicit-syntax), an element begins and ends with angle brackets (`<` and `>`). The first character inside the angle brackets is the specifier character, which indicates the type of the element. The specifier character is followed by the element's content. The content varies based on the type of the element. Elements may be nested, and they may contain comments.

```xfer
</ This is a comment, and below is a string element />
<"Hello, World!">
```

A string element may also be enclosed in just double quotes if there is no chance that the string will contain character sequences that make the closing of the element ambiguous ([compact syntax](#compact-syntax)). The enclosing quotes may be repeated as many times as necessary to disambiguate the string element.

```xfer
"Hello, World!"
""A quote is a " character.""
"""An empty string is represented by an empty pair of quotes ("")."""
""A string may contain <"another string">.""
```

In cases where the string contains a sequence that makes the closing ambiguous, the string element must be enclosed in angle brackets.

```xfer
<"Alice said, "Boo!"">
```

For non-string element types, only the leading specifier is necessary for almost all cases. For some elements like integers and keywords, even this may be omitted ([implicit syntax](#implicit-syntax)).

```xfer
42 </ Integer element />
-42 </ Integer element />
&9223372036854775807 </ Long element />
~true </ Boolean element/>
key "value" </ Key/value pair />
```

Comments are always enclosed in angle brackets and one or more slash (`/`) characters.

```xfer
</ This is a comment. />
<// Comments may enclose </other comments/> if the enclosing specifiers are repeated. //>
```

Due to the nature of the XferLang syntax, representing empty values requires explicit delimiters.

```xfer
emptyString <""> 
```

A null value is represented by the `?` specifier.

```xfer
nullValue ?
alsoNull <??>
```

## Features of XferLang
* [Nested Elements](#nested-elements)
* [Safer Embedding](#safer-embedding)
* [Comments](#comments)
* [Strict typing](#strict-typing)
* [Metadata](#metadata)
* [Placeholder substitution](#placeholder-substitution)

### Nested Elements
In XferLang, elements are delimited by angle brackets (`<` and `>`) and element-specific specifier characters  (such as `!`, `/`, `#`, `"`, and so on). Nesting of elements is accomplished by repeating the specifier character in the enclosing delimiters as many times as necessary to disambiguate the inner elements.

```xfer
<//This is how a comment </can contain another comment/>, //>
"""and a string can contain <""another string which <"contains another string">"">."""
```

### Safer Embedding
One of the design goals of XferLang is to eliminate the requirement to escape special characters. Enclosing data with unique delimiters already reduces the chances of a collision with the enclosed data, but in the event that a collision does occur, the specifier character can be repeated as many times as necessary to disambiguate the data.

```xfer
<"String elements may already contain "quotes" without any issues.">
<""To contain <"Xfer string delimiters">, repeat the string specifiers in the enclosing delimiters."">
<"""""Specifiers may be repeated as many times as necessary.""""">
```

This does not mean that escaping is not supported. There is a text element, the Evaluated Element (or eval element), that will evaluate any embedded elements and include their resulting values in the text value of the element. Using this feature and embedding character elements, it is possible to escape any character sequence.

```xfer
</ The following evaluated-text element will render as " I ❤︎ XferLang 😀 ". />
' I <\$2764\><\$fe0e\> XferLang <\$1F600\> '
```

Compare this to the standard string element, where the contents are not evaluated but rather rendered verbatim.

```xfer
</ The following string element will render as " I <\$2764\><\$fe0e\> XferLang <\$1F600\> ". />
" I <\$2764\><\$fe0e\> XferLang <\$1F600\> "
```

### Comments

XferLang documents may contain comments that are ignored by the parser.

```xfer
</ This is a comment. />
```

Comments may also be embedded in other elements, including other comments.

```xfer
<//This is how a comment </can contain another comment/>, //>
</// This nesting can <// go on </ and on/> //> and on. ///>
```

### Strict Typing

While JSON relies on JavaScript's type inference, XferLang requires explicit typing. The core types supported by XferLang are [string](#string-element), [character](#character-element), [integer](#integer-element), [long integer](#long-element), [double](#double-element), [decimal](#decimal-element), [Boolean](#boolean-element), and [date/time](#datetime-element). Each element's type is indicated by a specifier character in its opening delimter, and its content is then parsed according to the rules for that type.

```xfer
</ String element />
"Hello, World!"

</ Character element. All of the below examples render as 'A'. />
\65
\$1F600

</ Integer element />
42

</ Long element (default is 64 bits) />
&5000000000

</ Double element />
^3.1415926535

</ Decimal element />
*123.45

</ Boolean element />
~true
~false

</ Date/time element />
@2019-01-01T00:00:00@

```

### Metadata

XferLang documents can contain metadata that is not part of the data itself. This metadata can be used for a variety of purposes, such as defining the version of XferLang that the document conforms to and other information that may be useful to the parser or the consumer of the data.

```xfer
<!
    xfer "1.0.0"
    message_id "5D3208CB-77EC-4BC4-A256-97AD296BBEF7"
    ttl 3600
    description "This is a sample document."
!>
```

### Placeholder Substitution

XferLang documents may contain placeholders that are replaced with values at runtime.

```xfer
message 'Hello, <|USER|>!'
```

## XferLang Element Syntax
An XferLang element may support up to three syntax variations: explicit syntax, compact syntax, and implicit syntax. All elements support explicit syntax, but the compact and implicit syntaxes are more concise.

### Explicit Syntax
When using explicit syntax, the element is enclosed in opening and closing delimiters composed of outer angle brackets (less-than, `<`, and greater-than, `>`) and an inner specifier character. The content of the element is enclosed in the opening and closing delimiters.

```xfer
<"Hello, World!">
<#123#>
<[ <#1#> <#2#> <#3#> ]>
<{ <:key:> <#value> }>
```

The specifier may be repeated if required by the contents of the element.

```xfer
<""This string element contains <"another string element">."">
```

### Compact Syntax

Compact syntax does away with the opening and closing angle brackets. Instead, the specifier is followed by the content of the element. The content is terminated by the specifier, white space, or other special characters.

For text elements and collection elements, the element is enclosed in opening and closing specifiers. 

```xfer
"Hello, World!"
[ "one" "two" "three" ]
```

The specifier may be repeated if required by the contents of the element.

```xfer
""The poem "Casey at the Bat" is a baseball classic.""
```

For non-text elements, the element begins with the specifier and ends with white space or other special characters.

```xfer
*123.45
~true
\$20
```

### Implicit Syntax
When using implicit syntax, certain elements may be used without any enclosing delimiters if the type of the element can be inferred from the contents and the surrounding elements.

Integers generally do not require any enclosing delimiters. They must be followed by whitespace or the closing delimiter of an enclosing object, array, or tuple.

```xfer
123
(456 789)
[101112 131415]
```

Keywords in a key/value pair may also be used without enclosing delimiters when they contain only alphanumeric characters (A-Z, a-z, 0-9) or underscores (`_`). They must be followed by whitespace or the opening delimiter of another element.

```xfer
{
    name"Alice"
    age 30
    balance*123.45
}
```

### Why Three Different Element Syntaxes?

In the prototype design of XferLang, digraph pairs (such as `<" ">` or `<{ }>`) were used to delimit elements. However, I received feedback that this syntax was difficult to read and write, especially for large or complex documents. The compact syntax is more concise and easier to read and write, but the explicit syntax allows for nested elements and comments. In nearly all cases, the compact syntax is sufficient, but the explicit syntax is always available for cases where it is needed.

What this means, in a practical sense, is that you'll almost always use a mixture of implicit and compact syntax when you work with XferLang, only stepping up the syntax ladder when you need to disambiguate the contents of an element from the element's delimiters.

## XferLang Elements

This section describes the various XferLang element types.

### String Element

The String element is used to contain text data. The contents of the element will be stored as entered, including any embedded elements, white space, line breaks, etc.

* **Specifier:** `"` (Quotation Mark, U+0022)
* **Explicit Syntax:** Enclose the content in `<"` and `">` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing quotation marks.
* **Implicit Syntax:** Not supported

```xfer
</ String element />
<"Hello, World!"> </ Explicit syntax />
"Hello, World!" </ Compact syntax />
```

A String element may contain any UTF-8 character, including whitespace, line breaks, and special characters. If the string contains a sequence that would make the closing of the element ambiguous, the specifiers in the outer delimiter must be repeated as many times as necessary to disambiguate the delimiters and explicit syntax may be required. 

```xfer
</ The specifiers below are repeated to allow for the embedded quotes. />
""This string element contains "quoted" content.""

</ Explicit syntax is required to disambiguate the closing delimiter from the ending quote character. />
<"Alice said, "Boo!"">
```
A String element may also contain embedded elements, which will be rendered as entered.

```xfer
</ The following will render as " I <\$2764\><\$fe0e\> XferLang <\$1F600\> ". />
' I <\$2764\><\$fe0e\> XferLang <\$1F600\> '
```

### Evaluated Text Element

The Evaluated Text element is used to evaluate embedded elements and include their resulting values in the text value of the element.

* **Specifier:** `'` (Apostrophe, U+0027)
* **Explicit Syntax:** Enclose the content in `<'` and `'>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing apostrophes.
* **Implicit Syntax:** Not supported

Elements which may be embedded within an evaluated text element are as follows:

* string
* character
* integer
* long
* double
* decimal
* boolean
* date/time
* placeholder
* other evaluated text elements

An Evaluated Text element may contain the same text content as a String element, but it may also contain embedded elements.

All elements embedded in an evaluated-text element which are intended to be evaluated must use [explicit syntax](#explicit-syntax).

```xfer
</ Evaluated (or eval) element. The element below will render as 
"Inner elements are evaluated 1 at a time and rendered as is." />

<'Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><''as<\$20\>is''>.'>

</ The following will render as " I ❤︎ XferLang 😀 ". />
' I <\$2764\><\$fe0e\> XferLang <\$1F600\> '

```

### Boolean Element

The Boolean element is used to represent a true or false value.

* **Specifier:** `~` (Tilde, U+007E)
* **Explicit Syntax:** Enclose the content in `<~` and `~>` delimiters.
* **Compact Syntax:** Follow the tilde specifier with the content.
* **Implicit Syntax:** Not supported

```xfer
</ Boolean element />
<~true~> </ Explicit syntax />
~false </ Compact syntax />
```

### Integer Element

The Integer element is used to represent a 32-bit signed integer value.

* **Specifier:** `#` (Number Sign, U+0023)
* **Explicit Syntax:** Enclose the content in `<#` and `#>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** The integer value may be used without any enclosing delimiters. The value must be followed by whitespace, the opening delimiter of another element, or the closing delimiter of an enclosing object, array, or tuple.

Integer values are decimal by default, but they may also be hexadecimal when the value is preceded by `$` or binary when the value is preceded by `%`.

```xfer
</ Explicit syntax />
<#42#>
<#$2A#> </ Hexadecimal integer value />
<#%00101010#> </ Binary integer value />

</ Compact syntax />
#42
#$2A 
#%00101010 

</ Implicit syntax />
42
$2A
%00101010
```

### Long Element

The Long element is used to represent a 64-bit signed integer value.

* **Specifier:** `&` (Ampersand, U+0026)
* **Explicit Syntax:** Enclose the content in `<&` and `&>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** Not supported

Long values are decimal by default, but they may also be hexadecimal when the value is preceded by `$` or binary when the value is preceded by `%`.

```xfer
</ Long element (default is 64 bits) />
<&5000000000&>
&5000000000
&$BAADF00D
&%10101010
```

### Double Element

The Double element is used to represent a 64-bit floating-point value.

* **Specifier:** `^` (Caret, U+005E)
* **Explicit Syntax:** Enclose the content in `<^` and `^>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** Not supported

```xfer
</ Double element />
<^3.1415926535^>
^3.1415926535
```

### Decimal Element

The Decimal element is used to represent a 128-bit decimal value.

* **Specifier:** `*` (Asterisk, U+002A)
* **Explicit Syntax:** Enclose the content in `<*` and `*>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** Not supported

```xfer
</ Decimal element />
<*123.45*>
*123.45
```

### Character Element

The Character element is used to represent a character. These may be stand-alone elements, or they may be embedded in an [evaluated text element](#evaluated-text-element).

* **Specifier:** `\` (Reverse Solidus, or less formally, backslash, U+005C)
* **Explicit Syntax:** Enclose the content in `<\` and `\>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** Not supported

Character values are decimal by default, but they may also be hexadecimal when the value is preceded by `$` or binary when the value is preceded by `%`.

```xfer
</ Character element. All of the below examples render as 'A'. />
<\65\>
\65
\$41
\%01000001

</ Unicode codepoints may be used as well. The below example renders as '😀'. />
\$1F600
```

Certain pre-defined keywords exist for characters as well.

* `nul` - NUL (U+0000)
* `cr` - Carriage return (U+000D)
* `lf` - Line feed (U+000A)
* `nl` - New line (platform-specific)
* `tab` - Tab (U+0009)
* `vtab` - Vertical tab (U+000B)
* `bksp` - Backspace (U+0008)
* `ff` - Form feed (U+000C)
* `bel` - Bell (U+0007)
* `quote` - `"` Quote (U+0022)
* `apos` - `'` Apostrophe (U+0027)
* `backslash` - `\` Backslash (U+005C)
* `lt` - `<` Less than (U+003C)
* `gt` - `>` Greater than (U+003E)

```xfer
<\gt\> </ Renders as '>' />
<\tab\> </ Inserts a tab character />
```

### Date/Time Element

The Date/Time element is used to represent a date and time value. The value must be in ISO 8601 format.

* **Specifier:** `@` (Commercial At, U+0040)
* **Explicit Syntax:** Enclose the content in `<@` and `@>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing `@` characters.
* **Implicit Syntax:** Not supported

```xfer
</ Date/time element />
<@2019-01-01T00:00:00@>
@2019-01-01T00:00:00@
```

### Null Element

The Null element is used to represent a null value.

* **Specifier:** `?` (Question Mark, U+003F)
* **Explicit Syntax:** A pair of `<?` and `?>` delimiters with no content.
* **Compact Syntax:** A single `?` character.
* **Implicit Syntax:** Not supported

```xfer
nullValue ?
explicitNull <??>
```

### Placeholder Element

The Placeholder element is used to represent a placeholder that will be replaced with a value at runtime. In the current implementation, this is always an environment variable, but future implementations may support other types of placeholders.

* **Specifier:** `|` (Vertical Line, or less formally, pipe, U+007C)
* **Explicit Syntax:** Enclose the content in `<|` and `|>` delimiters.
* **Compact Syntax:** Follow the specifier with the content.
* **Implicit Syntax:** Not supported

```xfer
</ Placeholder element (almost always embedded in another element). />
'<|USERPROFILE|>'
#<|NUMBER_OF_PROCESSORS|>
```

### Keyword Element and Key/Value Pairs

The Keyword element is used to represent a keyword that is part of a key/value pair. The keyword may be used without any enclosing delimiters if it only consists of alphanumeric characters or the character '_'.

If a keyword needs to include whitespace or any other character besides [A-Z], [a-z], [0-9], or '_', it must be enclosed in keyword specifiers (':').

* **Specifier:** `=` (Equal Sign, U+003D)
* **Explicit Syntax:** Enclose the content in `<=` and `=>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing equal signs.
* **Implicit Syntax:** The keyword may be used without any enclosing delimiters if it only contains [A-Z], [a-z], [0-9], or '_' and it does not begin with [0-9]. The keyword must be followed by whitespace or the opening delimiter of another element.

```xfer
</ A key/value pair consists of a keyword followed by a value element. />
name "Paul"
age $36
location "Singapore"
```

If a keyword needs to include whitespace or any other character besides alphanumeric or '_', it must be enclosed in keyword specifiers (':').

```xfer
{
    :first name: "Alice"
    :last name: "Smith"
}
```

If, for some reason, a keyword contains character sequences that would make it impossible to parse correctly, it must be enclosed in explicit delimiters.

```xfer
{
    <:first name::> "Alice"
    <:last name::> "Smith"
}
```

### Object Element

The Object element is used to represent a collection of key/value pairs.

* **Specifiers:** `{` (Left Curly Bracket, U+007B) and `}` (Right Curly Bracket, U+007D)
* **Explicit Syntax:** Enclose the content in `<{` and `}>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing curly brackets.
* **Implicit Syntax:** Not supported

```xfer
</ Objects consist of key/value pairs. />
object { 
    key "value"
    boolean ~false
}
```

### Array Element

The Array element is used to represent a collection of elements of the same type.

* **Specifiers:** `[` (Left Square Bracket, U+005B) and `]` (Right Square Bracket, U+005D)
* **Explicit Syntax:** Enclose the content in `<[` and `]>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing square brackets.
* **Implicit Syntax:** Not supported

```xfer
</ Arrays may only hold a single type of element. />
[ 1 2 3 ]> </ Integer array />
[ "1" "2" "3" ]> </ String array />
```

### Tuple Element

The Tuple element is used to represent a collection of values of any type.

* **Specifiers:** `(` (Left Parenthesis, U+0028) and `)` (Right Parenthesis, U+0029)
* **Explicit Syntax:** Enclose the content in `<(` and `)>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing parentheses.
* **Implicit Syntax:** Not supported

```xfer
</ Tuples are a collection of values of any type, analogous to JSON arrays. />
(
    "value"
    123
    ~true
    @2019-01-01@
)
```

### Metadata Element

The Metadata element is used to represent metadata that is not part of the data itself. It contains key/value pairs that may be used for a variety of purposes, such as defining the version of XferLang that the document conforms to or other information that may be useful to the parser or the consumer of the data.

* **Specifiers:** `!` (Exclamation Mark, U+0021)
* **Explicit Syntax:** Enclose the content in `<!` and `!>` delimiters.
* **Compact Syntax:** Enclose the content in opening and closing exclamation marks.
* **Implicit Syntax:** Not supported

```xfer
<!
    xfer "1.0.0" 
    message_id "5D3208CB-77EC-4BC4-A256-97AD296BBEF7" 
    ttl 3600 
    description "This is a sample document." 
!>
```

The `xfer` keyword is reserved by the XferLang specification and indicates the version of XferLang to which the document conforms. Additional key/value pairs may be included, with their meanings defined by the document itself or its schema.

The metadata element may only appear at the beginning of a document before any other non-comment elements.

```xfer
</ This is a valid document />
!xfer "1.0.0"!
"Hello, World!"
</ Metadata may not appear after any non-comment elements. />
```

#### `xfer` Keyword

The `xfer` keyword is reserved by the XferLang specification and indicates the version of XferLang to which the document conforms. The value of the `xfer` keyword must be a string element.

```xfer
! xfer "1.0.0" !
```

### Comment Element

The Comment element is used to represent a comment that is not part of the data itself. This comment can be used for documenting the data or providing additional information that may be useful the consumer of the data. The contents and enclosing delimiters are ignored by the parser.

* **Specifiers:** `/` (Solidus, U+002F)
* **Explicit Syntax:** Enclose the content in `</` and `/>` delimiters.
* **Compact Syntax:** Not supported.
* **Implicit Syntax:** Not supported.

```xfer
</ This is a comment. />
<// A comment may safely contain </other comments/> when the outer specifiers are repeated. //>
```

## XferLang Document Structure

Comment elements may appear anywhere in an XferLang document and are ignored by the parser. No comments will appear in the parsed structure.

An XferLang document begins with a metadata element. If the element is not provided explicitly, an implicit metadata element is added to the document with a default version number assigned. A metadata element may not appear after any non-comment elements.

The root content element of an XferLang document is an implicit [tuple](#tuple-element) element. This element may contain any number of other elements.

In the document below, the string value with the content `Hello, World!` is the first element in the root tuple, the integer with the value `42` is the second element in the root tuple, and the array element containing three string values is the third element in the root tuple.

```xfer
!xfer "1.0.0"!
"Hello, World!"
42
["abc" "def" "ghi"]
```

## XferLang Object Model

_Coming soon..._

## XferLang Parser

_Coming soon..._

## Serialization


The [serialization/deserialization class](https://github.com/paulmooreparks/Xfer/blob/master/ParksComputing.Xfer.Lang/XferConvert.cs) makes use of the [object model](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer.Lang/Elements) to write object contents out to a stream or read object contents from a stream. The class is thread safe, and although it is not yet optimized for performance, it is already useful for demonstrating XferLang's capabilities.

## XferLang Grammar

The BNF grammar may be found [in the repository](xfer.bnf). It mirrors the current parser implementation and the description in this document.
