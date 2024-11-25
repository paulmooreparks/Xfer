# <"Hello, Xfer!">

_Welcome to everyone who came here from [Hacker News](https://news.ycombinator.com/item?id=42114543). Thank you so much for all the great input and discussion!_

Xfer is a data interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management. 

This project is still in its infancy and is quite experimental, even exploratory. The code you'll find in this repository is also experimental. So far, I've built an [object model](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer/Models/Elements), a [parser](https://github.com/paulmooreparks/Xfer/blob/master/ParksComputing.Xfer/Services/Parser.cs), and a [serialization/deserialization class](https://github.com/paulmooreparks/Xfer/blob/master/ParksComputing.Xfer/XferConverter.cs) as part of my [.NET Xfer Library](https://github.com/paulmooreparks/Xfer/tree/master/ParksComputing.Xfer), but at the moment this code is completely not ready for prime time. It's not even thread safe yet! About once a week I'll completely refactor everything, so don't get terribly attached to anything you see here. However, if you do like some of the ideas, please [let me know](mailto:paul@parkscomputing.com). I'm always open to feedback.

That said, I do plan to make the code professional-grade in the future, and I want to add implementations in other languages (Java, Rust, C++, JavaScript, and TypeScript are on my list). If you want to contribute, please [let me know](mailto:paul@parkscomputing.com). I'd love to have your help.

## Xfer and JSON Compared

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

Following is the equivalent Xfer document, using minimized syntax. Notice the explicit, rather than implicit, data types.

```xfer
{
    name "Alice"
    age 30
    isMember ~true
    scores [*85 *90 *78.5]
    profile {
        email "alice@example.com"
        joinedDate @2023-01-15T12:00:00
    } 
}
```

Here is the same Xfer document with all unnecessary whitespace removed.

```xfer
{name"Alice"age 30 isMember~true scores<[*85 *90 *78.5]>profile{email"alice@example.com"joinedDate@2023-01-15T20:00:00}}
```

## Basic Syntax

An Xfer document is composed of keywords and elements. An element typically begins and ends with angle brackets (< and >) unless using minimized syntax. The first character inside the angle brackets is the specifier character, which indicates the type of the element. The specifier character is followed by the element's content. The content varies based on the type of the element. Elements may be nested, and they may contain comments.

```xfer
</ This is a comment, and below is a string element />
<"Hello, World!">
```

A string element may also be enclosed in just double quotes if there is no chance that the string will contain character sequences that make the closing of the element ambiguous. The enclosing quotes may be repeated as many times as necessary to disambiguate the string element.

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

For non-string element types, only the leading specifier is necessary for almost all cases. For integers, even this may be omitted if the value is positive.

```xfer
42 </ Integer element />
#-42 </ Integer element />
&9223372036854775807 </ Long element />
~true </ Boolean element/>
```

The main exception to this rule is the comment element. Comments always require angle brackets and one or more slash ('/') characters.

```xfer
</ This is a comment. />
<// Comments may enclose </other comments/> if the enclosing specifiers are repeated. //>
```

### Why Two Different Element Syntaxes?

In the prototype design of Xfer, only digraph pairs were used to delimit elements. However, I received feedback that this syntax was difficult to read and write, especially for complex documents. The minimized syntax is more concise and easier to read and write, but it is less flexible than the digraph syntax. The digraph syntax allows for nested elements and comments, while the minimized syntax does not. In nearly all cases, though, the minimized syntax is sufficient, but the digraph syntax is always available for cases where it is needed.

## Design Goals
* **Explicit Types**: All values are explicitly typed.
* **No Commas**: Xfer allows for objects and arrays to be defined without any separator characters between elements.
* **No Escaping**: Xfer does not require escaping of special characters in values. Instead, values are enclosed in unique paired digraphs that eliminate the need for escaping.
* **Nullable Values**: Xfer supports null values only for types that are defined as nullable.

## Features of Xfer
* [Nested Elements](#nested-elements)
* [Safer Embedding](#safer-embedding)
* [Comments](#comments)
* [Strict typing](#strict-typing)
* [Metadata](#metadata)
* [Placeholder substitution](#placeholder-substitution)

### Nested Elements
In Xfer, elements are delimited by angle brackets (< and >) and element-specific specifier characters  (such as !, /, #, ", and so on). Nesting of elements is accomplished by repeating the specifier character in the enclosing delimiters as many times as necessary to disambiguate the inner elements.

```xfer
<//This is how a comment </can contain another comment/>, //>
"""and a string can contain <""another string which <"contains another string">"">."""
```

### Safer Embedding
One of the design goals of Xfer is to eliminate the requirement to escape special characters. Enclosing data with unique paired digraphs already reduces the chances of a collision with the enclosed data, but in the event that a collision does occur, the specifier character can be repeated as many times as necessary to disambiguate the data.

```xfer
<"String elements may already contain "quotes" without any issues.">
<""To contain <"Xfer string digraphs">, repeat the string specifiers in the enclosing digraphs."">
<"""""Specifiers may be repeated as many times as necessary.""""">
```

This does not mean that escaping is not supported. There is a text element, the Evaluated Element (or eval element), that will evaluate any embedded elements and include their resulting values in the text value of the element. Using this feature and embedding character elements, it is possible to escape any character sequence.

```xfer
</ The following evaluated-text element will render as " I ❤︎ Xfer 😀 ". />
` I <\$2764\><\$fe0e\> Xfer <\$1F600\> `
```

Compare this to the standard string element, where the contents are not evaluated but rather rendered verbatim.

```xfer
</ The following string element will render as " I <\$2764\><\$fe0e\> Xfer <\$1F600\> ". />
" I <\$2764\><\$fe0e\> Xfer <\$1F600\> "
```

### Comments

Xfer documents may contain comments that are ignored by the parser.

```xfer
</ This is a comment. />
```

Comments may also be embedded in other elements, including other comments.

```xfer
<//This is how a comment </can contain another comment/>, //>
</// This nesting can <// go on </ and on/> //> and on. ///>
```

### Strict Typing

While JSON builds on JavaScript's loose typing, Xfer is strictly typed. The basic types are string, character, integer, long integer, double, decimal, boolean, and date/time. The type of an element is indicated by the specifier character in the opening delimter. The content of the element is then parsed according to the rules for that type.

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
@2019-01-01T00:00:00

```

### Metadata

Xfer documents can contain metadata that is not part of the data itself. This metadata can be 
used for a variety of purposes, such as defining the version of Xfer that the document conforms 
to and other information that may be useful to the parser or 
the consumer of the data.

```xfer
<!
    version "1.0.0"
    message_id "5D3208CB-77EC-4BC4-A256-97AD296BBEF7"
    ttl 3600
    description "This is a sample document."
!>
```

### Placeholder Substitution

Xfer documents may contain placeholders that are replaced with values at runtime.

```xfer
message <"Hello, <|USER|>!">
```

## Xfer Elements

```xfer
</ String element />
<"Hello, World!"> </ Digraph syntax />
"Hello, World!" </ Minimized syntax />

</ Boolean element />
<~true~> </ Digraph syntax />
~false </ Minimized syntax />

</ Integer element (default is 32 bits). Numeric values may be decimal (default), 
hexadecimal (preceded by $), or binary (preceded by %)./>
<#42#>
42
#$2A
#%00101010

</ Long element (default is 64 bits) />
<&5000000000&>
&5000000000
&$BAADF00D
&%10101010

</ Double element />
<^3.1415926535^>
^3.1415926535

</ Decimal element />
<*123.45*>
*123.45

</ Character element. All of the below examples render as 'A'. />
<\65\>
\65
\$41
\%01000001
\$1F600

</ Certain pre-defined keywords exist for characters as well. />
<\nl\>
<\tab\>

</ Date/time element />
<@2019-01-01T00:00:00@>
@2019-01-01T00:00:00

</ Evaluated (or eval) element. The element below will render as "Inner elements are evaluated 1 
at a time and rendered as is." />
<`Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><``as<\$20\>is``>.`>

</ Placeholder element (almost always embedded in another element). />
`<|USERPROFILE|>`
#<|NUMBER_OF_PROCESSORS|>

</ A key/value pair consists of a keyword followed by a value element. />
name <"Paul">
age <#$36#>
location <"Singapore">

</ Objects consist of key/value pairs. />
object { 
    key "value"
    boolean ~false
}

</ Arrays may only hold a single type of element. />
[ 1 2 3 ]> </ Integer array />
[ "1" "2" "3" ]> </ String array />

</ Property bags are a collection of values of any type, analogous to JSON arrays. />
(
    "value"
    123
    ~true
    @2019-01-01
)
```

