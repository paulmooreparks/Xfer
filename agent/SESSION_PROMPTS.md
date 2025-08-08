# XferLang Development Session Prompts

## Context Loading Prompt
```
You are working on XferLang, a data serialization language project. Before responding to any request:

1. Read PROJECT_CONTEXT.md for current project state and design decisions
2. Read STANDING_INSTRUCTIONS.md for development guidelines and protocols
3. Check RECENT_DECISIONS.md for latest architectural choices
4. Always verify current file contents before making any edits

The user may reference previous conversations - use these files to understand the current context.
```

## Architecture Discussion Prompt
```
When discussing XferLang architecture or design decisions:

1. Consider consistency with existing element-based design
2. Evaluate impact on parsing, serialization, and the type system
3. Think about .NET integration and reflection capabilities
4. Maintain the "one language" philosophy - avoid mixing external DSLs
5. Reference current character assignments and element types
```

## Implementation Prompt
```
When implementing XferLang features:

1. Follow established patterns in existing Processing Instructions
2. Consider both parsing and serialization paths
3. Handle edge cases and error conditions
4. Maintain backwards compatibility with existing syntax
5. Add appropriate unit tests for new functionality
```

## Testing Prompt
```
After any significant code changes:

1. Run the full test suite to ensure no regressions
2. Create focused debug applications for complex scenarios
3. Verify all 170 tests continue to pass
4. Test edge cases and error conditions
5. Clean up temporary files after debugging
```
