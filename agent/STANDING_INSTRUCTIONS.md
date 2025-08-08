# Standing Instructions for XferLang Development

## General Guidelines
1. **Always check current file contents** before making edits - the user frequently makes manual changes between requests
2. **Use replace_string_in_file with 3-5 lines of context** to ensure unambiguous edits
3. **Maintain consistency** with XferLang's element-based design philosophy
4. **Avoid introducing external dependencies** unless specifically requested
5. **Run tests after significant changes** to ensure no regressions

## Code Editing Rules
- When modifying parser code, be aware of shared state issues and call stack interference
- Processing Instructions should follow the established pattern in existing PIs
- New element types should inherit from appropriate base classes
- Always consider both parsing and serialization when adding new elements

## Testing Requirements
- Run full test suite (`dotnet test`) after parser changes
- Create debug applications for complex scenarios when needed
- All 170 tests should pass before considering changes complete

## Documentation Updates
- Update PROJECT_CONTEXT.md when new concepts are introduced
- Update this file when new standing instructions are established
- Maintain character assignment documentation when delimiters change
- Create new design documents in `docs/whitepapers/` for ideas, designs, and architectural decisions

## Communication Protocol
- When the user says "remember this" or "standing instruction", update these files
- Ask for clarification on design decisions that affect core architecture
- Provide multiple implementation options when the approach isn't clear
- Focus on the user's current file/selection when responding to requests

## File Management
- Clean up temporary debug files after use
- Use absolute paths for all file operations
- Create proper project structures for new demo applications
- **Always create demo projects under the `examples/` directory** with proper .csproj files and README documentation

## Meta Instruction
**IMPORTANT**: When the user explicitly states something should be remembered across sessions or gives a standing instruction, immediately update either PROJECT_CONTEXT.md or this file as appropriate. Confirm the update with the user.
