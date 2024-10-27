using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PropertyBagElement : Element {
    public const char OpeningMarker = '(';
    public const char ClosingMarker = ')';

    private List<Element> _items = new();

    public IEnumerable<Element> Values { 
        get { 
            return _items; 
        } 
    }

    public PropertyBagElement() : base("propertyBag", new(OpeningMarker, ClosingMarker)) { }

    public PropertyBagElement(IEnumerable<Element> values) : this() {
        _items.AddRange(values);
    }

    public PropertyBagElement(params Element[] values) : this() {
        _items.AddRange(values);
    }

    public void Add(Element element) {
        _items.Add(element);
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var item in _items) {
            sb.Append(item.ToString());
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
