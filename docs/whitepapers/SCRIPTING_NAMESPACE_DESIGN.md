# Scripting Namespace Architecture Design

## Overview

This document outlines the architectural separation between **Processing Instructions** (document-level directives) and **Scripting** (expression evaluation operators) to create a cleaner, more maintainable codebase.

## Current State Analysis

### Processing Instructions (Document Directives)
Currently in `ParksComputing.Xfer.Lang.ProcessingInstructions`:
- `CharDefProcessingInstruction` - Define custom character mappings
- `DefinedProcessingInstruction` - Check if element is defined (conditional logic)
- `DocumentProcessingInstruction` - Document metadata
- `DynamicSourceProcessingInstruction` - Configure dynamic data sources
- `IdProcessingInstruction` - Assign element IDs
- `ProcessingInstruction` - Base class
- `PropertiesProcessingInstruction` - Element properties
- `TagProcessingInstruction` - Element tagging

### Problem
- `DefinedProcessingInstruction` is really an expression evaluator, not a document directive
- Future conditional operators (`eq`, `gt`, `lt`, `and`, `or`, etc.) would blur this line further
- PIs become overloaded with both document processing AND scripting concerns

## Proposed Architecture

### 1. **ParksComputing.Xfer.Lang.Scripting** (New Namespace)

#### Core Abstractions
```csharp
// Base expression evaluation
public abstract class ScriptingOperator {
    public abstract string OperatorName { get; }
    public abstract object Evaluate(ScriptingContext context, params Element[] arguments);
}

public class ScriptingContext {
    public Dictionary<string, object> Variables { get; set; } = new();
    public Parser? Parser { get; set; }
    public Element? CurrentElement { get; set; }
    // Built-in environment access
    public string Platform => Environment.OSVersion.Platform.ToString();
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString();
    // etc.
}
```

#### Operator Categories

**Comparison Operators** (`ParksComputing.Xfer.Lang.Scripting.Comparison`)
- `EqualsOperator` (`eq`)
- `NotEqualsOperator` (`ne`)
- `GreaterThanOperator` (`gt`)
- `LessThanOperator` (`lt`)
- `GreaterThanOrEqualOperator` (`gte`)
- `LessThanOrEqualOperator` (`lte`)

**Logical Operators** (`ParksComputing.Xfer.Lang.Scripting.Logical`)
- `AndOperator` (`and`)
- `OrOperator` (`or`)
- `NotOperator` (`not`)

**Utility Operators** (`ParksComputing.Xfer.Lang.Scripting.Utility`)
- `DefinedOperator` (`defined`)
- `TypeOfOperator` (`typeof`)
- `LengthOperator` (`length`)
- `ContainsOperator` (`contains`)

**String Operators** (`ParksComputing.Xfer.Lang.Scripting.String`)
- `StartsWithOperator` (`startswith`)
- `EndsWithOperator` (`endswith`)
- `MatchesOperator` (`matches`)

**Version Operators** (`ParksComputing.Xfer.Lang.Scripting.Version`)
- `VersionEqualsOperator` (`version-eq`)
- `VersionGreaterThanOperator` (`version-gt`)
- `VersionLessThanOperator` (`version-lt`)

### 2. **Processing Instructions** (Document-Level Only)

**Keep in `ParksComputing.Xfer.Lang.ProcessingInstructions`:**
- `CharDefProcessingInstruction` - Document-level character definitions
- `DocumentProcessingInstruction` - Document metadata
- `DynamicSourceProcessingInstruction` - Document-level source configuration
- `IdProcessingInstruction` - Element identification
- `PropertiesProcessingInstruction` - Element properties
- `TagProcessingInstruction` - Element tagging

**Add New Control Flow PIs:**
- `IfProcessingInstruction` - Conditional inclusion (mirrors `if` statement)
- `ElseProcessingInstruction` - Alternative inclusion
- `SwitchProcessingInstruction` - Multi-way branching
- `CaseProcessingInstruction` - Switch case matching

### 3. **Hybrid Elements** (Both PI and Scripting)

Some operators merit **both** representations:

#### `defined` - Example of Clean Separation

**Scripting Operator** (`DefinedOperator`):
```csharp
public class DefinedOperator : ScriptingOperator {
    public override string OperatorName => "defined";

    public override object Evaluate(ScriptingContext context, params Element[] arguments) {
        if (arguments.Length != 1)
            throw new ArgumentException("defined requires exactly 1 argument");

        return arguments[0]?.ParsedValue != null;
    }
}
```

**Processing Instruction** (`DefinedProcessingInstruction`):
```csharp
public class DefinedProcessingInstruction : ProcessingInstruction {
    private readonly DefinedOperator _operator = new();

    public override void ProcessingInstructionHandler() {
        var context = new ScriptingContext { /* ... */ };
        IsDefined = (bool)_operator.Evaluate(context, SourceElement);
    }
}
```

#### `if` - Control Flow with Expression Evaluation

**Processing Instruction** (`IfProcessingInstruction`):
```csharp
public class IfProcessingInstruction : ProcessingInstruction {
    private readonly ScriptingEngine _engine = new();

    public bool ShouldInclude { get; private set; }

    public override void ProcessingInstructionHandler() {
        var context = new ScriptingContext { /* parser context */ };
        ShouldInclude = (bool)_engine.Evaluate(Value, context);
    }
}
```

**Scripting Engine** (Expression Evaluator):
```csharp
public class ScriptingEngine {
    private readonly Dictionary<string, ScriptingOperator> _operators = new();

    public object Evaluate(Element expression, ScriptingContext context) {
        // Parse expression tree and delegate to appropriate operators
        // Example: eq["Windows" "<|PLATFORM|>"] -> EqualsOperator.Evaluate()
    }
}
```

## Migration Strategy

### Phase 1: Create Scripting Foundation
1. **Create base namespace**: `ParksComputing.Xfer.Lang.Scripting`
2. **Add core abstractions**: `ScriptingOperator`, `ScriptingContext`, `ScriptingEngine`
3. **Implement basic operators**: `defined`, `eq`, `gt`, `lt`, `and`, `or`

### Phase 2: Migrate Existing Logic
1. **Move `DefinedProcessingInstruction` logic** to `DefinedOperator`
2. **Update `DefinedProcessingInstruction`** to delegate to operator
3. **Ensure backward compatibility** - no breaking changes

### Phase 3: Add Control Flow PIs
1. **Implement `IfProcessingInstruction`** using scripting engine
2. **Add `ElseProcessingInstruction`** with proper syntax (`<! else "" !>`)
3. **Test integration** with existing conditional logic

### Phase 4: Advanced Operators
1. **String operations**: `startswith`, `endswith`, `matches`
2. **Version operations**: `version-eq`, `version-gt`, etc.
3. **Collection operations**: `contains`, `length`

## Benefits

### 🏗️ **Architectural Clarity**
- **Clear separation of concerns**: Document processing vs expression evaluation
- **Single responsibility**: Each class has one focused purpose
- **Easier testing**: Operators can be unit tested independently

### 🔄 **Reusability**
- **Scripting operators** can be used by multiple PIs
- **No duplication**: `eq` logic written once, used everywhere
- **Extensibility**: New operators don't require new PIs

### 📈 **Maintainability**
- **Logical organization**: Related operators grouped together
- **Consistent interfaces**: All operators implement same contract
- **Future-proof**: Easy to add new operator categories

### 🎯 **Performance**
- **Operator registry**: Fast lookup for expression evaluation
- **Compiled expressions**: Potential for expression caching/optimization
- **Reduced PI overhead**: Simple delegation pattern

## Implementation Notes

### Type Safety
```csharp
// Operators can enforce type safety
public class GreaterThanOperator : ScriptingOperator {
    public override object Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments, 2);
        ValidateComparable(arguments[0], arguments[1]);

        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);

        return Comparer.Default.Compare(left, right) > 0;
    }
}
```

### Dynamic Element Resolution
```csharp
// Operators handle dynamic element resolution consistently
public abstract class ScriptingOperator {
    protected object ResolveValue(Element element, ScriptingContext context) {
        return element switch {
            DynamicElement dynamic => ResolveDynamicValue(dynamic, context),
            _ => element.ParsedValue
        };
    }
}
```

### Expression Parsing
```csharp
// ScriptingEngine parses XferLang expressions into operator calls
// eq["Windows" "<|PLATFORM|>"] -> EqualsOperator.Evaluate(context, "Windows", platform_value)
// and(eq[...] gt[...]) -> AndOperator.Evaluate(context, result1, result2)
```

## Conclusion

This architecture provides:
- **Clean separation** between document processing and scripting
- **Flexible foundation** for complex conditional logic
- **Backward compatibility** with existing code
- **Extensible design** for future operator additions

The `defined` operator becomes the first example of this pattern, with both a reusable scripting operator and a PI that delegates to it for document-level conditional evaluation.
