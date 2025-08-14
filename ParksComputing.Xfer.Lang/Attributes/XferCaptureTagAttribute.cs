using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Marks a property so that, during deserialization, the tag from the corresponding
/// XferLang key/value pair is captured into a sibling property.
/// </summary>
/// <remarks>
/// Usage:
///   class Foo {
///     [XferCaptureTag(nameof(BarTag))]
///     public string Bar { get; set; } = string.Empty;
///     public string? BarTag { get; set; }
///   }
/// When deserializing an element like: { <! tag "Baz" !> Bar "Hi!" }, BarTag will be set to "Baz".
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XferCaptureTagAttribute : Attribute {
    /// <summary>
    /// The name of the property on the same type that should receive the tag value.
    /// The property should be writable and typically of type string (nullable allowed).
    /// </summary>
    public string TargetPropertyName { get; }

    public XferCaptureTagAttribute(string targetPropertyName) {
        TargetPropertyName = targetPropertyName ?? throw new ArgumentNullException(nameof(targetPropertyName));
    }
}
