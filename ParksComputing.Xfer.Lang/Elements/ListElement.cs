using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public abstract class ListElement : CollectionElement {
    /// <summary>
    /// Holds only semantic items (not PIs/comments)
    /// </summary>

    protected ListElement(string elementName, ElementDelimiter delimiter) : base(elementName, delimiter) { }

    public override int Count => _items.Count;

    public override Element? GetElementAt(int index) => index >= 0 && index < Count ? _items[index] : null;

    /// <summary>
    /// Add a semantic item. Non-semantic elements (PIs/comments) should be added to Children only.
    /// </summary>
    public override bool Add(Element element) {
        if (element is ParksComputing.Xfer.Lang.ProcessingInstructions.ProcessingInstruction || element is CommentElement) {
            // Non-semantic: add only to Children
            if (!Children.Contains(element)) {
                Children.Add(element);
                element.Parent = this;
            }
            return true;
        }
        _items.Add(element);
        if (!Children.Contains(element)) {
            Children.Add(element);
            element.Parent = this;
        }
        return true;
    }

    public override string ToString() {
        return string.Join(" ", _items);
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
