using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using Xfer.Service.Services;
using Xfer.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;

namespace Xfer.Service;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
        builder.Services.AddSingleton<XferInputFormatter>();
        builder.Services.AddSingleton<XferOutputFormatter>();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "Xfer API", 
                Version = "v1",
                Description = "A REST API using XferLang as an alternative to JSON"
            });
            c.OperationFilter<XferExampleFilter>();
            c.SchemaFilter<XferSchemaFilter>();
            c.DocumentFilter<XferDocumentFilter>();
        });
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xfer API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        
        // Middleware to set default Accept header for Xfer content
        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.ContainsKey("Accept") || 
                context.Request.Headers.Accept.ToString() == "*/*") {
                context.Request.Headers["Accept"] = "application/xfer";
            }
            await next();
        });
        
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

public class ConfigureMvcOptions : IConfigureOptions<MvcOptions> {
    private readonly XferInputFormatter _xferInputFormatter;
    private readonly XferOutputFormatter _xferOutputFormatter;

    public ConfigureMvcOptions(XferInputFormatter xferInputFormatter, XferOutputFormatter xferOutputFormatter) {
        _xferInputFormatter = xferInputFormatter;
        _xferOutputFormatter = xferOutputFormatter;
    }

    public void Configure(MvcOptions options) {
        // Insert Xfer formatters at the beginning to prioritize them
        options.InputFormatters.Insert(0, _xferInputFormatter);
        options.OutputFormatters.Insert(0, _xferOutputFormatter);
        
        // Configure model binding
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
    }
}

public class XferDocumentFilter : IDocumentFilter {
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
        // Post-process the document to ensure XferLang examples are properly set
        foreach (var path in swaggerDoc.Paths.Values) {
            foreach (var operation in path.Operations.Values) {
                // Process responses
                foreach (var response in operation.Responses.Values) {
                    if (response.Content?.ContainsKey("application/xfer") == true) {
                        var xferContent = response.Content["application/xfer"];
                        if (xferContent.Example != null) {
                            // Force the example to be treated as a string, not parsed JSON
                            var exampleValue = xferContent.Example.ToString();
                            if (!string.IsNullOrEmpty(exampleValue)) {
                                xferContent.Example = new OpenApiString(exampleValue.Trim('"'));
                            }
                        }
                    }
                }
                
                // Process request body
                if (operation.RequestBody?.Content?.ContainsKey("application/xfer") == true) {
                    var xferContent = operation.RequestBody.Content["application/xfer"];
                    if (xferContent.Example != null) {
                        var exampleValue = xferContent.Example.ToString();
                        if (!string.IsNullOrEmpty(exampleValue)) {
                            xferContent.Example = new OpenApiString(exampleValue.Trim('"'));
                        }
                    }
                }
            }
        }
    }
}

public class XferSchemaFilter : ISchemaFilter {
    public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
        // For types that we want to show as XferLang examples, 
        // we'll let the operation filter handle the examples
        // This filter ensures clean schema definitions
        if (context.Type == typeof(SampleData) || 
            context.Type == typeof(List<SampleData>) || 
            context.Type == typeof(IEnumerable<SampleData>)) {
            
            // Add XferLang format hint to description
            if (!string.IsNullOrEmpty(schema.Description)) {
                schema.Description += " (Supports XferLang format for better readability)";
            } else {
                schema.Description = "Supports XferLang format for better readability";
            }
        }
    }
}

public class XferExampleFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        // Add Xfer examples for responses
        foreach (var response in operation.Responses.Values) {
            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            
            // Clear existing content to avoid conflicts
            if (response.Content.ContainsKey("application/xfer")) {
                response.Content.Remove("application/xfer");
            }
            
            // Add Xfer content type with XferLang example
            var xferMediaType = new OpenApiMediaType();
            
            // Get the response type and create XferLang example
            if (TryGetResponseType(context, out var responseType)) {
                var exampleObject = CreateExampleObject(responseType, context.MethodInfo.Name);
                if (exampleObject != null) {
                    var xferExample = XferConvert.Serialize(exampleObject, Formatting.Pretty);
                    
                    // Try using a raw string approach to prevent JSON interpretation
                    var rawExample = new OpenApiString(xferExample);
                    
                    xferMediaType.Example = rawExample;
                    xferMediaType.Examples = new Dictionary<string, OpenApiExample> {
                        ["xferExample"] = new OpenApiExample {
                            Summary = "XferLang Format",
                            Description = "Example in XferLang format - notice the cleaner syntax without quotes",
                            Value = rawExample
                        }
                    };
                    
                    // Use a very simple schema to avoid JSON schema interference
                    xferMediaType.Schema = new OpenApiSchema {
                        Type = "string",
                        Description = $"Data in XferLang format. XferLang is more readable than JSON and has native support for .NET types like TimeSpan, TimeOnly, and enums.\n\nExample:\n{xferExample}",
                        Example = rawExample
                    };
                }
            }
            
            // Insert at the beginning to prioritize Xfer
            var newContent = new Dictionary<string, OpenApiMediaType> {
                ["application/xfer"] = xferMediaType
            };
            
            // Add existing content after Xfer
            foreach (var kvp in response.Content) {
                if (kvp.Key != "application/xfer") {
                    newContent[kvp.Key] = kvp.Value;
                }
            }
            
            response.Content = newContent;
        }
        
        // Add Xfer examples for request bodies
        if (operation.RequestBody?.Content != null) {
            // Clear existing Xfer content
            if (operation.RequestBody.Content.ContainsKey("application/xfer")) {
                operation.RequestBody.Content.Remove("application/xfer");
            }
            
            var xferMediaType = new OpenApiMediaType();
            
            // Get the request body type and create XferLang example
            if (TryGetRequestBodyType(context, out var requestType)) {
                var exampleObject = CreateExampleObject(requestType, context.MethodInfo.Name);
                if (exampleObject != null) {
                    var xferExample = XferConvert.Serialize(exampleObject, Formatting.Pretty);
                    
                    var rawExample = new OpenApiString(xferExample);
                    
                    xferMediaType.Example = rawExample;
                    xferMediaType.Examples = new Dictionary<string, OpenApiExample> {
                        ["xferExample"] = new OpenApiExample {
                            Summary = "XferLang Request",
                            Description = "Send data using XferLang format",
                            Value = rawExample
                        }
                    };
                    
                    xferMediaType.Schema = new OpenApiSchema {
                        Type = "string",
                        Description = $"Send data in XferLang format - cleaner syntax than JSON.\n\nExample:\n{xferExample}",
                        Example = rawExample
                    };
                }
            }
            
            // Insert at the beginning to prioritize Xfer
            var newRequestContent = new Dictionary<string, OpenApiMediaType> {
                ["application/xfer"] = xferMediaType
            };
            
            // Add existing content after Xfer
            foreach (var kvp in operation.RequestBody.Content) {
                if (kvp.Key != "application/xfer") {
                    newRequestContent[kvp.Key] = kvp.Value;
                }
            }
            
            operation.RequestBody.Content = newRequestContent;
        }
    }
    
    private bool TryGetResponseType(OperationFilterContext context, out Type responseType) {
        responseType = null!;
        
        // Check for ActionResult<T> or Task<ActionResult<T>>
        var returnType = context.MethodInfo.ReturnType;
        
        // Handle Task<ActionResult<T>>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
            returnType = returnType.GetGenericArguments()[0];
        }
        
        // Handle ActionResult<T>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>)) {
            responseType = returnType.GetGenericArguments()[0];
            return true;
        }
        
        // Handle IActionResult with ProducesResponseType attribute
        var producesAttribute = context.MethodInfo
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>()
            .FirstOrDefault(attr => attr.StatusCode == 200);
            
        if (producesAttribute?.Type != null) {
            responseType = producesAttribute.Type;
            return true;
        }
        
        return false;
    }
    
    private bool TryGetRequestBodyType(OperationFilterContext context, out Type requestType) {
        requestType = null!;
        
        var parameters = context.MethodInfo.GetParameters();
        var bodyParameter = parameters.FirstOrDefault(p => 
            p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute), false).Any());
        
        if (bodyParameter != null) {
            requestType = bodyParameter.ParameterType;
            return true;
        }
        
        return false;
    }
    
    private object? CreateExampleObject(Type type, string methodName) {
        // Create different examples based on the endpoint
        if (type == typeof(SampleData)) {
            return methodName.ToLower() switch {
                "postsampledata" => new SampleData {
                    Name = "John Doe",
                    Age = 28,
                    TimeSpan = new TimeSpan(1, 2, 30, 45), // 1 day, 2:30:45
                    TimeOnly = new TimeOnly(14, 30, 0), // 2:30 PM
                    DateTime = new DateTime(2024, 3, 15, 10, 0, 0),
                    TestEnum = TestEnum.Pretty,
                    Salary = 65000.00m,
                    IsActive = true,
                    Tags = new List<string> { "new-hire", "developer", "remote" },
                    Metadata = new Dictionary<string, object> {
                        { "team", "Backend" },
                        { "location", "Remote" },
                        { "experience", 3 }
                    }
                },
                _ => new SampleData {
                    Name = "Alice Johnson",
                    Age = 30,
                    TimeSpan = new TimeSpan(28, 11, 43, 56), // 28 days, 11:43:56
                    TimeOnly = new TimeOnly(11, 43, 56),
                    DateTime = new DateTime(2021, 10, 31, 12, 34, 56),
                    TestEnum = TestEnum.Pretty,
                    Salary = 75000.50m,
                    IsActive = true,
                    Tags = new List<string> { "employee", "senior", "developer" },
                    Metadata = new Dictionary<string, object> {
                        { "department", "Engineering" },
                        { "startDate", new DateTime(2020, 1, 15) },
                        { "skillLevel", 8.5 },
                        { "hasRemoteAccess", true }
                    }
                }
            };
        }
        
        // Handle List<SampleData>
        if (type == typeof(List<SampleData>) || type == typeof(IEnumerable<SampleData>)) {
            return new List<SampleData> {
                new SampleData {
                    Name = "Alice Johnson",
                    Age = 30,
                    TimeSpan = new TimeSpan(28, 11, 43, 56),
                    TimeOnly = new TimeOnly(11, 43, 56),
                    DateTime = new DateTime(2021, 10, 31, 12, 34, 56),
                    TestEnum = TestEnum.Pretty,
                    Salary = 75000.50m,
                    IsActive = true,
                    Tags = new List<string> { "employee", "senior", "developer" }
                },
                new SampleData {
                    Name = "Bob Smith",
                    Age = 25,
                    TimeSpan = new TimeSpan(15, 8, 20, 10),
                    TimeOnly = new TimeOnly(9, 15, 30),
                    DateTime = new DateTime(2022, 5, 20, 14, 22, 18),
                    TestEnum = TestEnum.Indented,
                    Salary = 55000.00m,
                    IsActive = false,
                    Tags = new List<string> { "junior", "frontend" }
                }
            };
        }
        
        // Handle complex anonymous objects (for /complex endpoint)
        if (type == typeof(object) || type.Name.Contains("AnonymousType")) {
            return new {
                Message = "XferLang Complex Data Demo",
                Timestamp = new DateTime(2024, 3, 15, 12, 0, 0),
                Numbers = new[] { 1, 2, 3, 5, 8, 13, 21 },
                NestedObject = new {
                    Level1 = new {
                        Level2 = new {
                            Value = "Deep nesting works!"
                        }
                    }
                },
                NullableValues = new {
                    HasValue = (int?)42,
                    IsNull = (string?)null,
                    DefaultDecimal = (decimal?)null
                },
                Enums = new[] { TestEnum.None, TestEnum.Indented, TestEnum.Spaced, TestEnum.Pretty },
                BooleanTests = new {
                    TrueValue = true,
                    FalseValue = false
                }
            };
        }
        
        return null;
    }
}

public class XferInputFormatter : TextInputFormatter {
    private readonly ILogger<XferInputFormatter> _logger;

    public XferInputFormatter(ILogger<XferInputFormatter> logger) {
        _logger = logger;
        // XferLang standard media type
        SupportedMediaTypes.Add("application/xfer");
        // XferLang uses UTF-8 encoding
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanReadType(Type type) => type != null;

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding) {
        var request = context.HttpContext.Request;
        
        try {
            using var reader = new StreamReader(request.Body, encoding);
            var content = await reader.ReadToEndAsync();
            
            if (string.IsNullOrWhiteSpace(content)) {
                _logger.LogWarning("Empty request body received");
                return await InputFormatterResult.FailureAsync();
            }

            _logger.LogDebug("Deserializing Xfer content: {Content}", content);

            // Use XferService for improved error handling
            var result = XferService.Deserialize(content, context.ModelType);
            return await InputFormatterResult.SuccessAsync(result);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to deserialize Xfer content");
            context.ModelState.AddModelError(context.ModelName, $"Invalid Xfer format: {ex.Message}");
            return await InputFormatterResult.FailureAsync();
        }
    }
}

public class XferOutputFormatter : TextOutputFormatter {
    private readonly ILogger<XferOutputFormatter> _logger;
    
    public XferOutputFormatter(ILogger<XferOutputFormatter> logger) {
        _logger = logger;
        // XferLang standard media type
        SupportedMediaTypes.Add("application/xfer");
        // XferLang uses UTF-8 encoding
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => type != null;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding) {
        var response = context.HttpContext.Response;
        
        try {
            // Use XferService for consistent serialization
            if (XferService.TrySerialize(context.Object, out var content)) {
                _logger.LogDebug("Serialized response to Xfer: {Content}", content);
                await response.WriteAsync(content!, selectedEncoding);
            } else {
                _logger.LogError("Failed to serialize object to Xfer format");
                response.StatusCode = 500;
                await response.WriteAsync("Internal server error: Failed to serialize response", selectedEncoding);
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error writing Xfer response");
            if (!response.HasStarted) {
                response.StatusCode = 500;
                await response.WriteAsync("Internal server error", selectedEncoding);
            }
        }
    }
}
