# XferLang Rust Prototype

This crate provides a minimal Rust implementation of the XferLang data
format. Only a few element types are supported (strings, integers,
booleans and lists) but the structure should be easy to extend. The
library exposes a small C API so that it can be called from other
languages.

## Building

```
cargo build
```

## Testing

```
cargo test
```

The `xfer_parse` function accepts a C string containing XferLang
content and returns a pointer to an `XferDocument`.  The document can
be converted to JSON using `xfer_document_to_json` and released with
`xfer_document_free`. Any returned strings must be released with
`xfer_string_free`.
