using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class ArrayElement : CollectionElement {
    public static readonly string ElementName = "array";
    public const char OpeningMarker = '[';
    public const char ClosingMarker = ']';

    private List<Element> _items = new();
    private Type? _elementType;

    public Element[] Value { 
        get {
            return _items.ToArray();
        }
    }

    public override IEnumerable<Element> Values {
        get {
            return _items;
        }
    }

    public Element this[int index] {
        get {
            return _items[index];
        }
        set {
            // Enforce that the element type matches the existing array type
            if (_elementType != null && value.GetType() != _elementType) {
                throw new InvalidOperationException($"Element type {value.GetType().Name} does not match expected type {_elementType.Name}.");
            }

            _items[index] = value; 
        }
    }

    public ArrayElement() : base(ElementName, new(OpeningMarker, ClosingMarker)) { }

    public ArrayElement(IEnumerable<Element> values) : this() {
        _items.AddRange(values);
    }

    public ArrayElement(params Element[] values) : this() {
        _items.AddRange(values);
    }

    public override void Add(Element element) {
        if (_elementType == null) {
            // Set the element type if this is the first element being added
            _elementType = element.GetType();
        }
        else if (element.GetType() != _elementType) {
            // Enforce type matching for subsequent elements
            throw new InvalidOperationException($"Element type {element.GetType().Name} does not match expected type {_elementType.Name}.");
        }

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
