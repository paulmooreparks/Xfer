# Dynamic DB Resolver Demo

This demo shows how to use XferLang.NET with a custom dynamic source resolver that fetches values from a local Sqlite database using the new PI format (direct key/value pairs in `dynamicSource`).

## How it works
- The demo creates a local `demo.db` Sqlite database and populates it with sample secrets.
- The Xfer document contains a `dynamicSource` PI with keys mapped to `db:` references.
- The custom resolver (`DbDynamicSourceResolver`) looks for `db:` sources and fetches the value from the database.
- The demo prints out the resolved values for `password` and `greeting`.

## Running the demo
1. Build the project:
   ```pwsh
   dotnet build
   ```
2. Run the demo:
   ```pwsh
   dotnet run --project examples/dynamic-db-resolver-demo
   ```

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
