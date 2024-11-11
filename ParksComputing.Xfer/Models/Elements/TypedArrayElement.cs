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


    public TypedArrayElement() : base(ElementName, new(OpeningMarker, ClosingMarker)) {
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

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var item in _items) {
            sb.Append(item.ToString());
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }

    public override void Add(Element element) {
        if (element is not T typedElement) {
            throw new InvalidOperationException($"Element type {element.GetType().Name} does not match expected type {_elementType.Name}.");
        }

        Add(typedElement);
    }
}

