namespace ParksComputing.Xfer.Lang.Elements;

public class NumericValue<T> where T : struct, IConvertible {
    T _value = default;

    public T Value {
        get { return _value; }
        set { HasValue = true; _value = value; }
    }
    public NumericBase Base { get; set; } = NumericBase.Decimal;

    public bool HasValue { get; internal set; } = false;

    public NumericValue(T value, NumericBase numericBase = NumericBase.Decimal) {
        Value = value;
        Base = numericBase;
    }

    public NumericValue() {
    }

    public override string ToString() {
        string prefix = Base switch {
            NumericBase.Hexadecimal => "$",
            NumericBase.Binary => "%",
            _ => "" // No prefix for decimal
        };

        var stringValue = Base switch {
            NumericBase.Hexadecimal => Convert.ToInt32(Value).ToString("X"),
            NumericBase.Binary => Convert.ToString(Convert.ToInt32(Value), 2),
            _ => Value.ToString()
        };

        return $"{prefix}{stringValue}";
    }
}
