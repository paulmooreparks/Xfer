using System;
using System.Collections.Generic;
using System.Reflection;

using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using System.Collections;
using System.Runtime.CompilerServices;

namespace ParksComputing.Xfer.Lang;

public class XferConvert {
    public static string Serialize(object? o, Formatting formatting = Formatting.None, char indentChar = ' ', int indentation = 2, int depth = 0) {
        Element element = SerializeValue(o);
        return element.ToXfer(formatting, indentChar, indentation, depth);
    }

    public static Element SerializeValue(object? value) {
        return value switch {
            null => new NullElement(),
            DBNull => new NullElement(),
            int intValue => new IntegerElement(intValue),
            long longValue => new LongElement(longValue),
            bool boolValue => new BooleanElement(boolValue),
            double doubleValue => new DoubleElement(doubleValue),
            decimal decimalValue => new DecimalElement(decimalValue),
            DateTime dateTimeValue => new DateTimeElement(dateTimeValue),
            DateOnly dateOnlyValue => new DateElement(dateOnlyValue),
            TimeOnly timeOnlyValue => new TimeElement(timeOnlyValue),
            TimeSpan timeSpanValue => new TimeSpanElement(timeSpanValue),
            DateTimeOffset dateTimeOffsetValue => new DateTimeElement(dateTimeOffsetValue.ToString("o")),
            string stringValue => new StringElement(stringValue, style: ElementStyle.Explicit),
            char charValue => new CharacterElement(charValue),
            Guid guidValue => new StringElement(guidValue.ToString()),
            Enum enumValue => SerializeEnumValue(enumValue),
            Uri uriValue => new StringElement(uriValue.ToString(), style: ElementStyle.Explicit),
            int[] intArray => SerializeIntArray(intArray),
            long[] longArray => SerializeLongArray(longArray),
            bool[] boolArray => SerializeBooleanArray(boolArray),
            double[] doubleArray => SerializeDoubleArray(doubleArray),
            decimal[] decimalArray => SerializeDecimalArray(decimalArray),
            DateTime[] dateArray => SerializeDateArray(dateArray),
            DateOnly[] dateOnlyArray => SerializeDateOnlyArray(dateOnlyArray),
            TimeOnly[] timeOnlyArray => SerializeTimeOnlyArray(timeOnlyArray),
            TimeSpan[] timeSpanArray => SerializeTimeSpanArray(timeSpanArray),
            string[] stringArray => SerializeStringArray(stringArray),
            byte[] byteArray => new StringElement(Convert.ToBase64String(byteArray), style: ElementStyle.Explicit),
            object[] objectArray => new PropertyBagElement(objectArray.Select(SerializeValue)),
            IDictionary dictionary when IsGenericDictionary(dictionary.GetType()) => SerializeDictionary(dictionary),
            IEnumerable enumerable when IsGenericEnumerable(enumerable.GetType()) => SerializeEnumerable(enumerable),
            System.Runtime.CompilerServices.ITuple tuple when IsGenericTuple(tuple.GetType()) => SerializeTuple(tuple),
#if false
            IDictionary dictionary => new ObjectElement(
                dictionary.Cast<dynamic>().Select(kvp =>
                    new KeyValuePairElement(SerializeValue(kvp.Key), SerializeValue(kvp.Value)))
            ),
            System.Dynamic.ExpandoObject expando => new ObjectElement(
                expando.Select(kvp =>
                    new KeyValuePairElement(SerializeValue(kvp.Key), SerializeValue(kvp.Value)))
            ),
#endif
            IEnumerable<object> list => new PropertyBagElement(list.Select(SerializeValue)),
            IEnumerable enumerable => new PropertyBagElement(
                enumerable.Cast<object>().Select(SerializeValue)
            ),
            object objectValue => SerializeObject(objectValue)
        };
    }

    private static bool IsGenericDictionary(Type type) {
        try {
            var isGenericType = type.IsGenericType;

            if (isGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                var idictType = typeof(IDictionary<,>);
                var dictType = typeof(Dictionary<,>);

                return isGenericType && (genericTypeDefinition == idictType || genericTypeDefinition == dictType)
                    && genericArguments[0] == typeof(string);
            }

            return false;
        }
        catch (Exception) {
            return false;
        }
    }

    private static bool IsGenericEnumerable(Type type) {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            && type.GetGenericArguments()[0] == typeof(string);
    }

    private static bool IsGenericTuple(Type type) {
        try {
            var isGenericType = type.IsGenericType;

            if (isGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                var tupleType = typeof(Tuple<,>);

                return isGenericType && genericTypeDefinition == tupleType;
            }

            return false;
        }
        catch (Exception) {
            return false;
        }
    }

    private static PropertyBagElement SerializeEnumerable(IEnumerable enumerable) {
        var objElement = new PropertyBagElement();

        foreach (var value in enumerable) {
            var valueElement = SerializeValue(value);
            objElement.Add(valueElement);
        }

        return objElement;
    }

    private static ObjectElement SerializeDictionary(IDictionary dictionary) {
        var objElement = new ObjectElement();

        foreach (DictionaryEntry kvp in dictionary) {
            var keyElement = new IdentifierElement(kvp.Key.ToString()!);
            var valueElement = SerializeValue(kvp.Value);
            objElement.AddOrUpdate(new KeyValuePairElement(keyElement, valueElement));
        }

        return objElement;
    }

    private static PropertyBagElement SerializeTuple(ITuple tuple) {
        var element = new PropertyBagElement();

        for (int i = 0; i < tuple.Length; ++i) {
            var valueElement = SerializeValue(tuple[i]);
            element.Add(valueElement);
        }

        return element;
    }

    public static object Deserialize(string xfer, Type targetType) {
        // Use reflection to create an instance of the generic method
        var method = typeof(XferConvert).GetMethod(nameof(Deserialize), new[] { typeof(string) });
        if (method == null) {
            throw new InvalidOperationException("Could not find the generic Deserialize method.");
        }

        var genericMethod = method.MakeGenericMethod(targetType);
        return genericMethod.Invoke(null, new object[] { xfer }) ?? throw new InvalidOperationException($"Failed to deserialize content into type {targetType.Name}.");
    }

    private static object? DeserializeValue(Element element, Type targetType) {
        if (targetType.IsEnum && element is TextElement textElement) {
            var deserializeEnumMethod = typeof(XferConvert)
                .GetMethod(nameof(DeserializeEnumValue), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(targetType);

            return deserializeEnumMethod.Invoke(null, new object[] { textElement });
        }

        if (targetType.IsGenericType) {
            var genericType = targetType.GetGenericTypeDefinition();

            // Handle Dictionary<string, T>
            if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>)) {
                var keyType = targetType.GetGenericArguments()[0];
                var valueType = targetType.GetGenericArguments()[1];

                if (keyType != typeof(string)) {
                    throw new NotSupportedException("Only Dictionary<string, T> is supported.");
                }

                if (element is ObjectElement objectElement) {
                    return DeserializeDictionary(objectElement, valueType);
                }
            }
            // Handle List<T>
            else if (genericType == typeof(List<>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is PropertyBagElement propBagElement) {
                    return DeserializeEnumerable(propBagElement, valueType);
                }
            }
            else if (genericType == typeof(Tuple<>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is PropertyBagElement propBagElement) {
                    return DeserializeTuple(propBagElement, valueType);
                }
            }
            else if (genericType == typeof(KeyValuePair<,>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is KeyValuePairElement keyValueElement) {
                    return DeserializeKeyValuePair(keyValueElement, valueType);
                }
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
            DateTimeElement dateElement when targetType == typeof(DateTimeOffset) => new DateTimeOffset(dateElement.Value),
            DateTimeElement dateElement when targetType == typeof(TimeSpan) => dateElement.Value.TimeOfDay,
            DateTimeElement dateElement when targetType == typeof(TimeOnly) => dateElement.Value.TimeOfDay,
            DateTimeElement dateElement when targetType == typeof(DateTime) => dateElement.Value,
            DateTimeElement dateElement when targetType == typeof(object) => dateElement.Value,
            TimeElement timeElement when targetType == typeof(TimeOnly) => timeElement.Value,
            TimeElement timeElement when targetType == typeof(TimeSpan) => timeElement.Value.ToTimeSpan(),
            TimeElement timeElement when targetType == typeof(object) => timeElement.Value,
            TimeSpanElement timeSpanElement when targetType == typeof(TimeSpan) => timeSpanElement.Value,
            TimeSpanElement timeSpanElement when targetType == typeof(object) => timeSpanElement.Value,
            DateElement dateElement when targetType == typeof(DateOnly) => dateElement.Value,
            DateElement dateElement when targetType == typeof(DateTime) => dateElement.Value.ToDateTime(TimeOnly.MinValue),
            DateElement dateElement when targetType == typeof(object) => dateElement.Value,
            StringElement stringElement when targetType == typeof(string) => stringElement.Value,
            StringElement stringElement when targetType == typeof(object) => stringElement.Value,
            CharacterElement charElement when targetType == typeof(string) => charElement.Value,
            CharacterElement charElement when targetType == typeof(object) => charElement.Value,
            NullElement nullElement when targetType == typeof(string) => nullElement.Value,
            NullElement nullElement when targetType == typeof(object) => nullElement.Value,
            EvaluatedElement evalElement when targetType == typeof(string) => evalElement.Value,
            EvaluatedElement evalElement when targetType == typeof(object) => evalElement.Value,
            PlaceholderElement phElement when targetType == typeof(string) => phElement.Value,
            PlaceholderElement phElement when targetType == typeof(object) => phElement.Value,
            ArrayElement arrayElement => DeserializeArray(arrayElement, targetType),
            PropertyBagElement propertyBagElement => DeserializePropertyBag(propertyBagElement, targetType),
            ObjectElement objectElement => DeserializeObject(objectElement, targetType),
            _ => throw new NotSupportedException($"Type '{targetType.Name}' is not supported for deserialization")
        };
    }

    private static object DeserializeDictionary(ObjectElement objectElement, Type valueType) {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
        var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        foreach (var kvp in objectElement.Values) {
            var key = kvp.Key;
            var value = DeserializeValue(kvp.Value.Value, valueType);
            dictionary.Add(key, value);
        }

        return dictionary;
    }

    private static object DeserializeEnumerable(PropertyBagElement propBagElement, Type valueType) {
        var listType = typeof(List<>).MakeGenericType(valueType);
        var list = (IList)Activator.CreateInstance(listType)!; 

        foreach (var element in propBagElement.Values) {
            var value = DeserializeValue(element, valueType);
            list.Add(value); 
        }

        return list;
    }

    private static object DeserializeTuple(PropertyBagElement propBagElement, Type valueType) {
        throw new NotSupportedException();
    }

    private static object DeserializeKeyValuePair(KeyValuePairElement kvpElement, Type valueType) {
        var kvpType = typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType);

        var key = kvpElement.Key;
        var value = DeserializeValue(kvpElement.Value, valueType);

        return Activator.CreateInstance(kvpType, key, value)!;
    }


    private static ObjectElement SerializeObject(object o) {
        var type = o.GetType();
        var objElement = new ObjectElement();

        foreach (var property in type.GetProperties()) {
            var attribute = property.GetCustomAttribute<XferPropertyAttribute>();
            var evalAttribute = property.GetCustomAttribute<XferEvaluatedAttribute>();

            var name = attribute?.Name ?? property.Name;
            var value = property.GetValue(o);

            if (evalAttribute != null) {
                Element element;

                if (value is null) {
                    element = new NullElement();
                }
                else {
                    element = new EvaluatedElement(value.ToString() ?? string.Empty);
                }

                objElement.AddOrUpdate(new KeyValuePairElement(new IdentifierElement(name), element));
            }
            else {
                Element element = SerializeValue(value);
                objElement.AddOrUpdate(new KeyValuePairElement(new IdentifierElement(name), element));
            }
        }

        return objElement;
    }

    private static TypedArrayElement<IntegerElement> SerializeIntArray(int[] intArray) {
        var arrayElement = new TypedArrayElement<IntegerElement>();

        foreach (var item in intArray) {
            arrayElement.Add(new IntegerElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<LongElement> SerializeLongArray(long[] longArray) {
        var arrayElement = new TypedArrayElement<LongElement>();

        foreach (var item in longArray) {
            arrayElement.Add(new LongElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<BooleanElement> SerializeBooleanArray(bool[] boolArray) {
        var arrayElement = new TypedArrayElement<BooleanElement>();

        foreach (var item in boolArray) {
            arrayElement.Add(new BooleanElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<DoubleElement> SerializeDoubleArray(double[] doubleArray) {
        var arrayElement = new TypedArrayElement<DoubleElement>();

        foreach (var item in doubleArray) {
            arrayElement.Add(new DoubleElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<DecimalElement> SerializeDecimalArray(decimal[] decimalArray) {
        var arrayElement = new TypedArrayElement<DecimalElement>();

        foreach (var item in decimalArray) {
            arrayElement.Add(new DecimalElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<DateTimeElement> SerializeDateArray(DateTime[] dateArray) {
        var arrayElement = new TypedArrayElement<DateTimeElement>();

        foreach (var item in dateArray) {
            arrayElement.Add(new DateTimeElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<DateElement> SerializeDateOnlyArray(DateOnly[] dateArray) {
        var arrayElement = new TypedArrayElement<DateElement>();

        foreach (var item in dateArray) {
            arrayElement.Add(new DateElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<TimeElement> SerializeTimeOnlyArray(TimeOnly[] timeArray) {
        var arrayElement = new TypedArrayElement<TimeElement>();

        foreach (var item in timeArray) {
            arrayElement.Add(new TimeElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<TimeSpanElement> SerializeTimeSpanArray(TimeSpan[] timeArray) {
        var arrayElement = new TypedArrayElement<TimeSpanElement>();

        foreach (var item in timeArray) {
            arrayElement.Add(new TimeSpanElement(item));
        }

        return arrayElement;
    }

    private static TypedArrayElement<StringElement> SerializeStringArray(string[] stringArray) {
        var arrayElement = new TypedArrayElement<StringElement>();

        foreach (var item in stringArray) {
            arrayElement.Add(new StringElement(item, style: ElementStyle.Explicit));
        }

        return arrayElement;
    }

    private static StringElement SerializeEnumValue<TEnum>(TEnum enumValue) where TEnum : Enum {
        var key = typeof(TEnum).Name;
        var value = enumValue.ToString();
        return new StringElement(value);
    }

    private static TEnum DeserializeEnumValue<TEnum>(TextElement textElement) where TEnum : struct, Enum {
        return Enum.Parse<TEnum>(textElement.Value);
    }


    public static T Deserialize<T>(string xfer) where T : new() {
        var instance = new T();
        var type = typeof(T);

        if (instance == null) {
            throw new InvalidOperationException($"Could not create an instance of type {type.Name}.");
        }

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

        if (first is ObjectElement objectElement) {
            if (instance is not null && IsGenericDictionary(instance.GetType())) {
                IDictionary? dictionary = instance as IDictionary;
                if (dictionary is not null) {
                    foreach (var element in objectElement.Values) {
                        string value = DeserializeValue(element.Value.Value, typeof(string)) as string ?? string.Empty;
                        dictionary.Add(element.Key, value);
                    }

                    return instance;
                }
            }

            foreach (var element in objectElement.Values) {
                if (propertyMap.TryGetValue(element.Key, out var property)) {
                    object? value = DeserializeValue(element.Value.Value, property.PropertyType);
                    property.SetValue(instance, value);
                }
            }
        }
#if false
        else if (first is PropertyBagElement propertyBagElement) {
            foreach (var element in propertyBagElement.Values) {
                if (propertyMap.TryGetValue(element, out var property)) {
                    object? value = DeserializeValue(element, property.PropertyType);
                    property.SetValue(instance, value);
                }
            }
        }
#endif

        return instance;
    }

    private static object DeserializeArray(ArrayElement arrayElement, Type targetType) {
        Type? elementType;
        var elementTypeArray = targetType.GenericTypeArguments;

        if (elementTypeArray.Length == 0) {
            elementType = targetType.GetElementType();
        }
        else {
            elementType = elementTypeArray[0];
        }

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

    private static object DeserializePropertyBag(PropertyBagElement propertyBagElement, Type targetType) {
        var values = new List<object?>();
        foreach (var item in propertyBagElement.Values) {
            values.Add(DeserializeValue(item, typeof(object)));
        }
        return values;
    }

    private static object DeserializeObject(ObjectElement objectElement, Type targetType) {
        var instance = Activator.CreateInstance(targetType);

        if (instance == null) {
            throw new InvalidOperationException($"Could not create an instance of type {targetType.Name}.");
        }

        var properties = targetType.GetProperties();

        foreach (var prop in properties) {
            var attribute = prop.GetCustomAttribute<XferPropertyAttribute>();
            var propName = attribute?.Name ?? prop.Name;
            var matchingElement = objectElement.Values.FirstOrDefault(kvp => kvp.Key == propName).Value;

            if (matchingElement != null) {
                var propValue = DeserializeValue(matchingElement.Value, prop.PropertyType);
                prop.SetValue(instance, propValue);
            }
        }
        return instance;
    }
}
