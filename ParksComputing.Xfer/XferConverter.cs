using System;
using System.Collections.Generic;
using System.Reflection;

using ParksComputing.Xfer.Attributes;
using ParksComputing.Xfer.Models.Elements;
using ParksComputing.Xfer.Services;

public class XferConverter {
    public static string Serialize(object o) {
        var type = o.GetType();
        var obj = new ObjectElement();

        foreach (var property in type.GetProperties()) {
            var attribute = property.GetCustomAttribute<XferPropertyAttribute>();
            var literalAttribute = property.GetCustomAttribute<XferLiteralAttribute>();

            var name = attribute?.Name ?? property.Name;
            var value = property.GetValue(o);

            if (value != null) {
                if (literalAttribute != null) {
                    obj.AddOrUpdate(new KeyValuePairElement(new KeywordElement(name), new EvaluatedElement(value.ToString() ?? string.Empty)));
                }
                else {
                    Element element = SerializeValue(value);
                    obj.AddOrUpdate(new KeyValuePairElement(new KeywordElement(name), element));
                }
            }
        }

        return obj.ToString();
    }

    private static Element SerializeValue(object value) {
        if (value is Array arrayValue) {
            var arrayElement = new ArrayElement();
            foreach (var item in arrayValue) {
                if (item != null) {
                    var element = SerializeValue(item);
                    arrayElement.Add(element);
                }
                else {
                    // Handle null items, if needed
                    arrayElement.Add(new EmptyElement());
                }
            }
            return arrayElement;
        }

        return value switch {
            int intValue => new IntegerElement(intValue),
            long longValue => new LongElement(longValue),
            bool boolValue => new BooleanElement(boolValue),
            double doubleValue => new DoubleElement(doubleValue),
            decimal decimalValue => new DecimalElement(decimalValue),
            DateTime dateTimeValue => new DateElement(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss")),
            string stringValue => new StringElement(stringValue),
            IEnumerable<object> list => new PropertyBagElement(list.Select(SerializeValue)),
            object objectValue => new EvaluatedElement(objectValue.ToString() ?? string.Empty),
            _ => throw new NotSupportedException($"Type '{value.GetType().Name}' is not supported")
        };
    }

    public static T Deserialize<T>(string xfer) where T : new() {
        var obj = new T();
        var type = typeof(T);
        var properties = type.GetProperties();
        var propertyMap = new Dictionary<string, PropertyInfo>();

        foreach (var property in properties) {
            var attribute = property.GetCustomAttribute<XferPropertyAttribute>();
            var name = attribute?.Name ?? property.Name;
            propertyMap[name] = property;
        }

        var parser = new Parser();
        var document = parser.Parse(xfer);

        var first = document.Root.Values.First();

        if (first is ObjectElement propertyBag) {
            foreach (var element in propertyBag.Values) {
                if (propertyMap.TryGetValue(element.Key, out var property)) {
                    object? value = DeserializeValue(element.Value.Value, property.PropertyType);
                    property.SetValue(obj, value);
                }
            }
        }

        return obj;
    }

    private static object? DeserializeValue(Element element, Type targetType) {
        if (targetType.IsArray) {
            if (element is ArrayElement arrayElement) {
                var elementType = targetType.GetElementType();
                if (elementType == null) {
                    throw new InvalidOperationException($"Unable to determine element type for array.");
                }

                var values = new List<object?>();
                foreach (var item in arrayElement.Values) {
                    values.Add(DeserializeValue(item, elementType));
                }

                var array = Array.CreateInstance(elementType, values.Count);
                for (int i = 0; i < values.Count; i++) {
                    array.SetValue(values[i], i);
                }

                return array;
            }
            else {
                throw new InvalidOperationException($"Expected ArrayElement for array deserialization.");
            }
        }

        if (typeof(IEnumerable<object>).IsAssignableFrom(targetType)) {
            if (element is PropertyBagElement propertyBagElement) {
                var values = new List<object?>();
                foreach (var item in propertyBagElement.Values) {
                    values.Add(DeserializeValue(item, typeof(object)));
                }
                return values;
            }
            else {
                throw new InvalidOperationException($"Expected PropertyBagElement for IEnumerable deserialization.");
            }
        }

        return element switch {
            IntegerElement intElement when targetType == typeof(int) => intElement.Value,
            IntegerElement intElement when targetType == typeof(object) => intElement.Value,
            LongElement longElement when targetType == typeof(long) => longElement.Value,
            LongElement longElement when targetType == typeof(object) => longElement.Value,
            BooleanElement boolElement when targetType == typeof(bool) => boolElement.Value,
            BooleanElement boolElement when targetType == typeof(object) => boolElement.Value,
            DoubleElement doubleElement when targetType == typeof(double) => doubleElement.Value,
            DoubleElement doubleElement when targetType == typeof(object) => doubleElement.Value,
            DecimalElement decimalElement when targetType == typeof(decimal) => decimalElement.Value,
            DecimalElement decimalElement when targetType == typeof(object) => decimalElement.Value,
            DateElement dateElement when targetType == typeof(DateTime) => dateElement.Value,
            DateElement dateElement when targetType == typeof(object) => dateElement.Value,
            StringElement stringElement when targetType == typeof(string) => stringElement.Value,
            StringElement stringElement when targetType == typeof(object) => stringElement.Value,
            EvaluatedElement literalElement when targetType == typeof(string) => literalElement.Value,
            EvaluatedElement literalElement when targetType == typeof(object) => literalElement.Value,
            _ => throw new NotSupportedException($"Type '{targetType.Name}' is not supported for deserialization")
        };
    }
}
