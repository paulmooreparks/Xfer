using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public abstract class NumericElement<T> : TypedElement<T> {
    public NumericElement(T value, string name, ElementDelimiter delimiter) : base(value, name, delimiter) {
    }

    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit) {
            sb.Append($"{Value}");
        }
        else if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.OpeningSpecifier}{Value}");
        }
        else {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }

    public override string ToString() {
        return Value?.ToString() ?? string.Empty;
    }
}