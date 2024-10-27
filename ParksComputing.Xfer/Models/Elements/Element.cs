using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class Element
{
    public const char ElementOpeningMarker = '<';
    public const char ElementClosingMarker = '>';

    public string Name { get; }
    public Delimiter Delimiter { get; set; } = new Delimiter(ElementOpeningMarker, ElementClosingMarker);

    public Element(string name, Delimiter delimiter)
    {
        Name = name;
        Delimiter = delimiter;
    }

    public override string ToString()
    {
        return $"{Delimiter.Opening} {Delimiter.Closing}";
    }
}
