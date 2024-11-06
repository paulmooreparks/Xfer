using System;

namespace ParksComputing.Xfer.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class XferLiteralAttribute : Attribute {
}
