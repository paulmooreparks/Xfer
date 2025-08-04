using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Validates XferLang documents against defined schemas. Performs structural validation,
/// type checking, constraint evaluation, and rule enforcement to ensure documents
/// conform to their specified schema definitions.
/// </summary>
public class XferSchemaValidator {
    private readonly Dictionary<string, SchemaObject> _schemaObjects;

    public XferSchemaValidator(Dictionary<string, SchemaObject> schemaObjects) {
        _schemaObjects = schemaObjects;
    }

    public void Validate(TupleElement document) {
#if false
        if (!_schemaObjects.TryGetValue(schemaName, out var schemaObject)) {
            throw new InvalidOperationException($"Schema '{schemaName}' not found.");
        }

        foreach (var element in document.Values) {
            if (element is ObjectElement obj) {
                ValidateObject(obj, schemaObject);
            }
        }
#endif
    }

    private void ValidateObject(ObjectElement obj, SchemaObject schemaObject) {
        foreach (var field in schemaObject.Fields) {
            if (field.Value.IsRequired && !obj.Dictionary.Any(v => v.Key == field.Key)) {
                throw new InvalidOperationException($"Field '{field.Key}' is required in object '{schemaObject.Name}'.");
            }

            if (obj.Dictionary.Values.FirstOrDefault(v => v.Key == field.Key) is { } fieldValue) {
                ValidateField(fieldValue, field.Value);
            }
        }
    }

    private void ValidateField(KeyValuePairElement fieldValue, SchemaField schemaField) {
        if (schemaField.Type != fieldValue.Value.Name) {
            throw new InvalidOperationException($"Field '{schemaField.Name}' has an invalid type. Expected '{schemaField.Type}', got '{fieldValue.Value.GetType().Name}'.");
        }

        if (fieldValue.Value is ListElement listElement) {
            schemaField.CustomValidation?.Invoke(listElement);
        }
    }
}
