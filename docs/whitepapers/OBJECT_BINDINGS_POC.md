# XferLang Unified Invocation & Interop POC

Date: 2025-08-11 (Unified rewrite)
Status: Proposal (Pending Review)
Author: Copilot Agent

## 1. Goal
Adopt a single, uniform, s‑expression inspired **function application syntax** for *all* script evaluation: intrinsic operators, property access, method calls, reflection interop, future utilities. Every executable form is a tuple whose first element (head symbol) determines behavior; remaining children are argument expressions. No property-chain grammar; no key/value operator special cases in new examples. One mental model.

Examples:
```
<! script (
    let date getElementById (_document "sampleDate")
    let yr year (_date)
    let total count (_root (_document))
    log ("Year:" _yr "Total:" _total)
) !>

(MultiParam sampleClass
    (year (getElementById (_document "sampleDate")))
    (count (_root (_document))))
```

Design constraints:
- No new grammar; reuse existing tuple parsing.
- No evaluator heuristics to reinterpret sibling KVP nodes.
- Properties become unary functions; instance methods are functions with target as first arg.
- Extensible through function registry + controlled reflection allow‑list.

## 2. Non-Goals (Initial Scope)
- Property-chain sugar (e.g., `(document root count ())`).
- Pipeline / threading operators (`->`, `|>`). (Future macro layer.)
- Unrestricted reflection; only allow‑listed members.
- Overload resolution beyond basic arity + simple assignability.
- Async orchestration (initially disallow Task-returning members or treat result opaquely).

## 3. Parser Constraints
Parser already yields tuples; we rely strictly on tuple boundaries for argument grouping. Key/value pairs remain for *data* or legacy operator forms; they are not required for invocation. No post-parse structural rewriting.

### 3.1 Right-Associative Keyword Nesting (Clarification)
The existing grammar parses a sequence of keyword–value pairs in a right-associative manner. Thus a line like:
```
let date getElementById (_document "sampleDate")
```
is structurally equivalent to explicitly parenthesizing the invocation as:
```
let date (getElementById (_document "sampleDate"))
```
Parse tree sketch (outermost first):
```
KVP( key=let,
     value=KVP( key=date,
                value=KVP( key=getElementById,
                            value=Tuple( _document "sampleDate" ) ) ) )
```
Evaluation walks inward so the final tuple becomes the argument list for the `getElementById` function symbol, whose result is then the value bound to `date` by the enclosing `let`. This shorthand also works for:
```
let yr year (_date)
let total count (_root (_document))
```
Limitations / Guidance:
- Only the *innermost* node may be a tuple argument list; preceding identifiers are treated as nested keys, not separate arguments.
- Avoid mixing this form with additional sibling expressions on the same physical line to preserve clarity.
- Documentation examples may show either form; both are equivalent semantically.

### 3.2 Parameterized Intermediate Calls
When an intermediate result in a nested invocation itself requires parameters, each call still occupies its own tuple (or right-associative KVP chain terminating in a tuple). Example where an intermediate accessor hypothetically takes options:
```
let node getElementById (_document "sampleDate")
let transformed transformWithOptions (_node (options "fast"))
let finalResult process (_transformed (year (_node)))
```
Expanded parenthesized view for clarity:
```
let transformed (transformWithOptions (_node (options "fast")))
let finalResult (process (_transformed (year (_node))))
```
Rule of thumb: each symbol that accepts arguments owns exactly one tuple (its argument list). Nest by placing the next invocation as an argument expression.

## 4. Invocation Model
Tuple shape: `(head arg1 arg2 ...)`
1. If `head` is intrinsic (e.g., `let`, `if`, `and`, `or`, comparisons): intrinsic dispatch.
2. Else if `head` is a registered function symbol: call delegate.
3. Else attempt reflection resolution: treat `arg1` as target; map `head` to property or method on target type (allow‑listed).
4. Else: emit UnknownFunction warning → return `NullElement`.

Properties: `(year dateElem)`; Methods: `(getElementById document "id")`. Nest freely. No chain / hop grammar; explicit nesting only.

## 5. Execution Semantics
1. Evaluate intrinsic early-binding forms (`let`) before subsequent tuple siblings in the same script list.
2. For a non-intrinsic tuple, evaluate argument expressions left-to-right (respecting short-circuit for `and` / `or` / conditional branches).
3. Reflection dispatch: resolve property (arity=1) or method (target+args). Overload resolution = filter by arity then first assignment-compatible match; ambiguity → warning.
4. Marshal CLR args/results via marshaller.
5. Non-fatal issues yield warnings + `NullElement`.

## 6. Abstractions
```csharp
public interface IFunctionRegistry {
    bool TryGet(string name, out IFunction fn);
}
public interface IFunction {
    Element Invoke(EvalContext ctx, IReadOnlyList<Element> args);
}
public interface IMarshaller {
    object? ToClr(Element e, Type t);
    Element FromClr(object? value);
}
public sealed class EvalContext { /* parser, diagnostics, sinks, etc. */ }
```
Reflection layer separate: `IReflectionInvoker` handles allow‑list lookups.

## 7. Initial Function Set
| Symbol | Category | Signature | Notes |
|--------|----------|-----------|-------|
| `let` | Intrinsic (early) | (let name expr) | Binds name; returns NullElement. |
| `if` | Intrinsic | (if cond thenExpr [elseExpr]) | Lazy then/else. |
| `and` / `or` | Intrinsic | (and a b ...), (or a b ...) | Short-circuit. |
| `not` | Intrinsic | (not expr) | Boolean negation. |
| `eq` / `gt` / `lt` / etc. | Intrinsic | (eq a b) | Comparisons. |
| `log` | Intrinsic | (log part1 part2 ...) | Join (space) and emit. |
| `root` | Interop | (root (_document)) | Returns root element. |
| `count` | Interop | (count (_root (_document))) / (count collection) | Numeric length. |
| `getElementById` | Reflection | (getElementById (_document idString)) | Element or NullElement. |
| `year`..`epochSeconds` | Reflection(Property) | (year (_dateElement)) | Date component accessors. |

## 8. Marshalling (Minimal)
Map primitives (string/number/bool/null) and simple collections. Fail fast with diagnostic on unsupported target types. `FromClr` wraps primitives into existing element types; complex objects unsupported unless explicitly bound.

## 9. Diagnostics (Representative Codes)
| Code | Trigger | Handling |
|------|---------|----------|
| UnknownFunction | Head symbol undefined | Warn, NullElement |
| ArityMismatch | Intrinsic / function arity wrong | Warn |
| AmbiguousMember | Multiple reflection matches | Warn |
| MemberNotFound | Reflection failure | Warn |
| ArgumentTypeMismatch | Marshalling failed | Warn |
| NullTarget | Target argument null | Warn |

## 10. Reflection Allow‑List
Configuration lists: type → permitted members (methods, properties). All names normalized (e.g., camelCase) to function symbols. Properties produce unary functions; instance methods expect target as first argument. Optional static member exposure flagged explicitly.

## 11. Short-Circuit & Laziness
- `if`: evaluate condition; then only chosen branch.
- `and`: evaluate left-to-right; stop on falsy.
- `or`: evaluate left-to-right; stop on truthy.
Other functions evaluate all args before invocation.

## 12. Early Binding (`let`)
Script bodies are a sequence: walk once; whenever encountering `(let name expr)` evaluate `expr` with current environment, extend environment; result value can be any element. `let` returns NullElement so it can appear in expression sequences without affecting final value.

## 13. Testing Strategy
Core tests:
1. `Let_Binds_And_Interpolates`.
2. `Nested_Reflection_Call` `(year (getElementById (_document "d")))`.
3. `Count_Root` `(count (_root (_document)))`.
4. `Log_Joins_Parts`.
5. `UnknownFunction_Warns`.
6. `ArityMismatch_Warns`.
7. `AmbiguousMember_Warns` (manufactured type with overloads).
8. `DateAccessor_All`.

## 14. Incremental Implementation Plan
1. Function interfaces + simple registry (dictionary).
2. Implement intrinsics (`let`, `if`, `and`, `or`, `not`, `eq`).
3. Logging intrinsic + console sink abstraction.
4. Reflection allow‑list + invoker (methods + properties).
5. Register interop functions `root`, `count`, `getElementById`.
6. Date accessors (`year`, `month`, ...).
7. Integrate evaluator into script PI execution path.
8. Tests + diagnostics verification.
9. README update (replace chain examples with nested calls).

## 15. DateElement Accessors
Expose date components directly as unary functions (`(year date)`). Implementation: register functions that (a) ensure exactly one arg, (b) ensure arg is `DateElement`, (c) extract component.
Supported: `year`, `month`, `day`, `hour`, `minute`, `second`, `millisecond`, `offsetMinutes`, `dayOfWeek`, `dayOfYear`, `ticks`, `epochSeconds`, optional `iso`.
Future: `(now)`, `(parseDate isoString)`, arithmetic helpers `(addDays date 5)`.

## 16. Edge Cases
| Case | Handling |
|------|----------|
| Property accessor with >1 arg | ArityMismatch warning |
| Null target for reflection | NullTarget warning |
| Unsupported param type | ArgumentTypeMismatch warning |
| Void method | Return NullElement |
| Exception in reflection | Catch → warning + NullElement |

## 17. Alternatives Rejected
| Approach | Reason |
|----------|-------|
| Property-chain grammar | Ambiguity + duplicate mental model |
| Dual KVP + function forms for new features | Extra complexity; unify now (legacy KVP tolerated only for existing constructs) |
| Implicit target inference (dropping first arg) | Hidden magic, harder to reason |
| Full dynamic reflection (no allow‑list) | Safety + predictability |

## 18. Migration / Legacy
Legacy examples using `(document root count ())` become `(count (_root (_document)))`. Chain sugar is removed entirely; any transitional support would be a preprocessing rewrite (out of scope for core evaluator).

## 19. Performance Considerations
- Registry lookup: O(1) dictionary.
- Reflection: cache MethodInfo/PropertyInfo per (type,symbol,arity).
- Date accessors: trivial.
- Marshalling: minimal boxing; avoid reflection for primitives.

## 20. Future Macro Layer
Optional expansion step transforming pipeline-like or chain sugar into nested tuples before evaluation. Keeps evaluator stable; macros guaranteed to produce valid invocation tuples.

## 21. Acceptance Criteria
- All new tests pass; no regressions in existing suite.
- README + this whitepaper reflect unified model only.
- Diagnostics emitted for all negative scenarios enumerated.
- No leftover references to property chains in docs (except migration note).

## 22. Summary
Single uniform function application model adopted. Simpler evaluator, clear extension path, minimized ambiguity. Reflection interop constrained by allow‑list. Properties appear as unary functions; methods use explicit target argument. Date accessors delivered as ordinary unary functions. Future syntactic sugar (pipelines, chains) can compile down to this core without altering runtime semantics.

-- END --


