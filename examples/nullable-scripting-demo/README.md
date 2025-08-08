# Nullable Scripting Demo

This example demonstrates the benefits of changing `ScriptingOperator.Evaluate` from returning `object` to `object?` (nullable object).

## What This Demo Shows

1. **Nullable Return Semantics**: How the `IfOperator` can return `null` when no false-value is provided and the condition is false
2. **Type Safety**: How the nullable return type helps catch potential null reference issues at compile time
3. **Null-Safe Patterns**: Various C# patterns for safely handling nullable returns
4. **Developer Experience**: How IntelliSense and compiler warnings improve code quality

## Key Benefits Demonstrated

### Before the Change
```csharp
public abstract object Evaluate(ScriptingContext context, params Element[] arguments);
// Could return null but didn't indicate it in the type system
```

### After the Change
```csharp
public abstract object? Evaluate(ScriptingContext context, params Element[] arguments);
// Clearly indicates that null is a valid return value
```

## Running the Demo

```bash
cd examples/nullable-scripting-demo
dotnet run
```

## What You'll See

The demo will show:

1. **IfOperator behavior**: Different scenarios where the operator returns values or null
2. **Null-safe handling**: Using `?.`, `??`, pattern matching, and switch expressions
3. **Type safety**: How the compiler and IDE help prevent null reference exceptions
4. **Architectural benefits**: Why this change improves the API design

## Code Examples

### Null-Conditional Operator
```csharp
var result = engine.Evaluate("if", undefinedVar, "success");
var length = result?.ToString()?.Length ?? 0; // Safe even if result is null
```

### Null-Coalescing Operator
```csharp
var safeValue = result ?? "DEFAULT_VALUE"; // Provides fallback for null
```

### Pattern Matching
```csharp
var message = result switch {
    string s => $"Got string: {s}",
    null => "No value available",
    _ => $"Got {result.GetType().Name}: {result}"
};
```

## Integration with Processing Instructions

The demo also shows how this change benefits the hybrid Processing Instruction approach, where PIs like `IfProcessingInstruction` delegate to scripting operators while properly handling nullable returns.

This demonstrates the architectural decision to separate document processing (PIs) from expression evaluation (operators) while maintaining type safety throughout the system.
