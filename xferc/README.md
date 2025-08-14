# xferc

A consolidated CLI for working with XferLang documents. This file captures a roadmap of useful subcommands and features to guide development.

## Roadmap (proposed subcommands)

- Core authoring
  - parse: syntax check; print AST/element tree
  - fmt: canonical formatting; optional key-sorting; style preferences
  - lint: style/best‑practice checks (duplicate keys, mixed‑type arrays, naming conventions)
  - diff: semantic diff of two Xfer docs (key/value aware; ignore formatting noise)
  - patch: apply semantic patch/merge (support 3‑way with conflict markers)

- Validation and safety
  - validate: structural checks + custom rule packs; proper exit codes
  - schema-validate: check against Xfer schema(s) or JSON Schema (when converting JSON)
  - policy: enforce safe mode (disable file/env; disallow unknown PIs/operators)
  - audit: list dynamic/file usages, secrets‑like keys, unresolved references

- Conditional/dynamic tooling
  - resolve: materialize with PIs executed; dereferences and |dynamic| resolved
    - flags: --env-file, --var, --no-env, --no-file, --safe
  - dryrun: simulate resolution; show which values would be substituted
  - explain-if: trace why an if/defined evaluated true/false (operator trace)
  - bindings: list/inspect current let/script bindings; detect cycles/unresolved

- Query and edit (jq-like)
  - get/set: path-based read/update (e.g., object.key[2])
  - select/find: filter by key/value/tag/id predicates
  - grep: search keys/values/ids with regex
  - slice: extract subtrees into new documents

- IDs, tags, metadata
  - ids: list/validate uniqueness; auto-generate; rename
  - tags: list/filter/retag; tag counts
  - doc: read/write the document PI (version, author, tags, etc.)

- Generation and scaffolding
  - init/new: scaffold from templates (profiles) with prompts
  - codegen: generate C#/TS classes from Xfer (or from a sample set)
  - schema-gen: infer JSON Schema or Xfer schema from examples

- Conversion expansion (plugins ok)
  - yaml2xfer / xfer2yaml
  - toml2xfer / xfer2toml
  - xml2xfer / xfer2xml

- Testing and quality
  - roundtrip: ensure xfer → model → xfer is lossless
  - bench: parse/serialize/resolve performance microbenchmarks
  - fuzz: minimal fuzzer for parser stability

- Docs and visualization
  - tree: pretty-print tree view with types/specifiers
  - docgen: render HTML/Markdown from Xfer via templates
  - graph: export binding/ID graph (Graphviz/dot)

- DevX and extensibility
  - repl: interactive shell (evaluate operators, try PIs, inspect results)
  - plugin: load/unload custom operators/PIs/dynamic resolvers from assemblies; list registered
  - watch: watch mode to re-run fmt/resolve/docgen on change
  - completion: install shell completions; config profiles

## Useful global flags

- --in/--out (default stdin/stdout), --format {xfer,json,yaml}
- --strict, --warnings-as-errors
- --style {compact, explicit, minimal}
- --no-pi, --no-dynamic, --safe
- --env-file PATH, --var KEY=VALUE, --plugin PATH
- --trace, --json (machine-readable output)

## Notes

- This list is intentionally ambitious—treat it as a living plan. Prioritize based on user needs.
- Prefer a modular architecture: core + optional plugins (converters, operators, PIs, resolvers).

## Lint command design

This section outlines a concrete plan to design and ship an `xferc lint` command that’s useful, fast, and reusable.

### Scope and outputs (contract)

- Input: 1+ Xfer files or stdin; optional config
- Output: diagnostics with RuleId, Severity, Message, Path/Location (row/col). Formats: human text (default) and JSON
- Exit codes: 0 when no diagnostics ≥ fail level; non‑zero otherwise (configurable fail level)

### Reuse vs. new code

- Reuse: Parser, XferDocument, Elements tree, existing ParseWarnings (unknown operator, unresolved refs)
- New: lightweight analyzer engine (rule registry, traversal helpers, diagnostics), concrete rules
- Placement:
  - Reusable analyzers + diagnostics types: new library/namespace (e.g., `ParksComputing.Xfer.Lang.Analysis`)
  - CLI-only: config loading/merging, output formatting, exit policy, globbing

### Rules (MVP)

- Arrays/collections
  - XFER001 MixedTypeArray: warn when an Array contains mixed element types
  - XFER002 EmptyCollection: optional style warning for empty arrays/objects
- References and PIs
  - XFER010 UnresolvedReference: surface parser warnings for unresolved `_bindings`
  - XFER011 UnknownConditionalOperator: surface `if` PI with unknown operator (treated as no‑op)
  - XFER012 UnusedBinding: `let`/`script`-defined names never dereferenced
- Dynamic/policy
  - XFER020 DisallowedDynamicSource: flag `|KEY|` when policy forbids env/file/custom
  - XFER021 UnresolvedDynamic: dynamic resolves to null/empty (configurable)
- Style/consistency
  - XFER030 IdentifierCaseConvention: enforce key/name regex (snake/camel/Pascal/regex)
  - XFER031 PreferStyle: enforce serialization style (compact/explicit/minimal)
  - XFER032 InterpolationSafety: suggest explicit interpolation when compact likely needs escapes
- Metadata/structure
  - XFER040 DocPiPlacement: document PI (if present) must be first
  - XFER041 DuplicateIds: duplicate `id` PIs
  - XFER042 TagPolicy: restrict/require certain tags on elements

#### Nice-to-haves (later)

- XFER050 NumericWidth: value exceeds int but uses `#` instead of `&` (or vice versa)
- XFER051 HomogeneousTuple: discourage highly heterogeneous tuples
- XFER052 RoundTrip: warn if serialization with current settings would change style beyond whitespace

### Configuration

- File: `.xferlint.json` (optionally `.xferlint.xfer` later)
- Schema:
  - `rules`: { "XFER001": "error|warn|off", ... }
  - `conventions`: { identifierCase: "snake|camel|Pascal|regex", regex: "..." }
  - `policy`: { allowEnv: true/false, allowFile: true/false, allowUnknownOperators: false }
  - `style`: { prefer: "compact|explicit|minimal" }
  - `failOn`: "error|warn" (exit threshold)
  - `include`/`exclude` globs
- CLI overrides: `--rule XFER001=off`, `--style explicit`, `--policy no-env`, `--fail-on warn`, `--format json`

### Suppressions (no parser changes needed)

- Element-scoped via tag PI:
  - `<! tag "lint:disable:XFER001" !> …element…`
  - `<! tag "lint:disable-next:XFER001" !>` (applies to next element)
  - `<! tag "lint:enable:XFER001" !>`
- File-scoped via document PI tags or CLI config

### Architecture and code layout

- Analysis library (`ParksComputing.Xfer.Lang.Analysis`)
  - Diagnostic: RuleId, Severity, Message, Location { File, Row, Column, Path }
  - LintRule base: Id, Title, DefaultSeverity, Evaluate(XferDocument, LintContext) => IEnumerable<Diagnostic>
  - RuleRegistry: register built-ins; enable/disable/override severity
  - LintContext: settings (policy, style, conventions), helper services, file info
  - Traversal helpers: walk elements, arrays/objects, collect bindings/derefs
  - Adapters: map Parser.ParseWarnings -> Diagnostics
- CLI (xferc)
  - Command: `lint`
  - Config, includes/excludes, parse files, run analyzer, aggregate diagnostics
  - Output formatters: human, JSON; exit code based on highest severity ≥ `failOn`

### Rule implementation with current API

- MixedTypeArray: traverse `ArrayElement`, compare child element types
- UnresolvedReference: read `XferDocument.Warnings` (if exposed) or scan `ReferenceElement` vs collected bindings from `let/script` PIs
- UnknownConditionalOperator: map parser warning to diagnostic
- UnusedBinding: collect names from `let/script` vs dereference usages
- Disallowed/Unresolved Dynamic: find `DynamicElement`; use `DefinedOperator` in a safe resolver context
- IdentifierCaseConvention: apply regex to object keys
- PreferStyle: heuristic style checks or optional in-memory serialize-and-compare
- DocPiPlacement: verify first PI if present
- DuplicateIds: scan id PIs into a set

### CLI UX sketch

```
xferc lint path/**/*.xfer --fail-on warn --policy no-env --rule XFER021=off --format json
# Human output:
path/file.xfer:12:5 XFER001 warning MixedTypeArray: Array contains [IntegerElement,StringElement]
```

### Implementation steps

1) Add Analysis project with Diagnostic, Rule base, Registry, Context
2) Implement adapters for Parser warnings
3) Implement MVP rules: 001, 010, 011, 012, 020, 030, 040
4) Add xferc `lint` command (config + output + exit policy)
5) Tag-based suppression support
6) Tests: per-rule unit tests + integration on samples; snapshot outputs

### Edge cases

- Dynamic resolution: default “safe dry‑run” (no file IO) unless enabled; support `--env-file/--var`
- Large files: single-pass traversal; avoid heavy reserialization
- Unwrapped roots: parser normalizes; still report row/col from parser
- Unknown `if` operators: treated as no-op; lint surfaces a warning

### Placement summary

- Rule engine + generic rules: Analysis library (reusable across CLI/editor)
- CLI concerns: live only in `xferc`
- Optional future: comment-aware suppressions would require parser support; until then, PI tags suffice
