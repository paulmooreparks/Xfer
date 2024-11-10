using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class EmptyElement : TypedElement<string> {
    public static readonly string ElementName = "empty";
    public override string Value => string.Empty;
    public EmptyElement() : base(string.Empty, ElementName, new('\0', '\0')) { }
}
