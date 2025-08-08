# XferLang Processing Instruction Execution Framework

## Overview

This document specifies the enhanced Processing Instruction (PI) execution framework for XferLang, designed to support scriptable document generation through conditional logic, evaluation functions, and composable PI operations while maintaining full extensibility for client-defined PIs.

## Core Design Philosophy

### 1. PIs as Universal Building Blocks
All operations in XferLang should be implemented as Processing Instructions, creating a unified, composable system where:
- **Conditionals are PIs**: `if`, `eq`, `gt`, `defined`, etc.
- **Script execution is a PI**: `script` PI executes sequences of other PIs
- **All operations follow PI patterns**: Consistent syntax and behavior across all functionality

### 2. Scriptable PI Composition
PIs should be composable into scripts that execute sequentially:
```xfer
<! script (
    if ~true (log {destination "console"})
    charDef {hhg \$42}
    if (defined "DEBUG_MODE") (log {message "Debug enabled"})
) !>
```

### 3. Unified Registration System
Both built-in and external PIs should use the same registration mechanism with attribute-based discovery:
```csharp
[ProcessingInstruction("if")]
public class IfProcessingInstruction : ProcessingInstruction { ... }

[ProcessingInstruction("log", External = true)]
public class LogProcessingInstruction : ProcessingInstruction { ... }
```

## Implementation Roadmap

### Phase 1: Foundation - Simple "if" PI with Environment Variable Check

Start with a minimal implementation that demonstrates the core concepts:

#### 1.1 Environment Variable Conditional
Implement a simple `ifdef`-style conditional that checks if an environment variable is defined:

```xfer
<! if (defined "DEBUG_MODE") !>
{
    debugPanel {
        enabled ~true
        level "verbose"
    }
}
```

**Implementation Goals:**
- Single conditional type: `defined <env_var_name>`
- Binary behavior: include element if variable exists, remove if not
- Foundation for more complex conditionals

#### 1.2 Basic PI Registration Framework
Create attribute-based PI registration system:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ProcessingInstructionAttribute : Attribute
{
    public string Keyword { get; }
    public bool External { get; set; } = false;

    public ProcessingInstructionAttribute(string keyword)
    {
        Keyword = keyword;
    }
}

public static class PIRegistry
{
    public static void RegisterFromAssembly(Assembly assembly);
    public static void RegisterExternal<T>() where T : ProcessingInstruction;
}
```

#### 1.3 Element Conditional Processing
Implement element-level conditional inclusion/exclusion:

```csharp
public abstract class ConditionalProcessingInstruction : ProcessingInstruction
{
    public abstract bool EvaluateCondition(PIExecutionContext context);

    public override void ElementHandler(Element element)
    {
        if (!EvaluateCondition(CurrentContext))
        {
            element.MarkForRemoval();
        }
    }
}
```

### Phase 2: Expand Conditionals

#### 2.1 Additional Conditional Types
Add more conditional operations as separate PIs:
- `eq`: Equality comparison
- `ne`: Not equal
- `gt`, `lt`, `ge`, `le`: Numeric comparisons
- `and`, `or`, `not`: Logical operations

#### 2.2 Nested Conditional Syntax
Support nested conditionals within `if`:
```xfer
<! if (and (defined "DEBUG_MODE") (eq "ENVIRONMENT" "development")) !>
{ devTools ~true }
```

#### 2.3 Value-Based Conditionals
Extend beyond environment variables to support value comparisons:
```xfer
<! if (gt "USER_COUNT" 10) !>
{ scalingAlert ~true }
```

### Phase 3: Script PI Implementation

#### 3.1 Sequential PI Execution
Implement `script` PI that executes a sequence of other PIs:

```xfer
<! script (
    charDef {debug \$DEBUG}
    if (defined "DEBUG_MODE") (
        log {destination "console" message "Debug mode active"}
    )
    if (gt "USER_COUNT" 100) (
        log {destination "file:alerts.log" level "warn"}
    )
) !>
```

#### 3.2 Script Context Management
Each script maintains its own execution context with variable scoping:

```csharp
public class ScriptProcessingInstruction : ProcessingInstruction
{
    private readonly List<ProcessingInstruction> _instructions;

    public override void ProcessingInstructionHandler()
    {
        var scriptContext = CreateScriptContext();

        foreach (var instruction in _instructions)
        {
            instruction.Execute(scriptContext);
        }
    }
}
```

#### 3.3 Conditional Element Inclusion in Scripts
Scripts can conditionally include elements based on PI evaluation results:

```xfer
<! script (
    if (defined "FEATURE_FLAGS") (
        include {
            featureToggles {
                newUI ~true
                betaFeatures ~false
            }
        }
    )
) !>
```

### Phase 4: Advanced Features

#### 4.1 PI Composition and Chaining
Support complex PI compositions:
```xfer
<! script (
    let debugEnabled (defined "DEBUG_MODE")
    let userCount (env "USER_COUNT")
