using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public abstract class CollectionElement : Element {
    protected CollectionElement(string name, Delimiter delimiter) : base(name, delimiter) {
    }

    public abstract void Add(Element element);
}
