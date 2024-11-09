﻿# Xfer

Xfer is a data-serialization language that is designed to be a more flexible and more strictly-typed alternative to JSON.

This project is still in its infancy and is quite experimental. As it becomes a bit more concrete I'll add more 
details here. Essentially, it's a replacement for Json that provides stricter typing, support for comments, and 
more flexible syntax.

```xfer
</ This is a sample Xfer document. It demonstrates the basic syntax and features of the Xfer language. />

<// Elements are formed by enclosing them in angle brackets with another inner marker to designate
the meaning of the element. For example the character pairs </ and /> are used to enclose comments. 
When, as is the case here, the marker is repeated, it is so that the element can contain an embedded 
element of the same type. In this case, the outer comment contains an embedded comment, so the marker 
character, '/', is repeated as many times as necessary to allow embedded elements to be properly 
distinguished. //>

</ The following element is a metadata element. It consists of key/value pairs, some of which have 
reserved keywords (version, message_id, and ttl). If a metadata element is provided, it must be 
the first non-comment element in an Xfer document. />

<! <: version <"1.0.0"> :> !>

<:: keyValuePair <:key <"value"> :> ::> 

</ The outermost element of an Xfer document is an implicit property-bag element. It may contain 
elements of any type. />

</ A string element is a sequence of characters which are rendered literally, as they appear. />
<"Hello, Xfer!">

</ An evaluated element evaluates its embedded elements. The following will render as I ❤ Xfer. />
<_ I <\$2764\> Xfer _>

</ An example of embedding a string element and a comment inside an Xfer string. />
<""
<"Xfer"> is an Xfer string element </and this is an Xfer comment/>.
"">

</// A comment with <// a comment </ inside another comment /> //>. ///>

<"A string may contain character elements<\$20\>inside of it, which will not be parsed.">
<_When character elements are inside an evaluated<\$20\>element, they are parsed_>
<__An eval element may contain <_another eval_> if its markers are repeated__>
<_Other elements like strings (<"Hello, World!">), numbers (<#123#>, <&456&>), and dates (<@1976-04-07@>) 
may also be embedded in eval elements, and their evaluated results will become part of the rendered value._>

<{
    <: string <"value"> :> </ String marker is " />
    <: character <\65\> :> </ Character marker is \ />
    <: boolean <~true~> :> </ Boolean marker is ~ />
    <: integer <#42#> :> </ Integer marker is # />
    <: long <&5000000000&> :> </ Long marker is & />
    <: hexLongInteger <&$BAADF00D&> :> </ Hex numeric values are preceded by $ />
    <: binaryInteger <# %10101010 #> :> </ Binary numeric values are preceded by % />
    <: double <^3.1415926535^> :> </ Double marker is ^ />
    <: decimal <*123.45*> :> </ Decimal marker is * />
    <: date <@2019-01-01@> :> </ Date marker is @ />
    <: placeholder <_<|USERPROFILE|>_> :> </ Placeholder marker is | />
    <: evalString <_Inner elements <"are evaluated"> <#1#> at a time and<\$20\>rendered<\$20\><__as<\$20\>is__>._> :> </ Evaluated element marker is _ />
    <:: keyValuePair <:key <"value"> :> ::> </ Key-value pair marker is : />
    <: 
        array <[
            <#1#> 
            <#2#> 
            <#3#>
        ]> 
    :> </ Array markers are [ ] />
    <: 
        object <{ 
            <:key <"value">:> 
            <: boolean <~false~> :>
        }> </ Object markers are { } />
    :> 
    <: 
        propertyBag <(
            <"value">
            <#123#>
            <~true~>
            <@2019-01-01@>
        )> </ Property bag markers are ( ) />
    :>
    
}>

<"
/* Now it's possible to embed code with less worry */
namespace HelloWorldApp;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("Hello, Xfer!");
    }
}
">
```
