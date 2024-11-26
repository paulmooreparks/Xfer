using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class TypedArrayElement<T> : ArrayElement where T : Element {
    private Type _elementType = typeof(T);

    private List<T> _items = new();

    public T[] Value {
        get {
            return _items.ToArray();
        }
    }

    public override IEnumerable<T> Values {
        get {
            return _items;
        }
    }

    public T this[int index] {
        get {
            return _items[index];
        }
        set {
            _items[index] = value;
        }
    }


    public TypedArrayElement(ElementStyle style = ElementStyle.Compact) 
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style)) 
    {
    }

    public TypedArrayElement(IEnumerable<T> values) : this() {
        _items.AddRange(values);
    }

    public TypedArrayElement(params T[] values) : this() {
        _items.AddRange(values);
    }

    public void Add(T element) {
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
        foreach (var item in _items) {
            sb.Append(item.ToXfer());
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

    public override void Add(Element element) {
        if (element is not T typedElement) {
            throw new InvalidOperationException($"Element type {element.GetType().Name} does not match expected type {_elementType.Name}.");
        }

        Add(typedElement);
    }
}

