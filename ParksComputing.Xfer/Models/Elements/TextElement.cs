using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class TextElement : TypedElement<string> {
    public TextElement(string text, string name, ElementDelimiter delimiter) : base(text, name, delimiter) {
    }

    public override string ToString() {
        if (Delimiter.Style == ElementStyle.Bare) {
            return $"{Value}";
        }
        if (Delimiter.Style == ElementStyle.Minimized) {
            return $"{Delimiter.OpeningSpecifier}{Value}{Delimiter.OpeningSpecifier}";
        }
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
