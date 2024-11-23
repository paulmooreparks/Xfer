using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PropertyBagElement : Element {
    public static readonly string ElementName = "propertyBag";
    public const char OpeningMarker = '(';
    public const char ClosingMarker = ')';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    private List<Element> _items = new();

    public IEnumerable<Element> Values { 
        get { 
            return _items; 
        } 
    }

    public List<object> TypedValue {
        get {
            List<object> values = new (_items.Count);
            for (int i = 0; i < _items.Count; i++) {
                values[i] = _items[i];
            }
            return values;
        }
    }

    public string Value {
        get {
            return string.Join(", ", TypedValue);
        }
    }

    public PropertyBagElement(ElementStyle style = ElementStyle.Normal) 
        : base(ElementName, new(OpeningMarker, ClosingMarker, style)) 
    { 
    }

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
