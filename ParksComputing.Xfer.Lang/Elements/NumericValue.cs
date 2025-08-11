namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a typed numeric value together with its <see cref="NumericBase"/> and a flag
/// indicating whether an explicit value was assigned. This wrapper is used by numeric
/// element types to retain formatting intent (hex / binary) even if the underlying integer
/// value can be represented in multiple ways.
/// </summary>
/// <typeparam name="T">An integral convertible value type (e.g., int, long)</typeparam>
public class NumericValue<T> where T : struct, IConvertible {
    private T _value = default;

    /// <summary>
    /// Gets or sets the underlying numeric value. Setting a value marks <see cref="HasValue"/> true.
    /// </summary>
    public T Value {
        get { return _value; }
        set { HasValue = true; _value = value; }
    }

    /// <summary>
    /// Gets or sets the numeric base used for serialization / display.
    /// </summary>
    public NumericBase Base { get; set; } = NumericBase.Decimal;

    /// <summary>
    /// True once a value has been explicitly assigned (distinguishes default construction).
    /// </summary>
    public bool HasValue { get; internal set; } = false;

    /// <summary>
    /// Initializes a new instance with the supplied value and optional base.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="numericBase">The numeric base (default decimal).</param>
    public NumericValue(T value, NumericBase numericBase = NumericBase.Decimal) {
        Value = value;
        Base = numericBase;
    }

    /// <summary>
    /// Initializes a new instance with no explicit value (HasValue remains false until set).
    /// </summary>
    public NumericValue() { }

    /// <summary>
    /// Returns a string representing the value with an optional prefix ($ for hex, % for binary).
    /// </summary>
    public override string ToString() {
        string prefix = Base switch {
            NumericBase.Hexadecimal => "$",
            NumericBase.Binary => "%",
            _ => ""
        };

        var stringValue = Base switch {
            NumericBase.Hexadecimal => Convert.ToInt32(Value).ToString("X"),
            NumericBase.Binary => Convert.ToString(Convert.ToInt32(Value), 2),
            _ => Value.ToString()
        };

        return $"{prefix}{stringValue}";
    }
}
