using System;

namespace ParksComputing.Xfer.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class XferPropertyAttribute : Attribute {
    public string? Name { get; set; }

    public XferPropertyAttribute(string? name = null) {
        Name = name;
    }
}
