using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class EmptyElement : Element {
    public static readonly string ElementName = "empty";
    public EmptyElement() : base(ElementName, new('\0', '\0')) { }
}
