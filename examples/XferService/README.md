# XferLang Web Service Demo

This solution demonstrates how to create a modern ASP.NET Core web service that uses [XferLang](https://xferlang.org/) as an alternative to JSON for data serialization.

## What's New in This Update

### 🔧 **Improved XferService**
- Added proper error handling with `TrySerialize` and `TryDeserialize` methods
- Support for both compact and pretty formatting
- Enhanced type safety and null handling
- Added utility methods for better integration

### 🌐 **Enhanced ASP.NET Core Integration**
- Updated formatters with comprehensive error handling
- Better content negotiation support
- Improved logging and debugging capabilities
- **Standardized on `application/xfer` media type and UTF-8 encoding**

### 📊 **Enhanced Data Models**
- Added validation attributes for proper model validation
- Extended `SampleData` with more complex types (collections, dictionaries)
- Better equality and string representations

### 🎯 **Comprehensive API Endpoints**
- Multiple endpoints demonstrating different XferLang features
- Complex nested data structures
- Proper OpenAPI documentation
- Collection management endpoints

### 📖 **Enhanced Swagger Documentation**
- **Native XferLang examples in Swagger UI** instead of JSON
- Interactive documentation with actual XferLang syntax
- Content-type specific examples and descriptions
- Clear explanations of XferLang benefits

### 👨‍💻 **Improved Client**
- Better error handling and user feedback
- Comprehensive testing including serialization roundtrips
- Proper resource disposal
- Enhanced demo scenarios

## Projects Structure

- **Xfer.Service** - ASP.NET Core web API using XferLang formatters
- **Xfer.Client** - Console application demonstrating client usage
- **Xfer.Data** - Shared data models

## Features Demonstrated

### Data Types Support
- ✅ Primitives (string, int, bool, decimal)
- ✅ Date/Time types (DateTime, TimeOnly, TimeSpan)
- ✅ Enums
- ✅ Collections (List, Array)
- ✅ Dictionaries
- ✅ Nullable types
- ✅ Complex nested objects

### Web Service Features
- ✅ Content negotiation (Xfer vs JSON)
- ✅ Model validation
- ✅ Error handling
- ✅ **Native XferLang examples in Swagger UI**
- ✅ Multiple response formats

## Running the Demo

### 1. Start the Web Servicecd Xfer.Service
dotnet runThe service will start on `https://localhost:7021`

### 2. Run the Client Democd Xfer.Client
dotnet run
### 3. Explore the API
- **Swagger UI**: https://localhost:7021 *(now shows XferLang examples!)*
- **Sample Data**: GET `/api/sampledata`
- **Echo Endpoint**: POST `/api/sampledata`
- **Complex Data**: GET `/api/sampledata/complex`
- **All Data**: GET `/api/sampledata/all`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sampledata` | Get sample data showcasing various types |
| POST | `/api/sampledata` | Echo back posted data (validation demo) |
| GET | `/api/sampledata/all` | Get all stored sample data |
| DELETE | `/api/sampledata/all` | Clear all stored data |
| GET | `/api/sampledata/complex` | Complex nested data structures demo |

## Content Types

The service supports content types with proper negotiation:
- `application/xfer` (primary and standardized)
- `application/json` (fallback)

## Swagger Integration

### XferLang Examples in Swagger UI
The Swagger documentation now shows **actual XferLang syntax** instead of JSON when you select `application/xfer` as the media type:
```xfer
{
  name: "Alice Johnson"
  age: 30
  timeSpan: 28.11:43:56
  timeOnly: 11:43:56
  dateTime: 2021-10-31T12:34:56
  testEnum: Pretty
  salary: 75000.50
  isActive: true
  tags: [
    "employee"
    "senior" 
    "developer"
  ]
  metadata: {
    department: "Engineering"
    startDate: 2020-01-15T00:00:00
    skillLevel: 8.5
    hasRemoteAccess: true
  }
}
```

### How It Works
```csharp
public class XferExampleFilter : IOperationFilter {
    // Automatically generates XferLang examples for:
    // - Response bodies
    // - Request bodies  
    // - Different endpoint types
    // - Complex nested structures
}
```

## XferLang Integration Highlights

### Custom Formatters
```csharp
public class XferInputFormatter : TextInputFormatter
```

```csharp
public class XferOutputFormatter : TextOutputFormatter
```

### Service Layer
```csharp
public class XferService {
    public static string Serialize<T>(T data);
    public static T? Deserialize<T>(string xfer);
    public static bool TrySerialize<T>(T data, out string? result);
    public static bool TryDeserialize<T>(string xfer, out T? result);
}
```

### Content Negotiation
The service automatically selects the appropriate formatter based on:
- `Accept` header in requests
- `Content-Type` header in requests
- Default preference for XferLang format

## Example XferLang Output

### Simple Object
```xfer
{
  name: "Alice Johnson"
  age: 30
  timeSpan: 28.11:43:56
  timeOnly: 11:43:56
  dateTime: 2021-10-31T12:34:56
  testEnum: Pretty
  salary: 75000.50
  isActive: true
  tags: [
    "employee"
    "senior" 
    "developer"
  ]
  metadata: {
    department: "Engineering"
    startDate: 2020-01-15T00:00:00
    skillLevel: 8.5
    hasRemoteAccess: true
  }
}
```
### Complex Nested Structure
```xfer
{
  message: "XferLang Complex Data Demo"
  timestamp: 2024-03-15T12:00:00
  numbers: [ 1 2 3 5 8 13 21 ]
  nestedObject: {
    level1: {
      level2: {
        value: "Deep nesting works!"
      }
    }
  }
  nullableValues: {
    hasValue: 42
    isNull: null
    defaultDecimal: null
  }
  enums: [ None Indented Spaced Pretty ]
  booleanTests: {
    trueValue: true
    falseValue: false
  }
}
```

### Complex Nested Structure
```xfer
{
  message: "XferLang Complex Data Demo"
  timestamp: 2024-03-15T12:00:00
  numbers: [ 1 2 3 5 8 13 21 ]
  nestedObject: {
    level1: {
      level2: {
        value: "Deep nesting works!"
      }
    }
  }
  nullableValues: {
    hasValue: 42
    isNull: null
    defaultDecimal: null
  }
  enums: [ None Indented Spaced Pretty ]
  booleanTests: {
    trueValue: true
    falseValue: false
  }
}
```

## Key Improvements Made

1. **Error Resilience**: Comprehensive error handling throughout the pipeline
2. **Type Safety**: Better null handling and type constraints
3. **Performance**: Efficient serialization with minimal allocations
4. **Logging**: Detailed logging for debugging and monitoring
5. **Documentation**: Complete API documentation with **native XferLang examples**
6. **Testing**: Built-in roundtrip testing in the client
7. **Validation**: Proper model validation with meaningful error messages
8. **Standards Compliance**: Following ASP.NET Core best practices
9. **Media Type Standardization**: Clean `application/xfer` + UTF-8 only

This implementation showcases XferLang as a practical alternative to JSON for modern web APIs, providing better developer experience, cleaner documentation, and more efficient data interchange.
