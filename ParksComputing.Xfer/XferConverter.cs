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
            var name = attribute?.Name ?? property.Name;
            var value = property.GetValue(o);

            if (value != null) {
                Element element = SerializeValue(value);
                obj.AddOrUpdate(new KeyValuePairElement(new KeywordElement(name), element));
            }
        }

        return obj.ToString();
    }

    private static Element SerializeValue(object value) {
        return value switch {
            int intValue => new IntegerElement(intValue),
            long longValue => new LongIntegerElement(longValue),
            bool boolValue => new BooleanElement(boolValue),
            float floatValue => new FloatElement(floatValue),
            double doubleValue => new DoubleElement(doubleValue),
            decimal decimalValue => new DecimalElement(decimalValue),
            DateTime dateTimeValue => new DateElement(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss")),
            string stringValue => new StringElement(stringValue),
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
                    object? value = DeserializeValue(element.Value.Item2, property.PropertyType);
                    property.SetValue(obj, value);
                }
            }
        }


        return obj;
    }

    private static object? DeserializeValue(Element element, Type targetType) {
        return element switch {
            IntegerElement intElement when targetType == typeof(int) => intElement.Value,
            LongIntegerElement longElement when targetType == typeof(long) => longElement.Value,
            BooleanElement boolElement when targetType == typeof(bool) => boolElement.Value,
            FloatElement floatElement when targetType == typeof(float) => (float)floatElement.Value,
            DoubleElement doubleElement when targetType == typeof(double) => doubleElement.Value,
            DecimalElement decimalElement when targetType == typeof(decimal) => decimalElement.Value,
            DateElement dateElement when targetType == typeof(DateTime) => dateElement.Value,
            StringElement stringElement when targetType == typeof(string) => stringElement.Value,
            _ => throw new NotSupportedException($"Type '{targetType.Name}' is not supported for deserialization")
        };
    }
}
