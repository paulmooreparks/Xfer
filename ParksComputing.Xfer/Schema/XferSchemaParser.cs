using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Elements;
namespace ParksComputing.Xfer.Schema;

public class XferSchemaParser {
    public Dictionary<string, SchemaObject> ParseSchema(ObjectElement schemaObjectElement) {
        if (schemaObjectElement == null) {
            throw new ArgumentNullException(nameof(schemaObjectElement), "Schema object cannot be null.");
        }

        var schemaObjects = new Dictionary<string, SchemaObject>();

        // Locate the "definition" key within the schema object
        var definitionElement = schemaObjectElement.Values.Values.FirstOrDefault(kvp => kvp.Key == "definition")?.Value;

        if (definitionElement is not ArrayElement definitions) {
            throw new InvalidOperationException("Schema object does not contain a valid 'definition' array.");
        }

        foreach (var element in definitions.Values) {
            if (element is ObjectElement objectElement) {
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
                throw new InvalidOperationException("Definitions must contain only object elements.");
            }
        }

        return schemaObjects;
    }

    private string ExtractFieldType(KeyValuePairElement kvp) {
        // Determine the field's type based on its definition
        if (kvp.Value is ObjectElement fieldObject &&
            fieldObject.Values.Values.FirstOrDefault(v => v.Key == "element") is { Value: StringElement typeElement }) {
            return typeElement.Value;
        }

        throw new InvalidOperationException($"Field '{kvp.Key}' is missing a valid 'element' type definition.");
    }

    private bool CheckRequiredField(KeyValuePairElement kvp) {
        // Check for a "constraints" object containing a "required" key
        if (kvp.Value is ObjectElement fieldObject &&
            fieldObject.Values.Values.FirstOrDefault(v => v.Key == "constraints")?.Value is ObjectElement constraints &&
            constraints.Values.Values.FirstOrDefault(v => v.Key == "required")?.Value is BooleanElement boolElement) {
            return boolElement.Value;
        }

        return false;
    }
}
