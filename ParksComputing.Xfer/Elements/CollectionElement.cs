using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public abstract class CollectionElement<T> : Element {
    protected CollectionElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public abstract int Count { get; }

    public abstract T? GetElementAt(int index);

    public abstract bool Add(T element);
}
