using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class ArrayElement : Element {
    public static readonly string ElementName = "array";
    public const char OpeningSpecifier = '[';
    public const char ClosingSpecifier = ']';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public abstract IEnumerable<Element> Values { get; }

    public abstract void Add(Element element);

    public ArrayElement(string name, ElementDelimiter delimiter) : base(ElementName, new(OpeningSpecifier, ClosingSpecifier)) { }
}
