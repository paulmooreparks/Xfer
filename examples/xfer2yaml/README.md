# Xfer ↔ YAML (GitHub Actions example)

This example shows how to represent a GitHub Actions workflow YAML in XferLang, and sketches a path for tooling to convert between the two.

## Why manage YAML with XferLang?

- Explicit typing and unambiguous syntax
- Structural interpolation and bindings (avoid copy-paste in large workflows)
- Conditional inclusion (e.g., feature flags) with `<! if ... !>`
- Dynamic values via `|KEY|` (environment/file/const resolvers)
- Semantic formatting and linting (future: `xferc fmt`, `xferc lint`)

## Proposed tool surface

- CLI: `xferc yaml2xfer <in.yml> -o <out.xfer>` and `xferc xfer2yaml <in.xfer> -o <out.yml>`
- API: `YamlConvert.Serialize(Element)` and `YamlConvert.Deserialize(string)` (built on YamlDotNet)
- Options:
  - Preserve ordering for readability (stable key ordering by known schemas)
  - Style preferences (compact/explicit)
  - Policy flags to disable dynamic/file resolution during materialization

## Files

- `dotnet.yml` – source GitHub Actions workflow
- `dotnet.xfer` – equivalent XferLang representation

Round-trip is intended to be lossless for data; comments/anchors would require extra mapping conventions.
