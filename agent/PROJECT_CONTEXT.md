# XferLang Project Context

## Project Overview
XferLang is a data serialization and configuration language with advanced processing instruction capabilities, designed to be more expressive and powerful than JSON/XML while maintaining readability.

## Key Concepts
- **Elements**: Core data types (strings, numbers, objects, arrays, etc.)
- **Processing Instructions (PIs)**: Special directives that modify or enhance elements
- **Delimiters**: Characters that define element boundaries and types
- **XferPath**: Query/navigation system for accessing elements

## Current Character Assignments
- **Query Elements**: `` ` `` (backtick) - for queries/operations
- **Reference Elements**: `;` (semicolon) - for referencing other elements
- **Processing Instructions**: `<!` `!>` - for directives and commands
- **Comments**: `</` `/>` - for documentation
- **Dollar ($)**: Reserved for hex numbers
- **Percent (%)**: Reserved for binary numbers

## Active Development Areas
1. **Tag Processing Instructions**: `<!tag "name"!>` - assigns tags to elements
2. **Script Processing Instructions**: `<!script (...)!>` - embedded scripting language
3. **Reference System**: Using backticks for object references and method calls
4. **Method Call Elements**: New element type for object.method() style operations

## Design Philosophy
- One unified language (avoid mixing with JavaScript/other languages)
- Minimalistic scripting/pathing extensions
- Direct mapping to .NET methods and properties
- Functional programming style with method chaining
- Element-based approach consistent with XferLang core

## Project Structure
- `ParksComputing.Xfer.Lang/` - Core library
- `ParksComputing.Xfer.Lang.Tests/` - Unit tests
- `examples/` - Various demonstration projects
- `docs/` - Documentation and specifications
- `docs/whitepapers/` - Design documents, ideas, and architectural whitepapers
- `agent/` - AI assistant context and instruction files
