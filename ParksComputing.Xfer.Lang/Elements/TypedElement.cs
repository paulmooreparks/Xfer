using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class for elements that store a strongly-typed value.
/// Provides generic functionality for elements that wrap specific .NET types
/// with appropriate XferLang formatting and delimiter handling.
/// </summary>
/// <typeparam name="T">The type of value stored in this element.</typeparam>
public abstract class TypedElement<T> : Element {
    /// <summary>
    /// Gets or sets the strongly-typed value stored in this element.
    /// </summary>
    public virtual T Value { get; set; }

    /// <summary>
    /// Gets the parsed value of this element for external evaluation.
    /// For typed elements, this returns the strongly-typed Value.
    /// </summary>
    public override object? ParsedValue {
        get => Value;
        set => throw new InvalidOperationException("ParsedValue is read-only for TypedElement. Set the Value property instead.");
    }

    /// <summary>
    /// Initializes a new TypedElement with the specified value, name, and delimiter configuration.
    /// </summary>
    /// <param name="value">The value to store in this element.</param>
    /// <param name="name">The element type name.</param>
    /// <param name="delimiter">The delimiter configuration for this element.</param>
    public TypedElement(T value, string name, ElementDelimiter delimiter) : base(name, delimiter)
    {
        Value = value;
    }

    /// <summary>
    /// Converts the typed element to its XferLang string representation.
    /// The default implementation wraps the value with the configured delimiters.
    /// </summary>
    /// <returns>The XferLang string representation of the typed value.</returns>
    public override string ToXfer()
    {
        return $"{Delimiter.ExplicitOpening}{Value}{Delimiter.ExplicitClosing}";
    }

    /// <summary>
    /// Returns the string representation of the stored value.
    /// </summary>
    /// <returns>The value as a string, or empty string if the value is null.</returns>
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}

