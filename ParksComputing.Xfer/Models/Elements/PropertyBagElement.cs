﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PropertyBagElement : Element {
    public static readonly string ElementName = "propertyBag";
    public const char OpeningSpecifier = '(';
    public const char ClosingSpecifier = ')';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

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

    public PropertyBagElement(ElementStyle style = ElementStyle.Normal) 
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
        sb.Append(Delimiter.Opening);
        foreach (var item in _items) {
            sb.Append(item.ToXfer());
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }

    public override string ToString() {
        return ToXfer();
    }
}
