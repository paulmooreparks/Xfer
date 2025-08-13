using Cliffer;
using System.Collections.Generic;
using System.Net.Http;
using NJsonSchema.Validation;
using NJsonSchema;


namespace Json2Xfer;

[RootCommand("Convert JSON to XferLang format")]
[Argument(typeof(string), "input", "Input JSON file (default: stdin)")]
[Argument(typeof(string), "output", "Output Xfer file (default: stdout)")]
[Option(typeof(bool), "--pretty", "Pretty-print Xfer output", ["-p"])]
[Option(typeof(bool), "--compact", "Compact output (minimize whitespace)", ["-c"])]
[Option(typeof(bool), "--strict", "Strict mode: fail on conversion errors", ["-s"])]
[Option(typeof(bool), "--lenient", "Lenient mode: best-effort conversion", ["-l"])]
[Option(typeof(string), "--root", "Set root element name for top-level object", ["-r"])]
[Option(typeof(string), "--comment", "Inject a comment")]
[Option(typeof(bool), "--prefer-int32", "Prefer 32-bit integers if value fits", ["-i"])]
[Option(typeof(string), "--schema", "Validate input JSON against a JSON Schema (file path or URL)", ["-S"])]
internal class Json2XferCommand {
    public int Execute(
            string input,
            string output,
            bool pretty,
            bool compact,
            bool strict,
            bool lenient,
            bool preferInt32,
            string? schema,
            string? root,
            string? comment
        )
    {
        Console.WriteLine("Starting JSON to XferLang conversion...");
        // 1. Read JSON input
        string json;
        if (!string.IsNullOrEmpty(input)) {
            Console.WriteLine($"Reading JSON from file: {input}");
            json = File.ReadAllText(input);
        }
        else {
            using var sr = new StreamReader(Console.OpenStandardInput());
            json = sr.ReadToEnd();
        }

        // 2. Parse JSON
        Newtonsoft.Json.Linq.JToken jtoken;
        try {
            jtoken = Newtonsoft.Json.Linq.JToken.Parse(json);
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error parsing JSON: {ex.Message}");
            return 1;
        }

        // 2b. Optional JSON Schema validation
        if (!string.IsNullOrWhiteSpace(schema)) {
            try {
                JsonSchema? js;
                if (Uri.TryCreate(schema, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                    // Fetch schema from URL
                    using var http = new HttpClient();
                    var schemaJson = http.GetStringAsync(uri).GetAwaiter().GetResult();
                    js = JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
                }
                else {
                    if (!File.Exists(schema)) {
                        Console.Error.WriteLine($"Schema file not found: {schema}");
                        return 1;
                    }
                    var schemaJson = File.ReadAllText(schema);
                    js = JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
                }

                var validationErrors = js.Validate(json);
                if (validationErrors != null && validationErrors.Count > 0) {
                    Console.Error.WriteLine($"Schema validation failed ({validationErrors.Count} errors):");
                    foreach (var err in validationErrors) {
                        var path = string.IsNullOrWhiteSpace(err.Path) ? "<root>" : err.Path;
                        var message = err.ToString();
                        Console.Error.WriteLine($" - {path}: {message}");
                    }
                    if (strict) {
                        return 3;
                    }
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error validating against schema: {ex.Message}");
                if (strict) {
                    return 3;
                }
            }
        }

        // 3. Convert to XferLang (convert JToken -> plain .NET first)
        ParksComputing.Xfer.Lang.Elements.Element? xferElement = null;
        try {
            var plain = ConvertJTokenToNet(jtoken, preferInt32);
            xferElement = ParksComputing.Xfer.Lang.XferConvert.SerializeValue(plain);
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error converting to XferLang: {ex.Message}");
            if (strict) {
                return 2;
            }
        }
        if (xferElement == null) {
            Console.Error.WriteLine("Conversion produced no output.");
            return 2;
        }

        // 4. Formatting
        var formatting = ParksComputing.Xfer.Lang.Formatting.None;

        if (pretty) {
            formatting |= ParksComputing.Xfer.Lang.Formatting.Indented;
        }
        // 'Compact' formatting is not defined in the API; only use valid options

    string xferText = xferElement.ToXfer(formatting);

        // 5. Output
        if (!string.IsNullOrEmpty(output)) {
            File.WriteAllText(output, xferText);
        }
        else {
            Console.WriteLine(xferText);
        }

        return 0;
    }

    private static object? ConvertJTokenToNet(Newtonsoft.Json.Linq.JToken token, bool preferInt32) {
        switch (token.Type) {
            case Newtonsoft.Json.Linq.JTokenType.Object:
                var obj = (Newtonsoft.Json.Linq.JObject)token;
                var dict = new Dictionary<string, object?>();
                foreach (var prop in obj.Properties()) {
                    dict[prop.Name] = ConvertJTokenToNet(prop.Value, preferInt32);
                }
                return dict;
            case Newtonsoft.Json.Linq.JTokenType.Array:
                var arr = (Newtonsoft.Json.Linq.JArray)token;
                var list = new List<object?>();
                foreach (var item in arr) {
                    list.Add(ConvertJTokenToNet(item, preferInt32));
                }
                return list;
            case Newtonsoft.Json.Linq.JTokenType.Integer:
                var raw = ((Newtonsoft.Json.Linq.JValue)token).Value;
                if (raw is long l && preferInt32 && l >= int.MinValue && l <= int.MaxValue) {
                    return (int)l;
                }
                return raw;
            case Newtonsoft.Json.Linq.JTokenType.Float:
            case Newtonsoft.Json.Linq.JTokenType.String:
            case Newtonsoft.Json.Linq.JTokenType.Boolean:
                return ((Newtonsoft.Json.Linq.JValue)token).Value;
            case Newtonsoft.Json.Linq.JTokenType.Null:
            case Newtonsoft.Json.Linq.JTokenType.Undefined:
                return null;
            case Newtonsoft.Json.Linq.JTokenType.Date:
            case Newtonsoft.Json.Linq.JTokenType.Guid:
            case Newtonsoft.Json.Linq.JTokenType.TimeSpan:
                return ((Newtonsoft.Json.Linq.JValue)token).Value;
            default:
                // Fallback to string representation
                return ((Newtonsoft.Json.Linq.JValue)token).Value;
        }
    }
}

