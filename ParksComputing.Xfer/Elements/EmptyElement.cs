using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public class EmptyElement : TypedElement<string>
{
    public static readonly string ElementName = "empty";
    public override string Value => string.Empty;
    public EmptyElement() : base(string.Empty, ElementName, new('\0', '\0')) { }

    public override string ToXfer() => string.Empty;
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) => string.Empty;
    public override string ToString() => string.Empty;
}
