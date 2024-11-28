using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PropertyBagElement : Element {
    public static readonly string ElementName = "propertyBag";
    public const char OpeningSpecifier = '(';
    public const char ClosingSpecifier = ')';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

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

    public PropertyBagElement(ElementStyle style = ElementStyle.Compact) 
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style)) 
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

    public override string ToXfer() {
        var sb = new StringBuilder();
        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Opening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinOpening);
                break;
        }

        /* TODO: Whitespace between elements can be removed in a few situations by examining the delimiter style of the surrounding elements. */
        for (var i = 0; i < _items.Count(); ++i) {
            var item = _items[i];
            sb.Append(item.ToXfer());
            if (item.Delimiter.Style is ElementStyle.Implicit or ElementStyle.Compact && i + 1 < _items.Count()) {
                sb.Append(' ');
            }
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Closing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinClosing);
                break;
        }
        return sb.ToString();
    }

    public override string ToString() {
        return ToXfer();
    }
}
