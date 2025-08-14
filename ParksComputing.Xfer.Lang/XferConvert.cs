using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

/// <summary>
/// Provides static methods for converting between .NET objects and XferLang elements or text.
/// This class serves as the primary entry point for serialization and deserialization operations,
/// similar to JsonConvert in Newtonsoft.Json.
/// </summary>
public class XferConvert {
    private static readonly XferSerializerSettings DefaultSettings = new();

    /// <summary>
    /// Creates an ObjectElement from a POCO, similar to JObject.FromObject.
    /// </summary>
    public static ObjectElement FromObject(object o, XferSerializerSettings? settings = null)
    {
        return SerializeObject(o, settings ?? DefaultSettings);
    }

    /// <summary>
    /// Serializes an object to a XferLang string with formatting options.
    /// Uses default serializer settings.
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="indentChar">Character to use for indentation (default: space).</param>
    /// <param name="indentation">Number of indent characters per level (default: 2).</param>
    /// <param name="depth">Starting depth level for indentation (default: 0).</param>
    /// <returns>XferLang string representation of the object.</returns>
    public static string Serialize(object? o, Formatting formatting = Formatting.None, char indentChar = ' ', int indentation = 2, int depth = 0) {
        return Serialize(o, DefaultSettings, formatting, indentChar, indentation, depth);
    }

    /// <summary>
    /// Serializes an object to a XferLang string with custom settings and formatting options.
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <param name="settings">Serializer settings to control conversion behavior.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="indentChar">Character to use for indentation (default: space).</param>
    /// <param name="indentation">Number of indent characters per level (default: 2).</param>
    /// <param name="depth">Starting depth level for indentation (default: 0).</param>
    /// <returns>XferLang string representation of the object.</returns>
    public static string Serialize(object? o, XferSerializerSettings settings, Formatting formatting = Formatting.None, char indentChar = ' ', int indentation = 2, int depth = 0) {
        Element element = SerializeValue(o, settings);
        return element.ToXfer(formatting, indentChar, indentation, depth);
    }

    /// <summary>
    /// Converts an object to a XferLang Element using default settings.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The XferLang Element representation of the object.</returns>
    public static Element SerializeValue(object? value) {
        return SerializeValue(value, DefaultSettings);
    }

    /// <summary>
    /// Converts an object to a XferLang Element using custom serializer settings.
    /// Applies custom converters first, then falls back to built-in type conversion logic.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="settings">Serializer settings to control conversion behavior.</param>
    /// <returns>The XferLang Element representation of the object.</returns>
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
            Array enumArray when enumArray.GetType().GetElementType()?.IsEnum == true => SerializeEnumArray(enumArray),
            object[] objectArray when objectArray.GetType().GetElementType()?.IsEnum == true => SerializeEnumArray(objectArray),
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
            var keyElement = new KeywordElement(kvp.Key.ToString()!);
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

    /// <summary>
    /// Deserializes a XferLang string to an object of type T using default settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="xfer">The XferLang string to deserialize.</param>
    /// <returns>An object of type T, or default(T) if the string is null or empty.</returns>
    public static T? Deserialize<T>(string xfer) {
        return Deserialize<T>(xfer, DefaultSettings);
    }

    /// <summary>
    /// Deserializes a XferLang string to an object of type T using custom settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="xfer">The XferLang string to deserialize.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <returns>An object of type T, or default(T) if the string is null or empty.</returns>
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

    /// <summary>
    /// Deserializes a XferDocument to an object of type T using default settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="document">The XferDocument to deserialize.</param>
    /// <returns>An object of type T, or default(T) if the document has no elements.</returns>
    public static T? Deserialize<T>(XferDocument document) {
        return Deserialize<T>(document, DefaultSettings);
    }

    /// <summary>
    /// Deserializes a XferDocument to an object of type T using custom settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="document">The XferDocument to deserialize.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <returns>An object of type T, or default(T) if the document has no elements.</returns>
    public static T? Deserialize<T>(XferDocument document, XferSerializerSettings settings)
    {
        if (!document.Root.Values.Any())
        {
            return default;
        }

        // Per spec: document.Root must be a collection element (ObjectElement, ArrayElement, or TupleElement)
        if (!(document.Root is ObjectElement || document.Root is ArrayElement || document.Root is TupleElement))
        {
            throw new InvalidOperationException($"Invalid XferDocument: Root element must be a collection type (ObjectElement, ArrayElement, or TupleElement), but found '{document.Root.GetType().Name}'.");
        }

        // PI-driven deserialization customization
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();

        // If we're deserializing to a primitive type or string, use the first value from the collection
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
        {
            var first = document.Root.Values.FirstOrDefault();
            if (first == null)
            {
                return default;
            }
            var result = DeserializeValue(first, typeof(T), settings);
            return (T?)result;
        }
        else
        {
            // For complex types, deserialize the entire collection element
            var result = DeserializeValue(document.Root, typeof(T), settings);
            return (T?)result;
        }
    }

    /// <summary>
    /// Deserializes a XferDocument to an object of the specified type using default settings.
    /// </summary>
    /// <param name="document">The XferDocument to deserialize.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <returns>An object of the target type, or null if the document has no elements.</returns>
    public static object? Deserialize(XferDocument document, Type targetType) {
        return Deserialize(document, targetType, DefaultSettings);
    }

    /// <summary>
    /// Deserializes a XferDocument to an object of the specified type using custom settings.
    /// </summary>
    /// <param name="document">The XferDocument to deserialize.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <returns>An object of the target type, or null if the document has no elements.</returns>
    public static object? Deserialize(XferDocument document, Type targetType, XferSerializerSettings settings) {
        if (document.Root == null)
        {
            return default;
        }

        // The document root itself is the element we want to deserialize
        return DeserializeValue(document.Root, targetType, settings);
    }

    /// <summary>
    /// Deserializes a XferLang string to an object of the specified type using default settings.
    /// </summary>
    /// <param name="xfer">The XferLang string to deserialize.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <returns>An object of the target type, or null if the string is null, empty, or invalid.</returns>
    public static object? Deserialize(string xfer, Type targetType) {
        return Deserialize(xfer, targetType, DefaultSettings);
    }

    /// <summary>
    /// Deserializes a XferLang string to an object of the specified type using custom settings.
    /// </summary>
    /// <param name="xfer">The XferLang string to deserialize.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <returns>An object of the target type, or null if the string is null, empty, or invalid.</returns>
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

    /// <summary>
    /// Deserializes a XferLang Element to an object of type T using default settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="element">The XferLang Element to deserialize.</param>
    /// <returns>An object of type T.</returns>
    /// <exception cref="NullReferenceException">Thrown when the element is null.</exception>
    public static T? Deserialize<T>(Element element) {
        return Deserialize<T>(element, DefaultSettings);
    }

    /// <summary>
    /// Deserializes a XferLang Element to an object of type T using custom settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="element">The XferLang Element to deserialize.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <returns>An object of type T.</returns>
    /// <exception cref="NullReferenceException">Thrown when the element is null.</exception>
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

        if (targetType.IsEnum && element is IdentifierElement identifierElement) {
            // Use the non-generic overload that takes Type
            return DeserializeEnumValue(identifierElement, targetType);
        }

        if (targetType.IsGenericType) {
            var genericType = targetType.GetGenericTypeDefinition();

            // Handle Nullable<T>
            if (genericType == typeof(Nullable<>)) {
                var underlyingType = targetType.GetGenericArguments()[0];

                // If element is null, return null for nullable
                if (element is NullElement) {
                    return null;
                }

                // Otherwise, deserialize as the underlying type
                return DeserializeValue(element, underlyingType, settings);
            }

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
            IdentifierElement identifierElem when targetType.IsEnum => DeserializeEnumValue(identifierElem, targetType),
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
            NullElement nullElement when targetType == typeof(string) => null,
            NullElement nullElement when targetType == typeof(object) => null,
            NullElement nullElement when targetType.IsClass => null,
            InterpolatedElement evalElement when targetType == typeof(string) => evalElement.Value,
            InterpolatedElement evalElement when targetType == typeof(object) => evalElement.Value,
            DynamicElement phElement when targetType == typeof(string) => phElement.Value,
            DynamicElement phElement when targetType == typeof(object) => phElement.Value,
            ArrayElement arrayElement when targetType.IsArray => DeserializeArray(arrayElement, targetType, settings),
            TupleElement tupleElement when targetType.IsArray => DeserializeArrayFromTuple(tupleElement, targetType, settings),
            TupleElement tupleElement => DeserializeTuple(tupleElement, targetType, settings),
            ObjectElement objectElement => DeserializeObject(objectElement, targetType, settings),
            _ => throw new NotSupportedException($"Type '{targetType.Name}' is not supported for deserialization")
        };
    }

    private static object DeserializeDictionary(ObjectElement objectElement, Type valueType, XferSerializerSettings settings) {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
        var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        foreach (var kvp in objectElement.Dictionary) {
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
            // If a property is marked as a capture target (Tag/Id), it should not be serialized.
            var capTag = property.GetCustomAttribute<XferCaptureTagAttribute>();
            var capId = property.GetCustomAttribute<XferCaptureIdAttribute>();
            if (capTag != null && capId != null) {
                throw new InvalidOperationException($"Property '{type.FullName}.{property.Name}' cannot have both XferCaptureTag and XferCaptureId.");
            }
            if (capTag != null || capId != null) {
                continue; // skip serializing capture-target properties
            }
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

                objElement.AddOrUpdate(new KeyValuePairElement(new KeywordElement(name), element));
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
                    element = CreateAttributedDecimalElement(decimalValue, property, settings);
                }
                else if (value is double doubleValue) {
                    element = CreateAttributedDoubleElement(doubleValue, property, settings);
                }
                else {
                    element = SerializeValue(value, settings);
                }

                objElement.AddOrUpdate(new KeyValuePairElement(new KeywordElement(name), element));
            }
        }

        return objElement;
    }

    /// <summary>
    /// Creates a DecimalElement honoring XferDecimalPrecisionAttribute and XferNumericFormatAttribute.
    /// This duplicates intent of ElementStyleHelper but is inlined here to ensure test precision semantics.
    /// </summary>
    private static DecimalElement CreateAttributedDecimalElement(decimal value, PropertyInfo? property, XferSerializerSettings settings) {
        var formatAttr = property?.GetCustomAttribute(typeof(XferNumericFormatAttribute)) as XferNumericFormatAttribute;
        var precisionAttr = property?.GetCustomAttribute(typeof(XferDecimalPrecisionAttribute)) as XferDecimalPrecisionAttribute;
        var style = Helpers.ElementStyleHelper.GetDecimalStyle(value, settings.StylePreference);
        var element = new DecimalElement(value, style: style);
        if (formatAttr != null && formatAttr.Format != XferNumericFormat.Default) {
            element.SetNumericFormat(formatAttr.Format == XferNumericFormat.Default ? XferNumericFormat.Decimal : formatAttr.Format, formatAttr.MinBits, formatAttr.MinDigits);
        }
        if (precisionAttr != null) {
            // Round underlying value to reflect expected persistence semantics
            var rounded = Math.Round(value, precisionAttr.DecimalPlaces, MidpointRounding.AwayFromZero);
            element = new DecimalElement(rounded, style: style);
            if (formatAttr != null && formatAttr.Format != XferNumericFormat.Default) {
                element.SetNumericFormat(formatAttr.Format == XferNumericFormat.Default ? XferNumericFormat.Decimal : formatAttr.Format, formatAttr.MinBits, formatAttr.MinDigits);
            }
            element.SetPrecision(precisionAttr.DecimalPlaces, precisionAttr.RemoveTrailingZeros);
        }
        return element;
    }

    /// <summary>
    /// Creates a DoubleElement honoring XferDecimalPrecisionAttribute and XferNumericFormatAttribute.
    /// </summary>
    private static DoubleElement CreateAttributedDoubleElement(double value, PropertyInfo? property, XferSerializerSettings settings) {
        var formatAttr = property?.GetCustomAttribute(typeof(XferNumericFormatAttribute)) as XferNumericFormatAttribute;
        var precisionAttr = property?.GetCustomAttribute(typeof(XferDecimalPrecisionAttribute)) as XferDecimalPrecisionAttribute;
        var style = Helpers.ElementStyleHelper.GetDoubleStyle(value, settings.StylePreference);
        var element = new DoubleElement(value, style: style);
        if (formatAttr != null && formatAttr.Format != XferNumericFormat.Default) {
            element.SetNumericFormat(formatAttr.Format == XferNumericFormat.Default ? XferNumericFormat.Decimal : formatAttr.Format, formatAttr.MinBits, formatAttr.MinDigits);
        }
        if (precisionAttr != null) {
            var rounded = Math.Round(value, precisionAttr.DecimalPlaces, MidpointRounding.AwayFromZero);
            element = new DoubleElement(rounded, style: style);
            if (formatAttr != null && formatAttr.Format != XferNumericFormat.Default) {
                element.SetNumericFormat(formatAttr.Format == XferNumericFormat.Default ? XferNumericFormat.Decimal : formatAttr.Format, formatAttr.MinBits, formatAttr.MinDigits);
            }
            element.SetPrecision(precisionAttr.DecimalPlaces, precisionAttr.RemoveTrailingZeros);
        }
        return element;
    }

    // Removed legacy fallback reflection path; standard GetCustomAttribute is sufficient after constructor fix.

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

    private static ArrayElement SerializeEnumArray(Array enumArray) {
        var arrayElement = new ArrayElement();

        foreach (var item in enumArray) {
            if (item is Enum enumValue) {
                arrayElement.Add(SerializeEnumValue(enumValue));
            }
        }

        return arrayElement;
    }

    private static IdentifierElement SerializeEnumValue(Enum enumValue) {
        var value = enumValue.ToString();
        return new IdentifierElement(value);
    }

    private static IdentifierElement SerializeEnumValue<TEnum>(TEnum enumValue) where TEnum : Enum {
        var value = enumValue.ToString();
        return new IdentifierElement(value);
    }

    private static object DeserializeEnumValue(IdentifierElement identifierElement, Type enumType) {
        return Enum.Parse(enumType, identifierElement.Value);
    }

    private static TEnum DeserializeEnumValue<TEnum>(IdentifierElement identifierElement) where TEnum : struct, Enum {
        return Enum.Parse<TEnum>(identifierElement.Value);
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

    private static object DeserializeArrayFromTuple(TupleElement tupleElement, Type targetType, XferSerializerSettings settings) {
        Type? elementType = targetType.GetElementType();

        if (elementType == null) {
            throw new InvalidOperationException($"Unable to determine element type for array.");
        }

        var values = new List<object?>();
        foreach (var item in tupleElement.Values) {
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
        var valueDict = objectElement.Dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

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

        // Set remaining writable properties (not set via constructor) and apply capture-attributes on target props
        foreach (var prop in properties)
        {
            if (!prop.CanWrite) {
                continue;
            }

            var xferNameAttr = prop.GetCustomAttribute<XferPropertyAttribute>();
            var propName = xferNameAttr?.Name ?? prop.Name;
            // Only set if not already set by constructor
            if (valueDict.TryGetValue(propName, out var rawValue)) {
                var propValue = DeserializeValue(rawValue, prop.PropertyType, settings);
                prop.SetValue(instance, propValue);
            }
        }

        // Second pass: handle capture attributes that now live on the TARGET properties
        foreach (var targetProp in properties)
        {
            if (!targetProp.CanWrite) { continue; }

            var tagCap = targetProp.GetCustomAttribute<XferCaptureTagAttribute>();
            var idCap = targetProp.GetCustomAttribute<XferCaptureIdAttribute>();
            if (tagCap != null && idCap != null) {
                throw new InvalidOperationException($"Property '{targetType.FullName}.{targetProp.Name}' cannot have both XferCaptureTag and XferCaptureId.");
            }

            if (tagCap != null) {
                try {
                    // Resolve the SOURCE: either a CLR property by that name, or treat as document key directly
                    string configured = tagCap.TargetPropertyName;
                    string sourcePropName;
                    var sourceProp = properties.FirstOrDefault(p => string.Equals(p.Name, configured, StringComparison.Ordinal));
                    if (sourceProp != null) {
                        var sourceNameAttr = sourceProp.GetCustomAttribute<XferPropertyAttribute>();
                        sourcePropName = sourceNameAttr?.Name ?? sourceProp.Name;
                    } else {
                        sourcePropName = configured; // assume explicit document key
                    }

                    List<string> tags = new();
                    if (objectElement.Dictionary.TryGetValue(sourcePropName, out var kvpForTag)) {
                        if (kvpForTag.Tags != null && kvpForTag.Tags.Count > 0) { tags.AddRange(kvpForTag.Tags); }
                    } else {
                        var match = objectElement.Dictionary.FirstOrDefault(k => string.Equals(k.Key, sourcePropName, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(match.Key) && match.Value.Tags != null && match.Value.Tags.Count > 0) { tags.AddRange(match.Value.Tags); }
                    }

                    if (targetProp.PropertyType == typeof(string)) {
                        targetProp.SetValue(instance, tags.FirstOrDefault());
                    } else if (targetProp.PropertyType == typeof(List<string>)) {
                        targetProp.SetValue(instance, tags);
                    } else if (targetProp.PropertyType == typeof(string[])) {
                        targetProp.SetValue(instance, tags.ToArray());
                    }
                } catch { /* non-fatal: ignore tag capture errors */ }
            }

            if (idCap != null) {
                try {
                    string configured = idCap.TargetPropertyName;
                    string sourcePropName;
                    var sourceProp = properties.FirstOrDefault(p => string.Equals(p.Name, configured, StringComparison.Ordinal));
                    if (sourceProp != null) {
                        var sourceNameAttr = sourceProp.GetCustomAttribute<XferPropertyAttribute>();
                        sourcePropName = sourceNameAttr?.Name ?? sourceProp.Name;
                    } else {
                        sourcePropName = configured; // assume explicit document key
                    }

                    string? id = null;
                    if (objectElement.Dictionary.TryGetValue(sourcePropName, out var kvpForId)) {
                        id = kvpForId.Id;
                    } else {
                        var match = objectElement.Dictionary.FirstOrDefault(k => string.Equals(k.Key, sourcePropName, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(match.Key)) { id = match.Value.Id; }
                    }

                    if (targetProp.PropertyType == typeof(string)) {
                        targetProp.SetValue(instance, id);
                    }
                } catch { /* non-fatal: ignore id capture errors */ }
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

    #region Async File Operations

    /// <summary>
    /// Asynchronously serializes an object to a XferLang file with default settings.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToFileAsync<T>(T value, string filePath, CancellationToken cancellationToken = default)
    {
        await SerializeToFileAsync(value, filePath, DefaultSettings, Formatting.None, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a XferLang file with formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToFileAsync<T>(T value, string filePath, Formatting formatting, CancellationToken cancellationToken = default)
    {
        await SerializeToFileAsync(value, filePath, DefaultSettings, formatting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a XferLang file with custom settings and formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="settings">Serializer settings to control serialization behavior.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToFileAsync<T>(T value, string filePath, XferSerializerSettings settings, Formatting formatting = Formatting.None, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        using var writer = new StreamWriter(fileStream, Encoding.UTF8);

        await SerializeAsync(value, writer, settings, formatting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes a XferLang file to an object of type T using default settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        return await DeserializeFromFileAsync<T>(filePath, DefaultSettings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes a XferLang file to an object of type T using custom settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeFromFileAsync<T>(string filePath, XferSerializerSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        using var reader = new StreamReader(fileStream, Encoding.UTF8);

        return await DeserializeAsync<T>(reader, settings, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Async Stream Operations

    /// <summary>
    /// Asynchronously serializes an object to a TextWriter with default settings.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeAsync<T>(T value, TextWriter writer, CancellationToken cancellationToken = default)
    {
        await SerializeAsync(value, writer, DefaultSettings, Formatting.None, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a TextWriter with formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeAsync<T>(T value, TextWriter writer, Formatting formatting, CancellationToken cancellationToken = default)
    {
        await SerializeAsync(value, writer, DefaultSettings, formatting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a TextWriter with custom settings and formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="settings">Serializer settings to control serialization behavior.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeAsync<T>(T value, TextWriter writer, XferSerializerSettings settings, Formatting formatting = Formatting.None, CancellationToken cancellationToken = default)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var element = SerializeValue(value, settings);
        var xferString = element.ToXfer(formatting);

        await writer.WriteAsync(xferString).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes XferLang content from a TextReader to an object of type T using default settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="reader">The TextReader to read from.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default)
    {
        return await DeserializeAsync<T>(reader, DefaultSettings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes XferLang content from a TextReader to an object of type T using custom settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="reader">The TextReader to read from.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeAsync<T>(TextReader reader, XferSerializerSettings settings, CancellationToken cancellationToken = default)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        // Use the synchronous Parse method for now - we'll enhance this in Phase 2
        var document = XferParser.Parse(content);

        if (document is null || !document.Root.Values.Any())
        {
            return default;
        }

        return Deserialize<T>(document, settings);
    }

    /// <summary>
    /// Asynchronously serializes an object to a Stream with UTF-8 encoding using default settings.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The Stream to write to.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToStreamAsync<T>(T value, Stream stream, CancellationToken cancellationToken = default)
    {
        await SerializeToStreamAsync(value, stream, DefaultSettings, Formatting.None, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a Stream with UTF-8 encoding and formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The Stream to write to.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToStreamAsync<T>(T value, Stream stream, Formatting formatting, CancellationToken cancellationToken = default)
    {
        await SerializeToStreamAsync(value, stream, DefaultSettings, formatting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously serializes an object to a Stream with UTF-8 encoding, custom settings and formatting.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The Stream to write to.</param>
    /// <param name="settings">Serializer settings to control serialization behavior.</param>
    /// <param name="formatting">Controls indentation and formatting of the output.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task SerializeToStreamAsync<T>(T value, Stream stream, XferSerializerSettings settings, Formatting formatting = Formatting.None, CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        await SerializeAsync(value, writer, settings, formatting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes XferLang content from a Stream to an object of type T using default settings.
    /// Assumes UTF-8 encoding.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="stream">The Stream to read from.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeFromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        return await DeserializeFromStreamAsync<T>(stream, DefaultSettings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes XferLang content from a Stream to an object of type T using custom settings.
    /// Assumes UTF-8 encoding.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="stream">The Stream to read from.</param>
    /// <param name="settings">Serializer settings to control deserialization behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the deserialized object.</returns>
    public static async Task<T?> DeserializeFromStreamAsync<T>(Stream stream, XferSerializerSettings settings, CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
        return await DeserializeAsync<T>(reader, settings, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Async Try-Pattern Methods

    /// <summary>
    /// Attempts to asynchronously serialize an object to a file. Returns success status without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates success or failure.</returns>
    public static async Task<bool> TrySerializeToFileAsync<T>(T value, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await SerializeToFileAsync(value, filePath, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw cancellation
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to asynchronously deserialize a file to an object of type T. Returns result with success status without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the success status and the deserialized object if successful.</returns>
    public static async Task<(bool Success, T? Value)> TryDeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await DeserializeFromFileAsync<T>(filePath, cancellationToken).ConfigureAwait(false);
            return (true, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw cancellation
        }
        catch
        {
            return (false, default);
        }
    }

    /// <summary>
    /// Attempts to asynchronously serialize an object to a TextWriter. Returns success status without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates success or failure.</returns>
    public static async Task<bool> TrySerializeAsync<T>(T value, TextWriter writer, CancellationToken cancellationToken = default)
    {
        try
        {
            await SerializeAsync(value, writer, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw cancellation
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to asynchronously deserialize XferLang content from a TextReader to an object of type T. Returns result with success status without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="reader">The TextReader to read from.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the success status and the deserialized object if successful.</returns>
    public static async Task<(bool Success, T? Value)> TryDeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await DeserializeAsync<T>(reader, cancellationToken).ConfigureAwait(false);
            return (true, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw cancellation
        }
        catch
        {
            return (false, default);
        }
    }

    #endregion
}
