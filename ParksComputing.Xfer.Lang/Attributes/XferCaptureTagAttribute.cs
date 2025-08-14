using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Apply this to the TAG TARGET property to capture tag(s) from another property during deserialization.
/// The attribute references the SOURCE property that carries the Xfer key/value pair.
/// </summary>
/// <remarks>
/// <para>Usage (string target takes the first tag):</para>
/// <code><![CDATA[
/// class Foo {
///     public string Bar { get; set; } = string.Empty;
///     [XferCaptureTag(nameof(Bar))]
///     public string? BarTag { get; set; }
/// }
/// // Xfer: { <! tag "A" !> <! tag "B" !> Bar "Hi!" } => BarTag == "A"
/// ]]></code>
/// <para>Usage (List&lt;string&gt; target takes all tags):</para>
/// <code><![CDATA[
/// class FooList {
///     public string Bar { get; set; } = string.Empty;
///     [XferCaptureTag(nameof(Bar))]
///     public List<string>? BarTags { get; set; }
/// }
/// // Xfer: { <! tag "A" !> <! tag "B" !> Bar "Hi!" } => BarTags == ["A","B"]
/// ]]></code>
/// <para>Usage (string[] target takes all tags):</para>
/// <code><![CDATA[
/// class FooArray {
///     public string Bar { get; set; } = string.Empty;
///     [XferCaptureTag(nameof(Bar))]
///     public string[]? BarTags { get; set; }
/// }
/// // Xfer: { <! tag "A" !> <! tag "B" !> Bar "Hi!" } => BarTags == ["A","B"]
/// ]]></code>
/// <para>Notes:</para>
/// <list type="bullet">
///   <item><description>If no tags are present: string target => null; List&lt;string&gt; => empty list; string[] => empty array.</description></item>
///   <item><description>Property name matching is case-insensitive and respects any <c>[XferProperty]</c> rename; tag values are case-sensitive.</description></item>
///   <item><description>Duplicate tags are de-duplicated by the processor.</description></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XferCaptureTagAttribute : Attribute {
    /// <summary>
    /// The SOURCE identifier: either the CLR property name on the same type, or the exact document key.
    /// The decorated target property's type should be string, List&lt;string&gt;, or string[] (nullable allowed).
    /// </summary>
    public string TargetPropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XferCaptureTagAttribute"/> class.
    /// </summary>
    /// <param name="sourceName">The SOURCE identifier to capture from: either a CLR property name on the same type (case-insensitive, honoring any <c>[XferProperty]</c> rename) or the exact document key.</param>
    public XferCaptureTagAttribute(string sourceName) {
        TargetPropertyName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
    }
}
