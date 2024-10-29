# Xfer

This project is still in its infancy and is quite experimental. As it becomes a bit more concrete I'll add more 
details here. Essentially, it's a replacement for Json that provides stricter typing, support for comments, and 
more flexible syntax.

```xfer
<! This is a sample Xfer document. !>
<@ <: version <"1.0.0"> :> @>

<"Hello, "Xfer"!">
<" Xfer > Json ">
<" Json < Xfer ">

<! An example of embedding a string element and a comment inside an Xfer string. !>
<""<"Xfer"> is an Xfer string element <!and this is an Xfer comment!>."">

<!! A comment <! inside another comment !> !!>

<{
    <: key <"value"> :>
    <: Orange <"New Black"> :>
}>
```
