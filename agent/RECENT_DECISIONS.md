# Recent Architectural Decisions

## Character Assignments (Latest Update)
- **Query Elements**: Changed from `$` to `` ` `` (backtick) - `$` reserved for hex numbers
- **Reference Elements**: Changed from `%` to `;` (semicolon) - `%` reserved for binary numbers
- **Processing Instructions**: Remain `<!` `!>` 
- **Tag Processing Instructions**: `<!tag "name"!>` implemented and working

## Parser Fixes Applied
- **Multiple PI Application Bug**: Fixed issue where multiple processing instructions targeting the same element only applied the last one
- **Root Cause**: Shared `_pendingPIs` instance variable caused call stack interference
- **Solution**: Each parsing method now uses local pending PI lists instead of shared state
- **Status**: All 170 tests passing, fix verified with complex scenarios

## Script Processing Instructions (In Progress)
- **Goal**: Create scripting language within `<!script (...)!>` PI
- **Approach**: Functional style mapping to .NET methods - `let x get_property (:obj:)`
- **Syntax**: Using reference elements `(:name:)` for variable references
- **Method Calls**: Considering new element type for object.method() operations

## Reference System Evolution
- **Current**: Backtick (`) for reference elements
- **Usage**: Inside script PIs for variable references: `(:variableName:)`
- **Method Chaining**: Exploring functional approach: `get_baz (get_bar (:foo:))`
- **Goal**: Direct mapping to .NET reflection without external scripting engines

## Design Philosophy Reinforced
- **One Language**: Avoid mixing JavaScript or other external DSLs
- **Minimalistic Extensions**: Keep scripting/pathing features simple and XferLang-native
- **Element-Based**: All new features should fit within existing element architecture
- **Functional Style**: Method calls as functions with object as first parameter

## Next Priorities
1. Design method call element type for object.method() operations
2. Implement script PI parsing and evaluation
3. Establish variable scoping and reference resolution
4. Create comprehensive examples and documentation
