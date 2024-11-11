using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class ArrayElement : Element {
    public static readonly string ElementName = "array";
    public const char OpeningMarker = '[';
    public const char ClosingMarker = ']';

    public abstract IEnumerable<Element> Values { get; }

    public abstract void Add(Element element);

    public ArrayElement(string name, Delimiter delimiter) : base(ElementName, new(OpeningMarker, ClosingMarker)) { }
}
