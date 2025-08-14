using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Apply this to the ID TARGET property to capture the ID from another property during deserialization.
/// The attribute references the SOURCE property that carries the Xfer key/value pair.
/// </summary>
/// <remarks>
/// <para>Usage:</para>
/// <code><![CDATA[
/// class Foo {
///     public string Name { get; set; } = string.Empty;
///     [XferCaptureId(nameof(Name))]
///     public string? NameId { get; set; }
/// }
/// // Xfer snippet: { <! id "foo" !> name "bar" } => NameId == "foo"
/// ]]></code>
/// <para>Notes:</para>
/// <list type="bullet">
///   <item><description>If no ID is present, the target string remains null.</description></item>
///   <item><description>Property name matching is case-insensitive. The attribute argument may be the CLR property name (respects any [XferProperty] rename) or the exact document key.</description></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XferCaptureIdAttribute : Attribute {
    /// <summary>
    /// The SOURCE identifier: either the CLR property name on the same type, or the exact document key.
    /// The decorated target property's type should be string (nullable allowed).
    /// </summary>
    public string TargetPropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XferCaptureIdAttribute"/> class.
    /// </summary>
    /// <param name="sourceName">The SOURCE identifier to capture from: either a CLR property name on the same type (case-insensitive, honoring any <c>[XferProperty]</c> rename) or the exact document key.</param>
    public XferCaptureIdAttribute(string sourceName) {
        TargetPropertyName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
    }
}
