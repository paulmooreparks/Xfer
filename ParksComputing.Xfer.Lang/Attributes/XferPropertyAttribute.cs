using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Specifies the XferLang property name for a .NET property during serialization and deserialization.
/// When applied to a property, it overrides the default property name mapping behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class XferPropertyAttribute : Attribute {
    /// <summary>
    /// Gets or sets the custom name to use for this property in XferLang serialization.
    /// If null or not specified, the property's actual name will be used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XferPropertyAttribute"/> class.
    /// </summary>
    /// <param name="name">The custom name to use for this property, or null to use the property's actual name.</param>
    public XferPropertyAttribute(string? name = null) {
        Name = name;
    }
}
