# Conditional Parsing via Processing Instructions - Feasibility Study

## ⚠️ Critical Parser Issues Identified & Resolved

**Two fundamental issues were discovered that required design revisions:**

### 1. PI Syntax Compliance Issue
❌ **Problem**: `<! else !>` violates XferLang's PI syntax requirements
✅ **Solution**: Use `<! else "" !>` - PI keywords must have values (even empty string)

### 2. Dynamic Element Type Ambiguity
❌ **Problem**: `<! if gt[<|VAR|> #4] !>` causes parser confusion - what type is `<|VAR|>`?
✅ **Solution**: Use explicit type markers: `<! if gt[#4 #<|VAR|>] !>` - both explicitly integers

**Key Takeaway**: The conditional parsing design must respect XferLang's existing parser assumptions rather than fight them.

---

## Overview

The idea of implementing conditional parsing through XferLang's processing instruction system is not only feasible but exceptionally elegant. By leveraging XferLang's existing PI infrastructure and right-associative key/value parsing, we can create a powerful conditional compilation system that feels natural within the language.

## Current PI Infrastructure Analysis

XferLang already has a robust processing instruction system:

### Existing PI Components
```csharp
// PI processor registry in Parser.cs
private readonly Dictionary<string, PIProcessor> _piProcessorRegistry = new();

// Built-in PI processors
- CharDefProcessingInstruction
- DocumentProcessingInstruction
- IdProcessingInstruction
- DynamicSourceProcessingInstruction

// Extensible registration system
public void RegisterPIProcessor(string piKey, PIProcessor processor);
```

### Parser Integration Points
- PIs are parsed during element construction
- PIs can be applied to subsequent elements via `_pendingPIs` queue
- Parser has context awareness through `_delimStack` and position tracking
- Element hierarchy provides parent/child relationships for context

## Conditional Parsing Design

### 1. Core Concept

Use processing instructions to create conditional blocks that are evaluated during parsing, allowing content to be included or excluded based on runtime conditions.

### 2. Syntax Design

```xferlang
{
    // Basic conditional - PI targets next element
    <! if eq["Windows" <|PLATFORM|>] !>
    windows-specific: "data"
    <! elseif eq["Linux" <|PLATFORM|>] !>
    linux-specific: "data"
    <! else "" !>
    default-config: "data"

    shared-config: "always included"

    // Nested conditionals work naturally within targeted elements
    <! if defined(<|DEBUG|>) !>
    debug-info: {
        <! if eq["verbose" <|LOG_LEVEL|>] !>
        verbose-logging: true

        debug-symbols: true
    }

    // Simple single branch with explicit type markers
    <! if gt[#42 #<|THRESHOLD|>] !>
    high-performance-mode: true

    normal-config: "value"

    // Complex expressions target next element
    <! if and(
        or(
            eq["Windows" <|PLATFORM|>]
            eq["macOS" <|PLATFORM|>]
        )
        gte["2.0" <|VERSION|>]
    ) !>
    modern-desktop-features: true

    legacy-support: "always included"
}
```

### 2.1. No `<!endif!>` Needed - C-Style Semantics

Following XferLang's existing PI design where **each PI targets the immediately following element**, conditional PIs work exactly like C-style languages:

```xferlang
{
    // Single element target (like C: if (condition) statement;)
    <! if eq["debug" <|MODE|>] !>
    debug: true

    // If-elseif-else chain targets next element after each PI
    <! if eq["prod" <|ENV|>] !>
    prod-config: {}
    <! elseif eq["staging" <|ENV|>] !>
    staging-config: {}
    <! else "" !>
    dev-config: {}

    // Nested conditionals work naturally
    <! if eq["Windows" <|PLATFORM|>] !>
    windows-settings: {
        <! if eq["11" <|VERSION|>] !>
        win11-features: true

        shared-windows-config: "value"
    }

    // Arrays work like any other element
    <! if eq["prod" <|ENV|>] !>
    services: [
        "core-service"
        "monitoring-service"
        "backup-service"
    ]

    <! else "" !>
    services: [
        "core-service"
        "dev-tools"
    ]

    // Multiple independent conditionals
    <! if gt[#4 #<|CPU_CORES|>] !>
    parallel-processing: true

    <! if gte[^8.0 ^<|MEMORY_GB|>] !>
    cache-size: "2GB"

    <! else "" !>
    cache-size: "512MB"
}
```

**Key Insight:** Just like in C, but with proper PI syntax:
- `if (condition) statement;` - PI targets next element: `<! if condition !>`
- `if (condition) { block }` - PI targets next object/array
- `if (condition) stmt1; else stmt2;` - Two PIs: `<! if condition !>` then `<! else "" !>`
- No `endif` needed because each PI has a clear, single target

**Critical PI Syntax Requirements:**
- ❌ `<! else !>` - Invalid! PI must have key/value pair
- ✅ `<! else "" !>` - Valid! Keyword followed by value (empty string)
- ✅ `<! elseif condition !>` - Valid! Keyword followed by expression value

**Dynamic Element Type Resolution:**
- ❌ `<|VAR|>` in numeric context causes parser confusion
- ✅ `#<|VAR|>` explicitly resolves as integer in comparisons
- ✅ `"<|VAR|>"` explicitly resolves as string in comparisons
- ✅ `^<|VAR|>` explicitly resolves as decimal in comparisons
- ✅ `~<|VAR|>` explicitly resolves as boolean in comparisons

### 3. Expression Language

Leverage XferLang's right-associative parsing and array-based type safety for conditional expressions:

```xferlang
// Type-safe comparison operations (exactly 2 values in homogeneous arrays)
eq[value1 value2]          // equality - types must match
ne[value1 value2]          // inequality - types must match
lt[value1 value2]          // less than - comparable types
gt[value1 value2]          // greater than - comparable types
lte[value1 value2]         // less than or equal - comparable types
gte[value1 value2]         // greater than or equal - comparable types

// Examples with explicit types and proper dynamic element handling
eq[#42 #<|NUMERIC_VALUE|>]              // integer comparison - both explicitly integers
eq["string" "<|STRING_VALUE|>"]          // string comparison - both explicitly strings
gt[^3.14 ^<|FLOAT_VALUE|>]              // decimal comparison - both explicitly decimals
eq[~true ~<|BOOLEAN_VALUE|>]            // boolean comparison - both explicitly booleans

// WRONG: Mixed or untyped dynamic elements
// eq[<|VALUE|> "string"]               // ❌ Parser confusion - what type is <|VALUE|>?
// gt[#42 <|THRESHOLD|>]                // ❌ Parser expects dynamic element, gets literal

// CORRECT: Explicit typing for all values
eq["Windows" "<|PLATFORM|>"]            // ✅ Both strings
gt[#4 #<|CPU_CORES|>]                   // ✅ Both integers
gte[^2.0 ^<|VERSION|>]                  // ✅ Both decimals

// Logical operations (can take multiple arguments via tuples)
and(expr1 expr2 ...)       // logical AND - all must be true
or(expr1 expr2 ...)        // logical OR - at least one must be true
not(expr)                  // logical NOT - single argument

// Utility functions (single argument, type-specific dynamic elements)
defined(<|VARIABLE|>)      // check if variable is defined (raw dynamic element OK)
typeof(<|VALUE|>)          // get type of value (raw dynamic element OK)

// Collection operations (mixed argument types in tuples)
length(collection)         // get length of collection
contains(collection item)  // check if collection contains item

// String operations (string arrays for type safety)
startswith["<|STRING|>" "prefix"]   // both must be explicitly strings
endswith["<|STRING|>" "suffix"]     // both must be explicitly strings
matches["<|STRING|>" "pattern"]     // regex matching - both strings

// Version comparison (string arrays, treated as version numbers)
version-eq["<|V1|>" "<|V2|>"]      // version equality
version-gt["<|V1|>" "<|V2|>"]      // version greater than
version-gte["<|V1|>" "<|V2|>"]     // version greater than or equal

// Multi-value operations (arrays with 2+ elements, all same type)
in["<|VALUE|>" "option1" "option2" "option3"]    // value is in list of string options
in[#<|VALUE|> #1 #2 #3]                         // value is in list of integer options
between[#<|VALUE|> #1 #10]                     // integer value between min and max
between[^<|VALUE|> ^1.0 ^10.0]                 // decimal value between min and max
```

**Key Design Principles:**
- **PI Syntax Compliance**: All PIs must follow key/value structure (`<! key value !>`)
- **Explicit Type Markers**: Dynamic elements need type prefixes in comparisons (`#<|VAR|>` not `<|VAR|>`)
- **Arrays for type safety**: Comparison operators use arrays to enforce homogeneous types
- **Tuples for mixed types**: Logical and utility functions use tuples when types can vary
- **Right-associative advantage**: Enables natural nesting and composition
- **Minimal operators**: Start with essential operations, extend as needed

### 3.1. Addressing Parser Issues

**Problem 1: Invalid PI Syntax**
```xferlang
// ❌ WRONG - violates PI key/value requirement
<! else !>

// ✅ CORRECT - follows PI syntax rules
<! else "" !>        // else with empty string value
<! elseif condition !>  // elseif with condition expression value
```

**Problem 2: Dynamic Element Type Ambiguity**
```xferlang
// ❌ WRONG - parser confusion about types
<! if gt[<|COUNT|> #4] !>     // What type is <|COUNT|>? Parser expects dynamic element
<! if eq[<|PLATFORM|> "Win"] !>  // Mixed types without explicit markers

// ✅ CORRECT - explicit type markers resolve ambiguity
<! if gt[#<|COUNT|> #4] !>    // Both explicitly integers
<! if eq["<|PLATFORM|>" "Win"] !>  // Both explicitly strings
<! if gte[^<|VERSION|> ^2.0] !>     // Both explicitly decimals
<! if eq[~<|DEBUG|> ~true] !>       // Both explicitly booleans
```

**Problem 3: Parser Logic Quirks**
```xferlang
// Current behavior: This works due to parser quirk
<! if gt[#<|VAR|> #4] !>      // Type marker forces integer resolution

// Make this the standard: Always use explicit type markers
<! if defined(<|VAR|>) !>     // OK - utility functions handle raw dynamic elements
<! if eq[#<|VAR|> #42] !>     // Required - comparisons need explicit types
```

### 4. Variable System

Extend dynamic elements (`<|VAR|>`) for conditional evaluation:

```csharp
public class ConditionalContext
{
    public Dictionary<string, object> Variables { get; set; } = new();
    public Dictionary<string, Func<object[], bool>> Functions { get; set; } = new();

    // Built-in variables
    public string Platform => Environment.OSVersion.Platform.ToString();
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString();
    public Version DotNetVersion => Environment.Version;
    public bool IsDebug =>
        #if DEBUG
            true;
        #else
            false;
        #endif
}
```

## Implementation Strategy

### Phase 1: Core Conditional PIs

```csharp
public class ConditionalProcessingInstruction : ProcessingInstruction
{
    public ConditionalType Type { get; }          // If, ElseIf, Else, EndIf
    public Expression? Condition { get; }          // Parsed condition expression
    public bool ShouldInclude { get; set; }       // Evaluation result

    public enum ConditionalType { If, ElseIf, Else, EndIf }
}

public class ConditionalProcessor : PIProcessor
{
    private Stack<ConditionalBlock> _conditionalStack = new();
    private ConditionalContext _context = new();

    public ProcessingInstruction Process(KeyValuePairElement kvp, Parser parser)
    {
        string command = kvp.Key;

        return command switch
        {
            "if" => ProcessIf(kvp.Value, parser),
            "elseif" => ProcessElseIf(kvp.Value, parser),
            "else" => ProcessElse(parser),
            "endif" => ProcessEndIf(parser),
            _ => throw new InvalidOperationException($"Unknown conditional command: {command}")
        };
    }
}
```

### Phase 2: Expression Evaluator

```csharp
public abstract class Expression
{
    public abstract object Evaluate(ConditionalContext context);
}

public class ArrayComparisonExpression : Expression
{
    public string Operator { get; set; }
    public ArrayElement Arguments { get; set; }

    public override object Evaluate(ConditionalContext context)
    {
        // Validate array has exactly 2 elements for binary operations
        if (Arguments.Count != 2)
            throw new InvalidOperationException($"Operator '{Operator}' requires exactly 2 arguments, got {Arguments.Count}");

        // Array homogeneity is enforced by ArrayElement - types are guaranteed to match
        var left = EvaluateElement(Arguments[0], context);
        var right = EvaluateElement(Arguments[1], context);

        return Operator switch
        {
            "eq" => left.Equals(right),
            "ne" => !left.Equals(right),
            "lt" => Comparer.Default.Compare(left, right) < 0,
            "gt" => Comparer.Default.Compare(left, right) > 0,
            "lte" => Comparer.Default.Compare(left, right) <= 0,
            "gte" => Comparer.Default.Compare(left, right) >= 0,
            "startswith" => left.ToString().StartsWith(right.ToString()),
            "endswith" => left.ToString().EndsWith(right.ToString()),
            "matches" => Regex.IsMatch(left.ToString(), right.ToString()),
            _ => throw new InvalidOperationException($"Unknown array operator: {Operator}")
        };
    }
}

public class TupleLogicalExpression : Expression
{
    public string Operator { get; set; }
    public TupleElement Arguments { get; set; }

    public override object Evaluate(ConditionalContext context)
    {
        return Operator switch
        {
            "and" => Arguments.All(arg => Convert.ToBoolean(EvaluateElement(arg, context))),
            "or" => Arguments.Any(arg => Convert.ToBoolean(EvaluateElement(arg, context))),
            "not" when Arguments.Count == 1 => !Convert.ToBoolean(EvaluateElement(Arguments[0], context)),
            "defined" when Arguments.Count == 1 => IsVariableDefined(Arguments[0], context),
            _ => throw new InvalidOperationException($"Unknown tuple operator: {Operator}")
        };
    }

    private bool IsVariableDefined(Element element, ConditionalContext context)
    {
        if (element is DynamicElement dynamic)
            return context.Variables.ContainsKey(dynamic.Value);
        return false;
    }
}

public class VariableExpression : Expression
{
    public string VariableName { get; set; }

    public override object Evaluate(ConditionalContext context)
    {
        if (context.Variables.TryGetValue(VariableName, out var value))
            return value;

        // Check built-in variables
        return VariableName switch
        {
            "PLATFORM" => context.Platform,
            "ARCHITECTURE" => context.Architecture,
            "DEBUG" => context.IsDebug,
            _ => throw new InvalidOperationException($"Undefined variable: {VariableName}")
        };
    }
}

public class LiteralExpression : Expression
{
    public object Value { get; set; }

    public override object Evaluate(ConditionalContext context) => Value;
}

private static object EvaluateElement(Element element, ConditionalContext context)
{
    return element switch
    {
        DynamicElement dynamic => new VariableExpression { VariableName = dynamic.Value }.Evaluate(context),
        StringElement str => str.Value,
        IntegerElement num => num.Value,
        DecimalElement dec => dec.Value,
        BooleanElement boolean => boolean.Value,
        _ => throw new InvalidOperationException($"Cannot evaluate element type: {element.GetType()}")
    };
}
```

public class VariableExpression : Expression
{
    public string VariableName { get; set; }

    public override object Evaluate(ConditionalContext context)
    {
        if (context.Variables.TryGetValue(VariableName, out var value))
            return value;

        // Check built-in variables
        return VariableName switch
        {
            "PLATFORM" => context.Platform,
            "ARCHITECTURE" => context.Architecture,
            "DEBUG" => context.IsDebug,
            _ => throw new InvalidOperationException($"Undefined variable: {VariableName}")
        };
    }
}

public class LiteralExpression : Expression
{
    public object Value { get; set; }

    public override object Evaluate(ConditionalContext context) => Value;
}
```

### Phase 3: Parser Integration

```csharp
public class ConditionalAwareParser : Parser
{
    private Stack<ConditionalBlock> _conditionalStack = new();
    private ConditionalContext _conditionalContext = new();
    private bool _skipParsing = false;

    protected override Element ParseElement()
    {
        // Check if we're in a skipped conditional block
        if (_skipParsing && !IsConditionalPI())
        {
            return SkipElement();
        }

        return base.ParseElement();
    }

    private bool IsConditionalPI()
    {
        // Look ahead to see if this is a conditional PI
        return ElementOpening(ProcessingInstruction.ElementDelimiter, out _) &&
               PeekForConditionalKeyword();
    }

    private Element SkipElement()
    {
        // Fast-skip parsing until we find the next conditional PI
        SkipToNextConditionalPI();
        return new EmptyElement(); // Return placeholder
    }
}

public class ConditionalBlock
{
    public ConditionalType Type { get; set; }
    public bool ConditionMet { get; set; }
    public bool HasMatchedBranch { get; set; }
    public int StartPosition { get; set; }
    public int StartRow { get; set; }
    public int StartColumn { get; set; }
}
```

### Phase 4: Expression Parser

Leverage XferLang's existing parsing with array/tuple distinction for type safety:

```csharp
public class ExpressionParser
{
    public Expression ParseExpression(Element element)
    {
        return element switch
        {
            // Array-based type-safe operations: operator[value1 value2]
            KeyValuePairElement kvp when kvp.Value is ArrayElement array =>
                new ArrayComparisonExpression
                {
                    Operator = kvp.Key,
                    Arguments = array
                },

            // Tuple-based logical operations: operator(expr1 expr2 ...)
            KeyValuePairElement kvp when kvp.Value is TupleElement tuple =>
                new TupleLogicalExpression
                {
                    Operator = kvp.Key,
                    Arguments = tuple
                },

            // Variable reference: <|VAR|>
            DynamicElement dynamic => new VariableExpression { VariableName = dynamic.Value },

            // Literal values
            StringElement str => new LiteralExpression { Value = str.Value },
            IntegerElement num => new LiteralExpression { Value = num.Value },
            DecimalElement dec => new LiteralExpression { Value = dec.Value },
            BooleanElement boolean => new LiteralExpression { Value = boolean.Value },

            _ => throw new InvalidOperationException($"Unsupported expression element: {element.GetType()}")
        };
    }
}
```

**Expression Parsing Examples:**

```xferlang
// Parsed as ArrayComparisonExpression with homogeneous string array
eq["Windows" <|PLATFORM|>]

// Parsed as ArrayComparisonExpression with homogeneous integer array
gt[#42 <|THRESHOLD|>]

// Parsed as TupleLogicalExpression with mixed expression types
and(
    eq[<|PLATFORM|> "Windows"]
    gt[<|VERSION|> "1.0"]
)

// Nested expressions maintain type safety at each level
or(
    and(
        eq["Windows" <|PLATFORM|>]
        gte[#10 <|MIN_VERSION|>]
    )
    eq["Linux" <|PLATFORM|>]
)
```

## Use Cases

### 1. Platform-Specific Configuration

```xferlang
{
    database: {
        <! if eq["Windows" "<|PLATFORM|>"] !>
        connection-string: "Server=.\\SQLEXPRESS;Database=MyApp;Integrated Security=true"
        <! elseif eq["Linux" "<|PLATFORM|>"] !>
        connection-string: "Host=localhost;Database=myapp;Username=user;Password=pass"
        <! else "" !>
        connection-string: "postgresql://localhost/myapp"
    }

    logging: {
        <! if eq["Development" "<|ENVIRONMENT|>"] !>
        level: "Debug"
        console: true
        <! elseif eq["Production" "<|ENVIRONMENT|>"] !>
        level: "Warning"
        file: "/var/log/myapp.log"
    }
    }

    performance: {
        <! if gt[#4 #<|CPU_CORES|>] !>
        parallel-processing: true
        worker-threads: #<|CPU_CORES|>

        <! if gte[^8.0 ^<|MEMORY_GB|>] !>
        cache-size: "2GB"
        <! elseif gte[^4.0 ^<|MEMORY_GB|>] !>
        cache-size: "1GB"
        <! else "" !>
        cache-size: "512MB"
    }
}
}
```

### 2. Feature Flags

```xferlang
{
    features: {
        <! if defined(<|ENABLE_FEATURE_X|>) !>
        feature-x: {
            enabled: true
            config: { threshold: 100 }
        }
        <! endif !>

        <! if and(defined(<|BETA_FEATURES|>) eq(<|VERSION|> "2.0-beta")) !>
        experimental-ui: true
        <! endif !>
    }
}
```

### 3. Build Configuration

```xferlang
{
    build: {
        <! if eq[<|BUILD_TYPE|> "Debug"] !>
        optimization: false
        debug-symbols: true
        assertions: true

        <! elseif eq[<|BUILD_TYPE|> "Release"] !>
        optimization: true
        debug-symbols: false
        assertions: false

        <! elseif eq[<|BUILD_TYPE|> "Profile"] !>
        optimization: true
        debug-symbols: true
        profiling: true
        <! endif !>

        output-dir: {
            <! if eq["Windows" "<|PLATFORM|>"] !>
            path: "bin\\<|BUILD_TYPE|>"
            <! else "" !>
            path: "bin/<|BUILD_TYPE|>"
        }

        compiler-flags: [
            <! if eq["Debug" "<|BUILD_TYPE|>"] !>
            "-g" "-O0"
            <! elseif eq["Release" "<|BUILD_TYPE|>"] !>
            "-O3" "-DNDEBUG"

            <! if eq["x64" "<|ARCHITECTURE|>"] !>
            "-m64"
            <! elseif eq["x86" "<|ARCHITECTURE|>"] !>
            "-m32"
        ]

        version-check: {
            <! if version-gte["<|COMPILER_VERSION|>" "11.0"] !>
            cpp-standard: "c++20"
            <! elseif version-gte["<|COMPILER_VERSION|>" "9.0"] !>
            cpp-standard: "c++17"
            <! else "" !>
            cpp-standard: "c++14"
        }
    }
}
```

### 4. Complex Conditional Logic

```xferlang
{
    deployment: {
        <! if and(
            or(
                eq[<|ENVIRONMENT|> "Staging"]
                eq[<|ENVIRONMENT|> "Production"]
            )
            version-gte[<|VERSION|> "1.5.0"]
            not(contains(<|FEATURES|> "experimental"))
        ) !>
        auto-deploy: true
        health-checks: [
            "database"
            "cache"
            "external-api"
        ]
        <! endif !>

        <! if or(
            eq[<|ENVIRONMENT|> "Development"]
            defined(<|FORCE_MANUAL_DEPLOY|>)
        ) !>
        manual-approval: true
        <! endif !>

        resource-limits: {
            <! if and(
                eq["Production" "<|ENVIRONMENT|>"]
                gt[#1000 #<|EXPECTED_LOAD|>]
            ) !>
            cpu-limit: "4.0"
            memory-limit: "8GB"

            <! elseif eq["Staging" "<|ENVIRONMENT|>"] !>
            cpu-limit: "2.0"
            memory-limit: "4GB"

            <! else "" !>
            cpu-limit: "1.0"
            memory-limit: "2GB"
        }

        feature-gates: {
            <! if and(
                eq["premium" "<|TIER|>"]
                gte[#100 #<|USER_COUNT|>]
            ) !>
            advanced-analytics: true

            <! if in["<|REGION|>" "us-east" "us-west" "eu-central"] !>
            compliance-mode: "strict"
            <! else "" !>
            compliance-mode: "standard"
        }
    }
}
```

## Advanced Features

### 1. Include Files with Conditions

```xferlang
{
    <! if eq["Production" "<|ENVIRONMENT|>"] !>
    <! include "prod-config.xfer" !>
    <! else "" !>
    <! include "dev-config.xfer" !>

    // Conditional includes based on feature flags
    <! if defined(<|ENABLE_MONITORING|>) !>
    <! include "monitoring-config.xfer" !>

    // Platform-specific includes
    <! if eq["Windows" "<|PLATFORM|>"] !>
    <! include "windows-settings.xfer" !>
    <! elseif eq["Linux" "<|PLATFORM|>"] !>
    <! include "linux-settings.xfer" !>
}
```

### 2. Conditional Schema Validation

```xferlang
{
    <! if eq[~true ~<|STRICT_MODE|>] !>
    <! schema "strict-schema.xfer" !>
    <! else "" !>
    <! schema "loose-schema.xfer" !>

    // Environment-specific schema enforcement
    <! if eq["Production" "<|ENVIRONMENT|>"] !>
    <! schema "production-schema.xfer" !>
    <! elseif eq["Development" "<|ENVIRONMENT|>"] !>
    <! schema "development-schema.xfer" !>

    user-data: { /* ... */ }
}
```

### 3. Template Generation

```xferlang
{
    services: [
        <! for service in <|SERVICES|> !>
        {
            name: "<|service.name|>"
            port: #<|service.port|>
            <! if eq["web" "<|service.type|>"] !>
            protocol: "HTTP"
            health-check: "/health"
            <! elseif eq["database" "<|service.type|>"] !>
            protocol: "TCP"
            timeout: #30

            resources: {
                <! if gt[#100 #<|service.load|>] !>
                cpu: "2.0"
                memory: "4GB"
                <! else "" !>
                cpu: "1.0"
                memory: "2GB"
            }
        }
    ]

    load-balancer: {
        <! if gt[#3 length(<|SERVICES|>)] !>
        algorithm: "round-robin"
        health-checks: true
        <! else "" !>
        algorithm: "least-connections"
    }
}
```

## Implementation Challenges & Solutions

### 1. Parser State Management

**Challenge**: Maintaining parser state across conditional blocks

**Solution**:
- Use parser position tracking for rollback
- Implement conditional block stack for nested conditions
- Preserve delimiter stack state

### 2. Performance Optimization

**Challenge**: Efficiently skipping large conditional blocks

**Solution**:
- Implement fast-forward parsing that only looks for conditional PIs
- Use position bookmarking for quick jumps
- Cache evaluation results for repeated conditions

### 3. Error Handling

**Challenge**: Meaningful error messages in skipped blocks

**Solution**:
- Track conditional context in error messages
- Provide suggestions for undefined variables
- Show evaluation trace for complex expressions

### 4. IDE Integration

**Challenge**: Syntax highlighting and IntelliSense in conditional blocks

**Solution**:
- Extend VS Code extension with conditional awareness
- Provide hover information for variable values
- Gray out inactive conditional blocks

## Feasibility Assessment

### ✅ **Highly Feasible**

1. **Existing Infrastructure**: XferLang's PI system provides the perfect foundation
2. **Parser Integration**: Minimal changes needed to existing parser
3. **Expression Language**: Right-associative parsing naturally supports Lisp-like syntax
4. **Performance**: Can be implemented efficiently with skip-ahead parsing
5. **Extensibility**: Plugin system allows custom functions and variables

### 📋 **Implementation Complexity**

- **Low**: Basic if/else conditionals
- **Medium**: Expression evaluation system
- **Medium**: Nested conditional handling
- **High**: Advanced features (loops, includes)

### 🚀 **Recommended Approach**

1. **Start Small**: Implement basic if/else with simple variable substitution
2. **Iterate**: Add expression language incrementally
3. **Test Thoroughly**: Focus on edge cases and nested scenarios
4. **Document Well**: Provide clear examples and best practices

## Conclusion

Conditional parsing via processing instructions is not only feasible but represents a natural evolution of XferLang's capabilities. The combination of the existing PI infrastructure, right-associative parsing, and dynamic element system provides an elegant foundation for a powerful conditional compilation feature.

This feature would position XferLang as a unique configuration language that bridges the gap between static configuration files and dynamic templating systems, offering the safety and predictability of compile-time evaluation with the flexibility of runtime configuration.

The Lisp-like expression syntax feels natural within XferLang's design philosophy and leverages existing parsing capabilities effectively. Starting with basic conditionals and building up to more complex features would provide a clear implementation path with demonstrable value at each stage.
