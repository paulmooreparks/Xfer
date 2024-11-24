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

## Work in Progress

### Tighter Syntax

I'm experimenting with a syntax that will cut down on the "noise" a bit. There are situations where an element may not need to be wrapped in a pair of digraphs but may instead be begun with a single character or wrapped in a pair of characters. If the full digraphs are clearer or are required due to the nature of the data they enclose, then those are still supported.

```xfer
{
    name "Alice" </ Text elements must start and end in quotes. />
    </ Other elements may simply begin with a specifier character. />
    age #30 </ # instead of <#30#> />
    isMember ~true </ ~ instead of <~true~> />

    </ And so on... />
    scores [
        *85
        *90
        *78.5
    ]

    profile {
        email "alice@example.com"
        joinedDate @2023-01-15T12:00:00 </ @ instead of <@ @> />
    }
}
```

String elements must still be enclosed in quotes, and strings may even contain quotes without escaping them.

```xfer
</ No digraphs required for text elements that are straightforward to parse. />
speaker "Alice"

</ If the string contains an embedded specifier character, then the surrounding specifiers may be repeated as necessary. />
statement1 ""A quote is a " character.""

</ Digraphs are required when the closing specifier would be ambiguous. />
statement2 <"Alice said, "What's up?""> 
```

The code is in a bit of a weird state where two styles of syntax are supported, and I think I like it that way for now. If I decide that the old syntax really isn't needed, I'll remove it. But for now, I'm going to keep it around.

## Basic Syntax

An Xfer document is composed of keywords and elements. An element typically begins and ends with angle brackets (< and >) unless using minified syntax (discussed later). The first character inside the angle brackets is the specifier character, which indicates the type of the element. The specifier character is followed by the element's content. The content varies based on the type of the element. Elements may be nested, and they may contain comments.

```xfer
</ Below is a string element />
<"Hello, World!">
```

## Design Goals
* **Explicit Types**: All values are explicitly typed.
* **Nullable Values**: Xfer supports null values only for types that are defined as nullable.
* **No Escaping**: Xfer does not require escaping of special characters.

## Features of Xfer
* [Nested Elements](#nested-elements)
* [Safer Embedding](#safer-embedding)
* [Comments](#comments)
* [Strict typing](#strict-typing)
* [Metadata](#metadata)
* [Placeholder substitution](#placeholder-substitution)

### Nested Elements
In Xfer, elements are delimited by angle brackets (< and >) and element-specific specifier characters  (such as !, /, #, ", and so on). Nesting of elements is accomplished by repeating the specifier character in the outer element as many times as necessary to disambiguate the inner elements.

```xfer
<//This is how a comment </can contain another comment/>, //>
<"""and a string can contain <""another string which <"contains another string">"">.""">
```

### Safer Embedding
One of the design goals of Xfer is to eliminate escaping of special characters. Enclosing data with unique paired digraphs already reduces the chances of a collision with the enclosed data, but in the event that a collision does occur, the specifier character can be repeated as many times as necessary to disambiguate the data.

```xfer
<"String elements may already contain "quotes" without any issues.">
<""To contain <"Xfer string digraphs">, repeat the string specifiers in the enclosing digraphs."">
<"""""Specifiers may be repeated as many times as necessary.""""">
```

### Comments

Xfer documents may contain comments that are ignored by the parser.

```xfer
</ This is a comment. />
```

Comments may also be embedded in other elements, including other comments.

```xfer
<//This is how a comment </can contain another comment/>, //>
```

### Strict Typing

While JSON builds on JavaScript's loose typing, Xfer is strictly typed.

```xfer
</ String element />
<"Hello, World!">  

</ Boolean element />
<~true~>

</ Integer element (default is 32 bits). Numeric values may be decimal (default), 
hexadecimal (preceded by $), or binary (preceded by %)./>
<#42#>
<#$2A#>
<#%00101010#>

</ Long element (default is 64 bits) />
<&5000000000&>
<&$BAADF00D&>
<# %10101010 #>

</ Double element />
<^3.1415926535^>

</ Decimal element />
<*123.45*>

</ Character element. All of the below examples render as 'A'. />
<\65\>
<\$41\>
<\%01000001\>

</ Certain pre-defined keywords exist for characters as well. />
<\nl\>
<\tab\>

</ Date/time element />
<@2019-01-01T00:00:00@>

</ Evaluated (or eval) element. The element below will render as "Inner elements are evaluated 1 
at a time and rendered as is." />
<_Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><__as<\$20\>is__>._>

</ Placeholder element (almost always embedded in another element). />
<_<|USERPROFILE|>_>
<#<|NUMBER_OF_PROCESSORS|>#>

</ A key/value pair consists of a keyword followed by a value element. />
name <"Paul">
age <#$36#>
location <"Singapore">

</ Arrays may only hold a single type of element. />
<[ <#1#> <#2#> <#3#> ]> </ Integer array />
<[ <&1&> <&2&> <&3&> ]> </ Long array />

</ Objects consist of key/value pairs. />
object <{ 
    key <"value">
    boolean <~false~>
}>

</ Property bags are a collection of values of any type, so they are analogous to JSON arrays. />
<(
    <"value">
    <#123#>
    <~true~>
    <@2019-01-01@>
)>
```

### Metadata

Xfer documents can contain metadata that is not part of the data itself. This metadata can be 
used for a variety of purposes, such as defining the version of Xfer that the document conforms 
to and other information that may be useful to the parser or 
the consumer of the data.

```xfer
<@
    version <"1.0">
    message_id <"5D3208CB-77EC-4BC4-A256-97AD296BBEF7">
    ttl <#3600#>
    description <"This is a sample document.">
@>

```

### Placeholder Substitution

Xfer documents may contain placeholders that are replaced with values at runtime.

```xfer
message <"Hello, <|USER|>!">
```

