# If Processing Instruction Demo

This project demonstrates the comprehensive usage of the `<! if !>` processing instruction (PI) in XferLang. The If PI provides conditional element processing based on various types of conditions and expressions.

## Overview

The If Processing Instruction (`<! if !>`) evaluates conditional expressions and affects how target elements are processed. It demonstrates the hybrid approach where PIs delegate to reusable Scripting operators while providing document-level conditional semantics.

## Features Demonstrated

### 1. Variable Existence vs Truthiness (Important Distinction!)

There are **two different semantic operations** for variable checking:

#### Truthiness Check: `<! if <|VARIABLE|> !>`
- Checks if the variable exists **AND** has a truthy value
- Falsy values: `""` (empty string), `false`, `0`, `null`
- Use case: Enable features only when explicitly enabled

#### Existence Check: `<! if defined (<|VARIABLE|>) !>`
- Checks if the variable exists, regardless of its value
- Returns true even for falsy values like `false` or `""`
- Use case: Configure features that may be disabled but still need configuration

**Example showing the difference:**
```xferlang
</ FEATURE_TOGGLE exists but is set to false />
<! if defined (<|FEATURE_TOGGLE|>) !>
featureConfiguration {
    hasToggle ~true
    <! if <|FEATURE_TOGGLE|> !>
    enabledSettings { implementation "full" }
}
```

### 2. Simple Variable Existence Checks
- **Defined Variables**: `<! if <|VARIABLE|> !>` - checks if a variable is defined
- **Undefined Variables**: Tests behavior when variables don't exist

### 3. Equality Comparisons
- **String Comparison**: `<! if eq(<|ACTUAL|> "expected") !>`
- **Numeric Comparison**: `<! if eq(<|NUMBER|> #42) !>`
- **Boolean Comparison**: `<! if eq(<|FLAG|> ~true) !>`

### 4. Numeric Comparisons
- **Greater Than**: `<! if gt(<|VALUE|> #100) !>`
- **Less Than**: `<! if lt(<|SCORE|> #50) !>`
- **Greater/Equal**: `<! if ge(<|AGE|> #18) !>`
- **Less/Equal**: `<! if le(<|TEMP|> #32) !>`

### 5. Complex Conditions (Future)
- **Logical AND**: `<! if and(defined(<|DEBUG|>) eq(<|LOG_LEVEL|> "verbose")) !>`
- **Logical OR**: `<! if or(eq(<|ENV|> "dev") eq(<|ENV|> "test")) !>`
- **Negation**: `<! if not(eq(<|ENV|> "production")) !>`

## Project Structure

```
if-pi-demo/
├── IfPiDemo.csproj           # Project file with dependencies
├── Program.cs                # C# demo showing PI registration and usage
├── README.md                 # This documentation
├── basic-conditions.xfer     # Simple variable existence checks
├── equality-tests.xfer       # String, numeric, boolean equality
├── numeric-comparisons.xfer  # Range and threshold checks
├── mixed-types.xfer          # Different element types as conditions
├── error-conditions.xfer     # Invalid conditions and error handling
└── complex-document.xfer     # Real-world configuration example
```

## Running the Demo

```bash
cd examples/if-pi-demo
dotnet run
```

The program will:

1. **Register the If PI**: Shows how to integrate with the parser's PI system
2. **Process Each File**: Parses and evaluates conditional elements
3. **Display Results**: Shows which conditions were met and how elements were affected
4. **Demonstrate Error Handling**: Shows graceful handling of invalid conditions

## Expected Output

```
=== If Processing Instruction Demo ===

📄 Processing: basic-conditions.xfer
✓ Condition met: <! if <|DEBUG_MODE|> !> → true (variable defined)
✗ Condition failed: <! if <|UNDEFINED_VAR|> !> → false (variable undefined)

📄 Processing: equality-tests.xfer
✓ Condition met: <! if eq["Linux" <|PLATFORM|>] !> → true (strings match)
✗ Condition failed: <! if eq[#100 <|SCORE|>] !> → false (numbers don't match)

📄 Processing: numeric-comparisons.xfer
✓ Condition met: <! if gt[<|PROCESSORS|> #4] !> → true (8 > 4)
✗ Condition failed: <! if lt[<|AGE|> #18] !> → false (25 < 18)

📄 Processing: error-conditions.xfer
✗ Condition failed: <! if invalid_operator[...] !> → false (graceful error handling)

🎯 Demo completed: 12 conditions processed, 7 met, 5 failed
```

## Key Architecture Points

### 1. Hybrid PI-Scripting Integration
The If PI demonstrates how Processing Instructions can delegate to the Scripting system for consistent expression evaluation while providing document-level conditional semantics.

### 2. Dynamic Source Integration
Works seamlessly with dynamic sources (variables) to enable environment-specific configurations.

### 3. Type-Safe Evaluation
Handles different element types appropriately:
- **TextElement**: String comparison
- **NumericElement**: Numeric comparison
- **BooleanElement**: Boolean logic
- **DynamicElement**: Variable resolution
- **CollectionElement**: Operator expressions

### 4. Error Resilience
Invalid conditions gracefully default to `false`, preventing parsing failures while logging appropriate warnings.

### 5. Element State Management
Conditional results are stored in element metadata, enabling:
- Conditional serialization
- Styling based on conditions
- Chaining with other conditional PIs

## Use Cases

### 1. Environment-Specific Configuration
```xferlang
<! if eq["production" <|ENVIRONMENT|>] !>
database {
    server "prod.example.com"
    ssl ~true
}

<! if eq["development" <|ENVIRONMENT|>] !>
database {
    server "localhost"
    ssl ~false
}
```

### 2. Feature Flags
```xferlang
<! if eq[~true <|ENABLE_NEW_FEATURE|>] !>
newFeatureConfig {
    enabled ~true
    rolloutPercentage #100
}
```

### 3. Platform-Specific Settings
```xferlang
<! if eq["Windows" <|PLATFORM|>] !>
windowsSettings {
    pathSeparator "\\"
    homeDir "<|USERPROFILE|>"
}

<! if eq["Linux" <|PLATFORM|>] !>
linuxSettings {
    pathSeparator "/"
    homeDir "<|HOME|>"
}
```

### 4. Resource Scaling Based on System Specs
```xferlang
<! if gt[<|MEMORY_GB|> #16] !>
highMemoryConfig {
    maxCacheSize #2048
    workerThreads #8
}

<! if le[<|MEMORY_GB|> #8] !>
lowMemoryConfig {
    maxCacheSize #256
    workerThreads #2
}
```

## Technical Implementation

The If PI leverages XferLang's existing infrastructure:

- **PI Registration**: Uses `Parser.RegisterPIProcessor()` with factory method
- **Expression Evaluation**: Delegates to `ScriptingEngine.Evaluate()`
- **Operator Integration**: Works with `DefinedOperator`, `EqualsOperator`, etc.
- **Element Processing**: Implements both `ProcessingInstructionHandler()` and `ElementHandler()`
- **State Management**: Stores conditional results in element metadata

This design ensures the If PI integrates seamlessly with XferLang's existing systems while providing powerful conditional document processing capabilities.
