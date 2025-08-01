using System;
using System.Collections.Generic;
using System.Reflection;

using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Helpers;
using System.Collections;
using System.Runtime.CompilerServices;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Converters;
using System.Linq;

namespace ParksComputing.Xfer.Lang;

public class XferConvert {
    private static readonly XferSerializerSettings DefaultSettings = new();

    /// <summary>
    /// Creates an ObjectElement from a POCO, similar to JObject.FromObject.
    /// </summary>
    public static ObjectElement FromObject(object o, XferSerializerSettings? settings = null)
    {
        return SerializeObject(o, settings ?? DefaultSettings);
    }

    public static string Serialize(object? o, Formatting formatting = Formatting.None, char indentChar = ' ', int indentation = 2, int depth = 0) {
        return Serialize(o, DefaultSettings, formatting, indentChar, indentation, depth);
    }

    public static string Serialize(object? o, XferSerializerSettings settings, Formatting formatting = Formatting.None, char indentChar = ' ', int indentation = 2, int depth = 0) {
        Element element = SerializeValue(o, settings);
        return element.ToXfer(formatting, indentChar, indentation, depth);
    }

    public static Element SerializeValue(object? value) {
        return SerializeValue(value, DefaultSettings);
    }

    public static Element SerializeValue(object? value, XferSerializerSettings settings) {
        if (value != null) {
            foreach (var converter in settings.Converters) {
                if (converter.CanConvert(value.GetType())) {
                    return converter.WriteXfer(value, settings);
                }
            }
        }

        return value switch {
            null => new NullElement(),
            DBNull => new NullElement(),
            int intValue => new IntegerElement(intValue,
                elementStyle: Helpers.ElementStyleHelper.GetIntegerStyle(intValue, settings.StylePreference, settings.PreferImplicitSyntax)),
            long longValue => new LongElement(longValue,
                style: Helpers.ElementStyleHelper.GetLongStyle(longValue, settings.StylePreference)),
            bool boolValue => new BooleanElement(boolValue),
            double doubleValue => new DoubleElement(doubleValue,
                style: Helpers.ElementStyleHelper.GetDoubleStyle(doubleValue, settings.StylePreference)),
            decimal decimalValue => new DecimalElement(decimalValue,
                style: Helpers.ElementStyleHelper.GetDecimalStyle(decimalValue, settings.StylePreference)),
            DateTime dateTimeValue => new DateTimeElement(dateTimeValue),
            DateOnly dateOnlyValue => new DateElement(dateOnlyValue),
            TimeOnly timeOnlyValue => new TimeElement(timeOnlyValue),
            TimeSpan timeSpanValue => new TimeSpanElement(timeSpanValue),
            DateTimeOffset dateTimeOffsetValue => new DateTimeElement(dateTimeOffsetValue.ToString("o")),
            string stringValue => new StringElement(stringValue,
                style: Helpers.ElementStyleHelper.GetStringStyle(stringValue, settings.StylePreference)),
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
            object[] objectArray => new TupleElement(objectArray.Select(o => SerializeValue(o, settings))),
            IDictionary dictionary when IsGenericDictionary(dictionary.GetType()) => SerializeDictionary(dictionary, settings),
            IEnumerable enumerable when IsGenericEnumerable(enumerable.GetType()) => SerializeEnumerable(enumerable, settings),
            System.Runtime.CompilerServices.ITuple tuple when IsGenericTuple(tuple.GetType()) => SerializeTuple(tuple, settings),
#if false
            IDictionary dictionary => new ObjectElement(
                dictionary.Cast<dynamic>().Select(kvp =>
                    new KeyValuePairElement(SerializeValue(kvp.Key), SerializeValue(kvp.Value)))
            ),
            System.Dynamic.ExpandoObject expando => new ObjectElement(
                expando.Select(kvp =>
                    new KeyValuePairElement(SerializeValue(kvp.Key, settings), SerializeValue(kvp.Value, settings)))
            ),
#endif
            object objectValue => SerializeObject(objectValue, settings)
        };
    }

    private static bool IsGenericDictionary(Type type) {
        // This is a more robust way to check for dictionary types,
        // including handling of interfaces and concrete classes.
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
            return true;
        }

        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    private static bool IsGenericEnumerable(Type type) {
        // We don't want to treat strings as enumerables.
        if (type == typeof(string))
        {
            return false;
        }

        // This check is more robust for finding any IEnumerable<T> implementation,
        // while explicitly excluding dictionaries, which we handle separately.
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            && !IsGenericDictionary(type);
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

    private static TupleElement SerializeEnumerable(IEnumerable enumerable, XferSerializerSettings settings) {
        var objElement = new TupleElement();

        foreach (var value in enumerable) {
            var valueElement = SerializeValue(value, settings);
            objElement.Add(valueElement);
        }

        return objElement;
    }

    private static ObjectElement SerializeDictionary(IDictionary dictionary, XferSerializerSettings settings) {
        var objElement = new ObjectElement();

        foreach (DictionaryEntry kvp in dictionary) {
            var keyElement = new IdentifierElement(kvp.Key.ToString()!);
            var valueElement = SerializeValue(kvp.Value, settings);
            objElement.AddOrUpdate(new KeyValuePairElement(keyElement, valueElement));
        }

        return objElement;
    }

    private static TupleElement SerializeTuple(ITuple tuple, XferSerializerSettings settings) {
        var element = new TupleElement();

        for (int i = 0; i < tuple.Length; ++i) {
            var valueElement = SerializeValue(tuple[i], settings);
            element.Add(valueElement);
        }

        return element;
    }

    public static T? Deserialize<T>(string xfer) {
        return Deserialize<T>(xfer, DefaultSettings);
    }

    public static T? Deserialize<T>(string xfer, XferSerializerSettings settings) {
        if (string.IsNullOrWhiteSpace(xfer))
        {
            return default;
        }

        var document = XferParser.Parse(xfer);

        if (document is null || !document.Root.Values.Any())
        {
            return default;
        }

        return Deserialize<T>(document, settings);
    }

    public static T? Deserialize<T>(XferDocument document) {
        return Deserialize<T>(document, DefaultSettings);
    }

    public static T? Deserialize<T>(XferDocument document, XferSerializerSettings settings)
    {
        if (document.Root.Values.FirstOrDefault() is not Element first)
        {
            return default;
        }

        // PI-driven deserialization customization
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        var result = DeserializeValue(first, typeof(T), settings);

        return (T?)result;
    }

    public static object? Deserialize(XferDocument document, Type targetType) {
        return Deserialize(document, targetType, DefaultSettings);
    }

    public static object? Deserialize(XferDocument document, Type targetType, XferSerializerSettings settings) {
        if (document.Root.Values.FirstOrDefault() is not Element first)
        {
            return default;
        }
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        return DeserializeValue(first, targetType, settings);
    }

    public static object? Deserialize(string xfer, Type targetType) {
        return Deserialize(xfer, targetType, DefaultSettings);
    }

    public static object? Deserialize(string xfer, Type targetType, XferSerializerSettings settings) {
        if (string.IsNullOrWhiteSpace(xfer))
        {
            return default;
        }
        var document = XferParser.Parse(xfer);
        if (document is null || !document.Root.Values.Any())
        {
            return default;
        }
        return Deserialize(document, targetType, settings);
    }

    public static T? Deserialize<T>(Element element) {
        return Deserialize<T>(element, DefaultSettings);
    }

    public static T? Deserialize<T>(Element element, XferSerializerSettings settings) {
        if (element is null) {
            throw new NullReferenceException($"Xfer element is null.");
        }

        var type = typeof(T);
        return (T?)DeserializeValue(element, type, settings);
    }

    private static object? DeserializeValue(Element element, Type targetType) {
        return DeserializeValue(element, targetType, DefaultSettings);
    }

    private static object? DeserializeValue(Element element, Type targetType, XferSerializerSettings settings) {
        foreach (var converter in settings.Converters) {
            if (converter.CanConvert(targetType)) {
                return converter.ReadXfer(element, targetType, settings);
            }
        }

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
                    return DeserializeDictionary(objectElement, valueType, settings);
                }
            }
            // Handle List<T>
            else if (genericType == typeof(List<>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is TupleElement tupleElement) {
                    return DeserializeEnumerable(tupleElement, valueType, settings);
                }
            }
            else if (genericType == typeof(Tuple<>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is TupleElement tupleElement) {
                    return DeserializeTuple(tupleElement, valueType, settings);
                }
            }
            else if (genericType == typeof(KeyValuePair<,>)) {
                var valueType = targetType.GetGenericArguments()[0];

                if (element is KeyValuePairElement keyValueElement) {
                    return DeserializeKeyValuePair(keyValueElement, valueType, settings);
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
            StringElement stringElement when targetType == typeof(Guid) => Guid.Parse(stringElement.Value),
            StringElement stringElement when targetType == typeof(object) => stringElement.Value,
            CharacterElement charElement when targetType == typeof(string) => charElement.Value,
            CharacterElement charElement when targetType == typeof(object) => charElement.Value,
            NullElement nullElement when targetType == typeof(string) => nullElement.Value,
            NullElement nullElement when targetType == typeof(object) => nullElement.Value,
            InterpolatedElement evalElement when targetType == typeof(string) => evalElement.Value,
            InterpolatedElement evalElement when targetType == typeof(object) => evalElement.Value,
            DynamicElement phElement when targetType == typeof(string) => phElement.Value,
            DynamicElement phElement when targetType == typeof(object) => phElement.Value,
            ArrayElement arrayElement => DeserializeArray(arrayElement, targetType, settings),
            TupleElement tupleElement => DeserializeTuple(tupleElement, targetType, settings),
            ObjectElement objectElement => DeserializeObject(objectElement, targetType, settings),
            _ => throw new NotSupportedException($"Type '{targetType.Name}' is not supported for deserialization")
        };
    }

    private static object DeserializeDictionary(ObjectElement objectElement, Type valueType, XferSerializerSettings settings) {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
        var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        foreach (var kvp in objectElement.Values) {
            var key = kvp.Key;
            var value = DeserializeValue(kvp.Value.Value, valueType, settings);
            dictionary.Add(key, value);
        }

        return dictionary;
    }

    private static object DeserializeEnumerable(TupleElement tupleElement, Type valueType, XferSerializerSettings settings) {
        var listType = typeof(List<>).MakeGenericType(valueType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var element in tupleElement.Values) {
            var value = DeserializeValue(element, valueType, settings);
            list.Add(value);
        }

        return list;
    }

    private static object DeserializeKeyValuePair(KeyValuePairElement kvpElement, Type valueType, XferSerializerSettings settings) {
        var kvpType = typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType);

        var key = kvpElement.Key;
        var value = DeserializeValue(kvpElement.Value, valueType, settings);

        return Activator.CreateInstance(kvpType, key, value)!;
    }


    private static ObjectElement SerializeObject(object o, XferSerializerSettings settings) {
        var type = o.GetType();
        var objElement = new ObjectElement();
        var properties = settings.ContractResolver.ResolveProperties(type);

        foreach (var property in properties) {
            var attribute = property.GetCustomAttribute<XferPropertyAttribute>();
            var evalAttribute = property.GetCustomAttribute<XferEvaluatedAttribute>();

            var name = attribute?.Name ?? settings.ContractResolver.ResolvePropertyName(property.Name);
            var value = property.GetValue(o);

            if (value is null && settings.NullValueHandling == NullValueHandling.Ignore) {
                continue;
            }

            if (evalAttribute != null) {
                Element element;

                if (value is null) {
                    element = new NullElement();
                }
                else {
                    element = new InterpolatedElement(value.ToString() ?? string.Empty);
                }

                objElement.AddOrUpdate(new KeyValuePairElement(new IdentifierElement(name), element));
            }
            else {
                Element element;

                // Check for custom numeric formatting attributes
                if (value is int intValue) {
                    element = Helpers.ElementStyleHelper.CreateFormattedIntegerElement(intValue, property, settings.StylePreference, settings.PreferImplicitSyntax);
                }
                else if (value is long longValue) {
                    element = Helpers.ElementStyleHelper.CreateFormattedLongElement(longValue, property, settings.StylePreference);
                }
                else if (value is decimal decimalValue) {
                    element = Helpers.ElementStyleHelper.CreateFormattedDecimalElement(decimalValue, property, settings.StylePreference);
                }
                else if (value is double doubleValue) {
                    element = Helpers.ElementStyleHelper.CreateFormattedDoubleElement(doubleValue, property, settings.StylePreference);
                }
                else {
                    element = SerializeValue(value, settings);
                }

                objElement.AddOrUpdate(new KeyValuePairElement(new IdentifierElement(name), element));
            }
        }

        return objElement;
    }

    private static ArrayElement SerializeIntArray(int[] intArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in intArray) {
            arrayElement.Add(new IntegerElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeLongArray(long[] longArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in longArray) {
            arrayElement.Add(new LongElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeBooleanArray(bool[] boolArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in boolArray) {
            arrayElement.Add(new BooleanElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeDoubleArray(double[] doubleArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in doubleArray) {
            arrayElement.Add(new DoubleElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeDecimalArray(decimal[] decimalArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in decimalArray) {
            arrayElement.Add(new DecimalElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeDateArray(DateTime[] dateArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in dateArray) {
            arrayElement.Add(new DateTimeElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeDateOnlyArray(DateOnly[] dateArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in dateArray) {
            arrayElement.Add(new DateElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeTimeOnlyArray(TimeOnly[] timeArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in timeArray) {
            arrayElement.Add(new TimeElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeTimeSpanArray(TimeSpan[] timeArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in timeArray) {
            arrayElement.Add(new TimeSpanElement(item));
        }

        return arrayElement;
    }

    private static ArrayElement SerializeStringArray(string[] stringArray) {
        var arrayElement = new ArrayElement();

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


    private static object DeserializeArray(ArrayElement arrayElement, Type targetType, XferSerializerSettings settings) {
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
        // Use the Values property from ArrayElement
        foreach (var item in arrayElement.Values) {
            values.Add(DeserializeValue(item, elementType, settings));
        }

        var array = Array.CreateInstance(elementType, values.Count);

        for (int i = 0; i < values.Count; i++) {
            array.SetValue(values[i], i);
        }

        return array;
    }

    private static object DeserializeTuple(TupleElement tupleElement, Type targetType, XferSerializerSettings settings) {
        var values = new List<object?>();
        foreach (var item in tupleElement.Values) {
            values.Add(DeserializeValue(item, typeof(object), settings));
        }
        return values;
    }

    private static object DeserializeObject(ObjectElement objectElement, Type targetType, XferSerializerSettings settings) {
        // Map incoming data to a dictionary for easy lookup
        var valueDict = objectElement.Values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

        // Find the constructor with the most parameters
        var constructors = targetType.GetConstructors();
        var ctor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        object? instance = null;

        // Cache property attributes for parameter name lookup
        var properties = targetType.GetProperties();
        var propertyAttrMap = properties.ToDictionary(
            p => p.Name,
            p => p.GetCustomAttribute<XferPropertyAttribute>()?.Name ?? p.Name,
            StringComparer.OrdinalIgnoreCase);

        if (ctor != null && ctor.GetParameters().Length > 0)
        {
            var ctorParams = ctor.GetParameters();
            var args = new object?[ctorParams.Length];
            for (int i = 0; i < ctorParams.Length; i++)
            {
                var param = ctorParams[i];
                // Try to find a matching property attribute name, else use parameter name
                // param.Name is never null for constructor parameters, but suppress warning for static analysis
                string paramName = param.Name ?? string.Empty;
                string matchName = propertyAttrMap.TryGetValue(paramName, out var attrName) ? attrName : paramName;
                var match = valueDict.FirstOrDefault(kvp => string.Equals(kvp.Key, matchName, StringComparison.OrdinalIgnoreCase));
                if (match.Key != null)
                {
                    args[i] = DeserializeValue(match.Value, param.ParameterType, settings);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else
                {
                    throw new InvalidOperationException($"Missing required constructor parameter '{param.Name}' for type {targetType.FullName}");
                }
            }
            instance = ctor.Invoke(args);
        }
        else
        {
            // Fallback to default constructor
            instance = Activator.CreateInstance(targetType);
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not create an instance of type {targetType.Name}.");
            }
        }

        // Set remaining writable properties (not set via constructor)
        foreach (var prop in properties)
        {
            if (!prop.CanWrite) {
                continue;
            }

            var attribute = prop.GetCustomAttribute<XferPropertyAttribute>();
            var propName = attribute?.Name ?? prop.Name;
            // Only set if not already set by constructor
            if (valueDict.TryGetValue(propName, out var rawValue))
            {
                var propValue = DeserializeValue(rawValue, prop.PropertyType, settings);
                prop.SetValue(instance, propValue);
            }
        }
        return instance;
    }

    /// <summary>
    /// Deserializes an ObjectElement to a POCO of type T.
    /// </summary>
    public static T? ToObject<T>(ObjectElement element, XferSerializerSettings? settings = null)
    {
        if (element == null) {
            return default;
        }

        return (T?)DeserializeObject(element, typeof(T), settings ?? DefaultSettings);
    }

    /// <summary>
    /// Tries to create an ObjectElement from a POCO, returns false on error.
    /// </summary>
    public static bool TryFromObject(object o, out ObjectElement? result, XferSerializerSettings? settings = null)
    {
        try
        {
            result = SerializeObject(o, settings ?? DefaultSettings);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to deserialize an ObjectElement to a POCO of type T, returns false on error.
    /// </summary>
    public static bool TryToObject<T>(ObjectElement element, out T? result, XferSerializerSettings? settings = null)
    {
        try
        {
            result = ToObject<T>(element, settings);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
