using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class XferElement
{
    public string Name { get; }
    public Delimiter Delimiter { get; set; }

    public XferElement(string name, Delimiter delimiter)
    {
        Name = name;
        Delimiter = delimiter;
    }

    public override string ToString()
    {
        return $"{Delimiter.Opening} {Delimiter.Closing}";
    }
}
