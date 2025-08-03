# External Log PI Demo

This demo showcases how to create a **completely external Processing Instruction** for XferLang without modifying the core library. The Log PI demonstrates the extensibility of the PI registry system.

## What This Demo Shows

1. **External PI Creation**: A custom `LogProcessingInstruction` class that operates independently
2. **PI Registry Integration**: How to register/unregister external PIs with the parser
3. **Element Processing**: How PIs can process their target elements with custom logic
4. **Document-level PI Pattern**: How to properly handle document-level PIs that require manual ElementHandler invocation
5. **Multiple Configurations**: Different log levels, destinations, and formats

## Log PI Features

The Log PI supports various configuration options:

### Log Levels
- `debug` - Gray console output
- `info` - Cyan console output (default)
- `warn` - Yellow console output
- `error` - Red console output

### Destinations
- `console` - Output to console (default)
- `file:<path>` - Output to specified file

### Formats
- `pretty` - Multi-line formatted output with separators (default)
- `json` - JSON format with full element content
- `compact` - Single line with truncated content

### Optional Message
- `message` - Custom message to include in log output

## Usage Examples

### Document-level PIs with Recursive KVP Pattern
```xferlang
<! log { level "info" destination "console" format "pretty" } !>
{
    user_id 123
    name "John Doe"
    email "john@example.com"
}

<! log { level "error" destination { file "logs/errors.log" } format "json" } !>
{ error_code 500 message "Server Error" }
```

### Recursive KVP Configuration Examples
```xferlang
// Simple string values
destination "console"
format "pretty"

// Recursive KVP pattern for complex destinations
destination { file "logs/app.log" }
destination { console }
```

The recursive KVP pattern allows for more structured and extensible configuration. The Log PI automatically resolves:
- `{ file "path/to/file.log" }` → `"file:path/to/file.log"`
- `{ console }` → `"console"`
- `"console"` → `"console"` (simple strings work too)

## Important: Document-level PI Pattern

⚠️ **Key Discovery**: Document-level PIs do not automatically invoke their `ElementHandler` method. You must manually call it after parsing:

```csharp
var parser = new Parser();
LogPiExtension.Register(parser);

var doc = parser.Parse(xferContent);

// Manual ElementHandler invocation for document-level PIs
foreach (var pi in doc.ProcessingInstructions) {
    if (pi is LogProcessingInstruction logPI && pi.Target != null) {
        logPI.ElementHandler(pi.Target);
    }
}
```

This pattern is necessary because the parser sets the `Target` property but doesn't automatically call `ElementHandler` for document-level PIs.

## How It Works

1. **Registration**: `LogPiExtension.Register(parser)` adds the PI to the parser's registry
2. **Factory Method**: Creates `LogProcessingInstruction` instances when `<!log...!>` is encountered
3. **Target Assignment**: Parser automatically sets the PI's target to the following element
4. **Manual Execution**: Application code must invoke `ElementHandler(target)` for document-level PIs
5. **Output**: Logs are written to console or file based on configuration

## Key Architecture Points

- **Zero Core Modifications**: The core library doesn't know about the Log PI
- **Standard PI Interface**: Inherits from `ProcessingInstruction` base class
- **Registry Pattern**: Uses the extensible PI registry for registration
- **Clean Separation**: External assembly with its own logic and dependencies
- **Document-level Support**: Properly handles PIs that target root elements

## Running the Demo

```bash
dotnet run
```

The demo will:
1. Process the included `sample.xfer` file with document-level PIs
2. Create and process in-memory documents
3. Test different log configurations with various levels and formats
4. Create log files in the `logs/` directory
5. Demonstrate the manual ElementHandler invocation pattern

## Files Created

- `LogProcessingInstruction.cs` - The external PI implementation
- `LogPiExtension.cs` - Registry integration helpers (same file)
- `Program.cs` - Demo application showing proper usage patterns
- `sample.xfer` - Example XFER document with document-level log PIs
- `logs/` - Directory for log file output

## Key Learnings

1. **Document-level vs Inline PIs**: Document-level PIs require manual ElementHandler invocation
2. **Target Assignment**: Parser automatically assigns targets but doesn't execute handlers
3. **External Extensibility**: Complete PI functionality can be achieved without core library changes
4. **Registry Integration**: External PIs integrate seamlessly with the built-in PI system
5. **Recursive KVP Pattern**: Supports both simple strings and nested object configurations for maximum flexibility

## Recursive KVP Architecture

The Log PI demonstrates the power of the recursive KVP pattern:

```csharp
// Simple resolution
"console" → "console"

// Recursive resolution
{ file "logs/app.log" } → "file:logs/app.log"
{ console } → "console"
```

This pattern provides:
- **Consistency** with other XferLang PIs like DynamicSource
- **Extensibility** for future destination types
- **Backward Compatibility** with simple string values
- **Structured Configuration** for complex scenarios

This demonstrates that **anyone can create custom PIs** for XferLang without needing access to the core library source code!
