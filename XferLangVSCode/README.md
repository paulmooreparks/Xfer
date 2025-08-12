
# XferLang VS Code Extension

This extension provides syntax highlighting and basic editor help for [XferLang](https://xferlang.org/) files in Visual Studio Code, aligned with the current core language implementation.

## Features
* Processing instructions (document, dynamicSource, chardef, if, script, let, defined, id, tag)
* Explicit + compact forms for: strings, numbers (int/long/decimal/double), booleans, null, objects, arrays, tuples
* Signed numeric literals (+ / -) in implicit, compact, and explicit forms
* Dynamic elements (|key| / <| key |>) and references (leading underscores) highlighting
* Interpolated elements inside quoted strings
* Hexadecimal and binary numeric forms ($, %, #$, #%, &$, &%)
* Auto-closing & surrounding pairs for all specifiers and collection delimiters
* On-enter smart indent/outdent for {}, [], (), and their explicit <...> counterparts plus processing instructions
* Basic syntax resilience: nested comments & delimiter length variations

## Usage
Open any `.xfer` file to activate. Most constructs highlight as you type; increasing a delimiter run (adding more of the same specifier character) is treated uniformly.

## Installation
Install from the VS Code Marketplace (search: XferLang) or build locally:
1. Clone the repository
2. From `XferLangVSCode` run the build script (coming soon) or `vsce package`
3. Install the generated `.vsix` in VS Code

## Contributing
Pull requests and issues are welcome! See the repo for details.

## License
MIT License

## Changelog
### 0.15.3
Initial public version to coincide with XferLang pre-release v0.15:
* Updated grammar to current language spec (dynamic, defined, tag, signed numbers, references)
* Added indentation rules for collections and PIs
* Improved numeric patterns (explicit vs implicit, hex/binary prefixes)
* Added keyword PI highlighting

**Enjoy!**
