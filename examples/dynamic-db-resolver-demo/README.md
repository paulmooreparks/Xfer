# Dynamic DB Resolver Demo with New PI Registry

This demo showcases the **new extensible Processing Instruction (PI) registry system** in XferLang.NET. It demonstrates how custom processing instructions can be registered and used with the new registry-based architecture.

## What's New

- **✅ Extensible PI Registry**: Custom PIs can now be registered using `RegisterPIProcessor()`
- **✅ DynamicSourceProcessingInstruction**: A new built-in PI for configuring dynamic source resolution
- **✅ Registry-based Architecture**: Replaces hard-coded PI parsing with a flexible dictionary-based system

## How It Works

### 1. PI Registration

The `DynamicSourceProcessingInstruction` is automatically registered in the parser using the new registry system:

```csharp
// In Parser constructor - this happens automatically now!
RegisterPIProcessor(DynamicSourceProcessingInstruction.Keyword, CreateDynamicSourceProcessingInstruction);
```

### 2. Sample Xfer Document

The demo uses a `dynamicSource` PI with recursive KVPs to configure how dynamic elements are resolved:

```xferlang
(
    <!
    dynamicSource {
        dbpassword db "dbpassword"
        greeting const "This is a test greeting from PI config."
        username env "USERNAME"
        apikey db "apikey"
    }
    !>

    credentials {
        password '<|dbpassword|>'
        greeting '<|greeting|>'
        username '<|username|>'
        apikey '<|apikey|>'
    }
)
```

### 3. Custom Resolver

The `DbDynamicSourceResolver` searches for `DynamicSourceProcessingInstruction` instances and processes their recursive KVP configuration:

- `key db "dbkey"` - Fetch value from SQLite database using "dbkey"
- `key env "ENVVAR"` - Fetch value from environment variable "ENVVAR"
- `key const "value"` - Use constant "value"
- Direct string - Use as literal value

## Key Features Demonstrated

1. **Automatic PI Registration**: The new registry automatically handles `dynamicSource` PIs
2. **PI Serialization**: Processing instructions are correctly included in ToXfer() output
3. **Dynamic Resolution**: Dynamic elements are resolved according to PI configuration
4. **Extensibility**: Shows how the new system supports custom PI types

## Running the Demo

```bash
cd examples/dynamic-db-resolver-demo
dotnet run
```

## Expected Output

```
=== Dynamic DB Resolver Demo with New PI Registry ===
This demo shows the new extensible Processing Instruction registry system.
The 'dynamicSource' PI uses recursive KVPs: key sourceType "sourceValue"!

Setting up demo database...
  Inserted: dbpassword = SuperSecretFromDB!
  ...

DynamicSource PIs found: 1 (via new PI registry!)
✓ Found DynamicSource PI: dynamicSource
  dbpassword db "dbpassword"
  greeting const "This is a test greeting from PI config."
  ...

✓ Demo completed successfully!
✓ DynamicSourceProcessingInstruction was created via the new PI registry
✓ PI was correctly serialized in the output
✓ Dynamic element resolution worked as expected
```

## Technical Details

- **PI Registry**: Uses `Dictionary<string, PIProcessor>` for O(1) lookup
- **Factory Pattern**: PI creation uses factory methods for consistency
- **Backward Compatibility**: All existing PI functionality preserved
- **Extensible Design**: Easy to add new custom PIs

This demo proves that the new PI registry system is working correctly and provides a foundation for users to create their own custom processing instructions.

## Example output
```
Resolved password: SuperSecretFromDB!
Resolved greeting: Hello from the DB!
```

## Key files
- `Program.cs`: Main demo logic and custom resolver
- `dynamic-db-resolver-demo.csproj`: Project file with dependencies

## Requirements
- .NET 6+
- Microsoft.Data.Sqlite
- ParksComputing.Xfer.Lang
