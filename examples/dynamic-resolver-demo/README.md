# Dynamic Resolver Demo

## Killer Feature: dynamicSource PI Override

You can override dynamic value resolution in your Xfer document using the `dynamicSource` processing instruction (PI).

### How it works
- The resolver uses its default logic (e.g., reverse, DB, etc.) unless a PI override is present.
- If a `dynamicSource` PI is present, you can specify per-key overrides:
    - `demo "reverse:!dlroW ,olleH"` (custom logic)
    - `password "env:MY_PASSWORD"` (environment variable)
    - `greeting "This is a hard-coded greeting."` (hard-coded value)
- The PI can be placed at the top of your Xfer document.
- If a key is not present in the PI, the resolver falls back to its default logic.

### Example
```xfer
<! dynamicSource {
    demo "reverse:!dlroW ,olleH"
    greeting "env:GREETING_MSG"
    password "hardcoded-demo-password"
} !>
message {
    text '<|demo|>'
    greeting '<|greeting|>'
    password '<|password|>'
}
```

### Why this matters
- Enables flexible, testable, and environment-specific configuration.
- Supports secrets, test data, and runtime overrides without code changes.

**This is a killer feature for XferLang extensibility and integration!**
