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
