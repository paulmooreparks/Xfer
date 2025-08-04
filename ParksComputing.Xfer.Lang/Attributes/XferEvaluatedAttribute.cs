using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Indicates that a property should be serialized as an evaluated element in XferLang.
/// Evaluated elements use angle bracket delimiters and their content is processed
/// for dynamic content, variable substitution, or expressions during serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class XferEvaluatedAttribute : Attribute {
}
