using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class TextElement : TypedElement<string> {
    public TextElement(string text, string name, Delimiter delimiter) : base(text, name, delimiter) {
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
