using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;
namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Parses XferLang schema definitions from document elements into structured schema objects.
/// Converts schema syntax into usable schema models for validation and type checking.
/// </summary>
public class XferSchemaParser {
    /// <summary>
    /// Parses a schema object element into a dictionary of schema objects.
    /// Extracts schema definitions and converts them into structured SchemaObject instances.
    /// </summary>
    /// <param name="schemaObjectElement">The object element containing schema definitions to parse.</param>
    /// <returns>A dictionary mapping schema names to their corresponding SchemaObject definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when schemaObjectElement is null.</exception>
    public Dictionary<string, SchemaObject> ParseSchema(ObjectElement schemaObjectElement) {
        if (schemaObjectElement == null) {
            throw new ArgumentNullException(nameof(schemaObjectElement), "Schema object cannot be null.");
        }

        var schemaObjects = new Dictionary<string, SchemaObject>();

        if (schemaObjectElement.TryGetElement("definition", out TupleElement? definitionElement) && definitionElement is { }) {
            foreach (var element in definitionElement.Values) {
#if false
                if (element is KeyValuePairElement schemaKvp) {

                    if (schemaKvp.Value is KeyValuePairElement objectKvp && objectKvp.Value is ObjectElement objectElement) {

                    }

                    // Create a SchemaObject for each object definition
                    var schemaObject = new SchemaObject { Name = objectElement.Name };

                    foreach (var fieldElement in objectElement.Values.Values) {
                        if (fieldElement is KeyValuePairElement kvp) {
                            var schemaField = new SchemaField {
                                Name = kvp.Key,
                                Type = ExtractFieldType(kvp),
                                IsRequired = CheckRequiredField(kvp)
                            };
                            schemaObject.Fields.Add(schemaField.Name, schemaField);
                        }
                    }

                    schemaObjects.Add(schemaObject.Name, schemaObject);
                }
                else {
                    throw new InvalidOperationException($"Expected key/value pair.");
                }
#endif
            }
        }
        else {
            throw new InvalidOperationException("Schema object does not contain a valid 'definition' element.");
        }

        return schemaObjects;
    }

    private string ExtractFieldType(KeyValuePairElement kvp) {
        if (kvp.Value is KeyValuePairElement fieldKvp) {
            if (fieldKvp.Key == "element" && fieldKvp.Value is StringElement typeString) {
                return typeString.Value;
            }
        }
        // Determine the field's type based on its definition
        if (kvp.Value is ObjectElement fieldObject &&
            fieldObject.Dictionary.Values.FirstOrDefault(v => v.Key == "element") is { Value: StringElement typeElement }) {
            return typeElement.Value;
        }

        throw new InvalidOperationException($"Field '{kvp.Key}' is missing a valid 'element' type definition.");
    }

    private bool CheckRequiredField(KeyValuePairElement kvp) {
        // Check for a "constraints" object containing a "required" key
        if (kvp.Value is ObjectElement fieldObject &&
            fieldObject.Dictionary.Values.FirstOrDefault(v => v.Key == "constraints")?.Value is ObjectElement constraints &&
            constraints.Dictionary.Values.FirstOrDefault(v => v.Key == "required")?.Value is BooleanElement boolElement) {
            return boolElement.Value;
        }

        return false;
    }
}
