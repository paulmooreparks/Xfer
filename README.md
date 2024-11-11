# Xfer

Xfer is a data-serialization language that is designed to be a more flexible and more strictly-typed alternative to JSON.

This project is still in its infancy and is quite experimental. As it becomes a bit more concrete I'll add more 
details here. Essentially, it's a replacement for Json that provides stricter typing, support for comments, and 
more flexible syntax.

The code you'll find in this repository is also experimental. I'm building an object model and a serialization/deserialization library, 
but at the moment these are completely not ready for prime time. It's not even thread safe yet! At least once a week I'll completely 
refactor everything, so don't get terribly attached to anything you see here. However, if you do like some of the ideas, please let 
me know. I'm always open to feedback.

That said, I do plan to make the code professional-grade in the future, and I want to add implementations in other languages 
(Rust, JavaScript, and TypeScript are on my list).

## What Is the Purpose of Xfer?

As alluded to at the top of this document, the purpose is to provide an alternative to JSON that offers the following features 
(among others):

* Nested elements
* Strict typing
* Metadata
* Placeholder substitution
* Comments

### Nested Elements
I started working on Xfer after thinking of a rather goofy idea, albeit in a form that didn't make it into the current 
version of the language:

```xfer
<foo< Contents here can contain <bar< nested elements >bar> >foo>
```

Essentially, the idea was to create a form of element that can be modified to contain other elements. After playing with the 
idea for a while, I simplified it so that elements are delimited by angle brackets (< and >) and element-specific marker 
characters  (such as !, /, :, ", and so on). Containing nested elements is accomplished by repeating the marker character in 
the outer element as many times as necessary to disambiguate the inner elements.

```xfer
<//This is how a comment </can contain another comment/>, //>
<"""and a string can contain <""another string which <"contains another string">"">.""">
```

### Strict Typing

While JSON builds on JavaScript's loose typing, Xfer is strictly typed.

```xfer

</ String element />
<"Hello, World!">  

</ Boolean element />
<~true~>

</ Integer element (default is 32 bits). Numeric values may be decimal (default), hexadecimal (preceded by $), 
or binary (preceded by %)./>
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
<@2019-01-01T00:00@>

</ Evaluated (or eval) element />
</ The element below will render as "Inner elements are evaluated 1 at a time and rendered as is." />
<_Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><__as<\$20\>is__>._>

</ Placeholder element (almost always embedded in another element). />
<_<|USERPROFILE|>_>
<#<|NUMBER_OF_PROCESSORS|>#>

</ The <: :> element is a key/value pair, which consists of either an alphabetic keyword or a text 
element followed by a value element. />
<: name <"Paul"> :>
<: age <#$36#> :>
<: <"permanent location"> <"Singapore"> :>

</ Arrays may only hold a single type of element. />
<[ <#1#> <#2#> <#3#> ]> </ Integer array />
<[ <&1&> <&2&> <&3&> ]> </ Long array />

</ Objects consist of key/value pairs. />
<: 
    object <{ 
        <:key <"value">:> 
        <: boolean <~false~> :>
    }>
:> 

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
    <: version <"1.0"> :>
    <: message_id <"5D3208CB-77EC-4BC4-A256-97AD296BBEF7"> :>
    <: ttl <#3600#> :>
    <: description <"This is a sample document."> :>
@>

```

### Placeholder Substitution

Xfer documents may contain placeholders that are replaced with values at runtime.

```xfer
<: message <"Hello, <|USER|>!"> :>
```

### Comments

Xfer documents may contain comments that are ignored by the parser.

```xfer
</ This is a comment. />
```

Comments may also be embedded in other elements, including other comments.

```xfer
<//This is how a comment </can contain another comment/>, //>
<: key <"value"> </ and embedded comment /> :>
```

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
        "joinedDate": "2023-01-15T12:00:00Z"
    }
}
```

Following is the equivalent Xfer document. Notice the explicit, rather than implicit, data types.

```xfer
<{
    <: name <"Alice"> :>
    <: age <#30#> :>
    <: isMember <~true~> :>
    <: scores <[<*85*> <*90*> <*78.5*>]> :>
    <: profile <{
            <: email <"alice@example.com"> :>
            <: joinedDate <@2023-01-15T12:00:00@> :>
        }> 
    :>
}>
```

