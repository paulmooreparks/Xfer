using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public abstract class TypedElement<T> : Element
{
    public virtual T Value { get; set; }

    public TypedElement(T value, string name, ElementDelimiter delimiter) : base(name, delimiter)
    {
        Value = value;
    }

    public override string ToXfer()
    {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}

