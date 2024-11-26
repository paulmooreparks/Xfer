# The Xfer Data-Interchange Format

_Welcome to everyone who came here from [Hacker News](https://news.ycombinator.com/item?id=42114543). Thank you so much for all the great input and discussion!_

Xfer is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management. 

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

Following is the equivalent Xfer document, using [compact syntax](#compact-syntax) and [implicit syntax](#implicit-syntax). Notice the explicit, rather than implicit, data types.

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
{name"Alice"age 30 isMember~true scores[*85 *90 *78.5]profile{email"alice@example.com"joinedDate@2023-01-15T20:00:00}}
```

## Basic Syntax

An Xfer document is composed of keywords and elements. An element typically begins and ends with angle brackets (< and >) unless using [compact syntax](#compact-syntax). The first character inside the angle brackets is the specifier character, which indicates the type of the element. The specifier character is followed by the element's content. The content varies based on the type of the element. Elements may be nested, and they may contain comments.

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

## Design Goals
* **Explicit Types**: All values are explicitly typed.
* **No Commas**: Xfer allows for objects and arrays to be defined without any separator characters between elements.
* **No Escaping**: Xfer does not require escaping of special characters in values. Instead, values are enclosed in unique delimiters that eliminate the need for escaping.
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
One of the design goals of Xfer is to eliminate the requirement to escape special characters. Enclosing data with unique delimiters already reduces the chances of a collision with the enclosed data, but in the event that a collision does occur, the specifier character can be repeated as many times as necessary to disambiguate the data.

```xfer
<"String elements may already contain "quotes" without any issues.">
<""To contain <"Xfer string delimiters">, repeat the string specifiers in the enclosing delimiters."">
<"""""Specifiers may be repeated as many times as necessary.""""">
```

This does not mean that escaping is not supported. There is a text element, the Evaluated Element (or eval element), that will evaluate any embedded elements and include their resulting values in the text value of the element. Using this feature and embedding character elements, it is possible to escape any character sequence.

```xfer
</ The following evaluated-text element will render as " I ❤︎ Xfer 😀 ". />
' I <\$2764\><\$fe0e\> Xfer <\$1F600\> '
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

While JSON builds on JavaScript's loose typing, Xfer is strictly typed. The basic types are string, character, integer, long integer, double, decimal, Boolean, and date/time. The type of an element is indicated by the specifier character in the opening delimter. The content of the element is then parsed according to the rules for that type.

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

## Xfer Element Syntax
An Xfer element may support up to three syntax variations: explicit syntax, compact syntax, and implicit syntax. All elements support explicit syntax, but the compact and implicit syntaxes are more concise.

### Explicit Syntax
When using explicit syntax, the element is enclosed in opening and closing delimiters composed of outer angle brackets (less-than, '<', and greater-than, '>') and an inner specifier character. The content of the element is enclosed in the opening and closing delimiters.

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

Compact syntax does away with the opening and closing angle brackets (less-than, '<', and greater-than, '>'). Instead, the specifier is followed by the content of the element. The content is terminated by the specifier, white space, or other special characters.

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

Integers may generally be used without any enclosing delimiters. They must be followed by whitespace or the closing delimiter of an enclosing object, array, or property bag.

```xfer
123
(456 789)
[101112 131415]
{key 98765}
```

As hinted above, keywords in a key/value pair may also be used without enclosing delimiters when they contain only upper-case or lower-case alphabetic characters (A-Z, a-z) or underscores ('_'). They must be followed by whitespace or the opening delimiter of another element.

```xfer
{
    name"Alice"
    age 30
    balance*123.45
}
```

### Why Three Different Element Syntaxes?

In the prototype design of Xfer, digraph pairs (such as <" "> or <{ }>) were used to delimit elements. However, I received feedback that this syntax was difficult to read and write, especially for large or complex documents. The compact syntax is more concise and easier to read and write, however the explicit syntax allows for nested elements and comments. In nearly all cases, the compact syntax is sufficient, but the explicit syntax is always available for cases where it is needed.

What this means, in a practical sense, is that you'll almost always use a mixture of compact and implicit syntax when you work with Xfer, only stepping up the syntax ladder when you need to disambiguate the contents of an element from the element's delimiters.

## Xfer Elements

This section describes the various Xfer element types.

### String Element

The string element is used to contain text data. The contents of the element will be stored as entered, including any embedded elements, white space, line breaks, etc.

* **Specifier:** " (Quotation Mark, U+0022)
* **Explicit Syntax:** Enclose the content in <" and "> delimiters.
* **Compact Syntax:** Enclose the content in opening and closing quotation marks.
* **Implicit Syntax:** Not supported

```xfer
</ String element />
<"Hello, World!"> </ Explicit syntax />
"Hello, World!" </ Compact syntax />
```

### Evaluated Text Element

The Evaluated Text element is used to evaluate embedded elements and include their resulting values in the text value of the element.

* **Specifier:** ' (Apostrophe, U+0027)
* **Explicit Syntax:** Enclose the content in <' and '> delimiters.
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

All elements embedded in an evaluated-text element which are intended to be evaluated must use [explicit syntax](#explicit-syntax).

```xfer
</ Evaluated (or eval) element. The element below will render as 
"Inner elements are evaluated 1 at a time and rendered as is." />

<'Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><''as<\$20\>is''>.'>

</ The following will render as " I ❤︎ Xfer 😀 ". />
' I <\$2764\><\$fe0e\> Xfer <\$1F600\> '

```

### Boolean Element

The Boolean element is used to represent a true or false value.

* **Specifier:** ~ (Tilde, U+007E)
* **Explicit Syntax:** Enclose the content in <~ and ~> delimiters.
* **Compact Syntax:** Follow the tilde specifier with the word 'true' or 'false'.
* **Implicit Syntax:** Not supported

```xfer
</ Boolean element />
<~true~> </ Explicit syntax />
~false </ Compact syntax />
```

### Integer Element

The Integer element is used to represent a 32-bit signed integer value.

* **Specifier:** # (Number Sign, U+0023)
* **Explicit Syntax:** Enclose the content in <# and #> delimiters.
* **Compact Syntax:** Follow the specifier with the integer value.
* **Implicit Syntax:** The integer value may be used without any enclosing delimiters. The value must be followed by whitespace or the closing delimiter of an enclosing object, array, or property bag.

```xfer
</ Integer element (default is 32 bits). Numeric values may be decimal (default), 
hexadecimal (preceded by $), or binary (preceded by %)./>
<#42#> </ Explicit syntax />
#42 </ Compact syntax />
42 </ Implicit syntax />
#$2A
#%00101010
$2A
%00101010
```

### Long Element

The Long element is used to represent a 64-bit signed integer value.

* **Specifier:** & (Ampersand, U+0026)
* **Explicit Syntax:** Enclose the content in <& and &> delimiters.
* **Compact Syntax:** Follow the specifier with the long integer value.
* **Implicit Syntax:** Not supported

```xfer
</ Long element (default is 64 bits) />
<&5000000000&>
&5000000000
&$BAADF00D
&%10101010
```

### Double Element

The Double element is used to represent a 64-bit floating-point value.

* **Specifier:** ^ (Caret, U+005E)
* **Explicit Syntax:** Enclose the content in <^ and ^> delimiters.
* **Compact Syntax:** Follow the specifier with the double value.
* **Implicit Syntax:** Not supported

```xfer
</ Double element />
<^3.1415926535^>
^3.1415926535
```

### Decimal Element

The Decimal element is used to represent a 128-bit decimal value.

* **Specifier:** * (Asterisk, U+002A)
* **Explicit Syntax:** Enclose the content in <* and *> delimiters.
* **Compact Syntax:** Follow the specifier with the decimal value.
* **Implicit Syntax:** Not supported

```xfer
</ Decimal element />
<*123.45*>
*123.45
```

### Character Element

The Character element is used to represent a single character.

* **Specifier:** \ (Reverse Solidus, or less formally, backslash, U+005C)
* **Explicit Syntax:** Enclose the content in <\ and \> delimiters.
* **Compact Syntax:** Follow the specifier with the character code.
* **Implicit Syntax:** Not supported

```xfer
</ Character element. All of the below examples render as 'A'. />
<\65\>
\65
\$41
\%01000001

</ Unicode codepoints may be used as well. The below example renders as '😀'. />
\$1F600

</ Certain pre-defined keywords exist for characters as well. />
<\nl\>
<\tab\>
```

### Date/Time Element

The Date/Time element is used to represent a date and time value.

* **Specifier:** @ (Commercial At, U+0040)
* **Explicit Syntax:** Enclose the content in <@ and @> delimiters.
* **Compact Syntax:** Follow the specifier with the date and time value in ISO 8601 format.
* **Implicit Syntax:** Not supported

```xfer
</ Date/time element />
<@2019-01-01T00:00:00@>
@2019-01-01T00:00:00
```

### Placeholder Element

The Placeholder element is used to represent a placeholder that will be replaced with a value at runtime.

* **Specifier:** | (Vertical Line, or less formally, pipe, U+007C)
* **Explicit Syntax:** Enclose the content in <| and |> delimiters.
* **Compact Syntax:** Follow the specifier with the placeholder name.
* **Implicit Syntax:** Not supported

```xfer
</ Placeholder element (almost always embedded in another element). />
'<|USERPROFILE|>'
#<|NUMBER_OF_PROCESSORS|>
```

### Keyword Element and Key/Value Pairs

The Keyword element is used to represent a keyword that is part of a key/value pair. The keyword may be used without any enclosing delimiters if it only consists of alphabetic characters in the ranges [a-z] or [A-Z] or the character '_'.

If a keyword needs to include whitespace or any other character besides [A-Z] or '_', it must be enclosed in keyword specifiers (':').

* **Specifier:** : (Colon, U+003A)
* **Explicit Syntax:** Enclose the content in <: and :> delimiters.
* **Compact Syntax:** Enclose the keyword in opening and closing colons.
* **Implicit Syntax:** The keyword may be used without any enclosing delimiters. The keyword must be followed by whitespace or the opening delimiter of another element.

```xfer
</ A key/value pair consists of a keyword followed by a value element. />
name <"Paul">
age <#$36#>
location <"Singapore">
:full name: "Paul Moore Parks"
```

If a keyword needs to include whitespace or any other character besides [A-Z] or '_', it must be enclosed in keyword specifiers (':').

```xfer
{
    :first name: "Alice"
    :last name: "Smith"
}
```

If, for some reason, a keyword needs to contain colon characters or explicit-syntax keyword delimiters, it must be enclosed in explicit delimiters.

```xfer
{
    </ Why you would do this, I don't know, but I've been in software long enough to see similar things. />
    <:first name::> "Alice"
    <:last name::> "Smith"
}
```

### Object Element

The Object element is used to represent a collection of key/value pairs.

* **Specifiers:** { (Left Curly Bracket, U+007B) and } (Right Curly Bracket, U+007D)
* **Explicit Syntax:** Enclose the content in <{ and }> delimiters.
* **Compact Syntax:** Enclose the object in opening and closing curly brackets.
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

* **Specifiers:** [ (Left Square Bracket, U+005B) and ] (Right Square Bracket, U+005D)
* **Explicit Syntax:** Enclose the content in <[ and ]> delimiters.
* **Compact Syntax:** Enclose the array in opening and closing square brackets.
* **Implicit Syntax:** Not supported

```xfer
</ Arrays may only hold a single type of element. />
[ 1 2 3 ]> </ Integer array />
[ "1" "2" "3" ]> </ String array />
```

### Property Bag Element

The Property Bag element is used to represent a collection of values of any type.

* **Specifiers:** ( (Left Parenthesis, U+0028) and ) (Right Parenthesis, U+0029)
* **Explicit Syntax:** Enclose the content in <( and )> delimiters.
* **Compact Syntax:** Enclose the property bag in opening and closing parentheses.
* **Implicit Syntax:** Not supported

```xfer
</ Property bags are a collection of values of any type, analogous to JSON arrays. />
(
    "value"
    123
    ~true
    @2019-01-01
)
```

