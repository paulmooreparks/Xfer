# XferLang Conditional Operator Suite

This whitepaper defines a coherent, extensible set of conditional / boolean operators for XferLang scripting & processing instructions. It complements existing operators `defined`, `eq`, `if`, and `gt`, and establishes naming + behavioral conventions for future additions.

## Design Goals
- Minimal core, logically orthogonal.
- Consistent argument arity and coercion rules.
- Predictable truthiness and comparison semantics (numeric/date coercion first, fallback to ordinal string compare).
- Short‑circuit evaluation for logical operators.
- Safe failure: invalid arg count or uncoercible types produce warnings (future) and return `false` (or `null` for value-returning operators) rather than throwing.

## Truthiness Rules
(Reused internally):
- `null` => false
- Empty string => false
- Numeric zero => false
- Empty collection => false
- Everything else => true

## Core Comparison Operators
| Name | Args | Semantics |
|------|------|-----------|
| `eq` | 2 | Equality (already implemented) |
| `ne` | 2 | Logical negation of `eq` |
| `gt` | 2 | Greater-than (already implemented) |
| `gte` | 2 | Greater-than or equal |
| `lt` | 2 | Less-than |
| `lte` | 2 | Less-than or equal |
| `between` | 3–4 | Value in [low, high] (inclusive by default; 4th bool arg to set inclusive) |
| `approxEq` | 2–3 | Numeric approximate equality with epsilon (default 1e-9) |

Initial implementation in this phase: `ne`, `lt`, `lte`, `gte`.

## Logical / Boolean Operators
| Name | Args | Semantics |
|------|------|-----------|
| `and` | 2+ | Short‑circuit; returns first falsy else last truthy (boolean result) |
| `or` | 2+ | Short‑circuit; returns first truthy else last (boolean result) |
| `not` | 1 | Unary negation |
| `xor` | 2 | Exactly one truthy |
| `if` | 2–3 | (Already implemented) Ternary selector |

Initial implementation in this phase: `and`, `or`, `not`, `xor`.

## Future Sets (Planned)
- Range/temporal: `between`, `before`, `after`, `within`.
- Membership: `in`, `has`, `isEmpty`.
- String: `contains`, `startsWith`, `endsWith`, `matches` (regex).
- Approximate numeric / tolerance: `approxEq`.
- Null / existence: `null`, `notNull` (or rely on `defined` + equality with `null`).
- Type predicates: `typeIs`, `isNumeric`.

## Comparison Semantics
1. If both numeric => convert to `decimal` (fallback to `double`) and compare.
2. If both Date/DateTime => chronological compare.
3. Else compare ordinal string representations.
4. `ne(a,b)` implemented as `!eq(a,b)` to remain consistent.

## Logical Semantics
- Short‑circuit is enforced left-to-right.
- Arguments are evaluated to truthiness; original Operator returns boolean (not the original element) for determinism in conditional PIs.

## Warning Strategy (Current & Future)
| Warning Type | Trigger |
|--------------|---------|
| `UnknownConditionalOperator` | Referenced but unregistered operator in `if` PI |
| (future) `InvalidOperatorArity` | Provided args outside Min/Max |
| (future) `InvalidComparisonTypes` | Unsupported types for comparison |

## Registration Order
`OperatorRegistry.RegisterBuiltInOperators()` appends new operators after existing ones to avoid breaking earlier name resolution.

## Test Coverage Plan (Phase 1)
- `ne`: true/false cases including numeric, string, null.
- `lt`, `lte`, `gt`, `gte`: boundary equality and ordering across int/decimal/double.
- `and`, `or`: short‑circuit behavior and multi-arg evaluation.
- `not`, `xor`: edge cases (double true / double false for xor).

---
Phase 1 (this change) adds: `ne`, `lt`, `lte`, `gte`, `and`, `or`, `not`, `xor`.

Subsequent phases extend per roadmap above.
