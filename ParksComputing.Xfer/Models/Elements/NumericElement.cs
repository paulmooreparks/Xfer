using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public abstract class NumericElement<T> : TypedElement<T> {
    public NumericElement(T value, string name, ElementDelimiter delimiter) : base(value, name, delimiter) {
    }

    public override string ToString() {
        if (Delimiter.Style == ElementStyle.Bare) {
            return $"{Value} ";
        }
        if (Delimiter.Style == ElementStyle.Minimized) {
            return $"{Delimiter.OpeningSpecifier}{Value} ";
        }
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}