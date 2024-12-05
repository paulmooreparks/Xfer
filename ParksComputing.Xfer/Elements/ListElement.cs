using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public abstract class ListElement : CollectionElement {
    protected List<Element> _items = new();

    protected ListElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _items.Count;

    public override Element GetElementAt(int index) => index < Count ? _items[index] : throw new InvalidOperationException("No element at index {index}");

    public override void Add(Element element) => _items.Add(element);

    public override string ToString() {
        return string.Join(" ", _items);
    }

    public IEnumerable<Element> Values {
        get {
            return _items;
        }
    }

    public Element this[int index] {
        get {
            return _items[index];
        }
        set {
            _items[index] = value;
        }
    }


}
