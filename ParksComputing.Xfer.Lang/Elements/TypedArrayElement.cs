﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;
public class TypedArrayElement<T> : ArrayElement where T : Element
{
    private Type _elementType = typeof(T);

    private List<T> _items = new();

    public T[] Value
    {
        get
        {
            return _items.ToArray();
        }
    }

    public override IEnumerable<T> Values
    {
        get
        {
            return _items;
        }
    }

    public T this[int index]
    {
        get
        {
            return _items[index];
        }
        set
        {
            _items[index] = value;
        }
    }

    public TypedArrayElement(ElementStyle style = ElementStyle.Compact)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style))
    {
    }

    public TypedArrayElement(IEnumerable<T> values) : this()
    {
        _items.AddRange(values);
    }

    public TypedArrayElement(params T[] values) : this()
    {
        _items.AddRange(values);
    }

    public void Add(T element)
    {
        _items.Add(element);
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        bool isIndented = (formatting & Formatting.Indented) == Formatting.Indented;
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        string rootIndent = string.Empty;
        string nestIndent = string.Empty;

        var sb = new StringBuilder();

        if (isIndented)
        {
            rootIndent = new string(indentChar, indentation * depth);
            nestIndent = new string(indentChar, indentation * (depth + 1));
        }

        switch (Delimiter.Style)
        {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Opening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinOpening);
                break;
        }

        if (isIndented)
        {
            sb.Append(Environment.NewLine);
        }

        /* TODO: Whitespace between elements can be removed in a few situations by examining the delimiter style of the surrounding elements. */
        for (var i = 0; i < _items.Count(); ++i)
        {
            var item = _items[i];
            if (isIndented)
            {
                sb.Append(nestIndent);
            }
            sb.Append(item.ToXfer(formatting, indentChar, indentation, depth + 1));
            if (item.Delimiter.Style is ElementStyle.Implicit or ElementStyle.Compact && i + 1 < _items.Count())
            {
                sb.Append(' ');
            }
            if (isIndented)
            {
                sb.Append(Environment.NewLine);
            }
        }

        if (isIndented)
        {
            sb.Append(rootIndent);
        }

        switch (Delimiter.Style)
        {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Closing);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinClosing);
                break;
        }

        return sb.ToString();
    }

    public override string ToString()
    {
        return ToXfer();
    }

    public override void Add(Element element)
    {
        if (element is not T typedElement)
        {
            throw new InvalidOperationException($"Element type {element.GetType().Name} does not match expected type {_elementType.Name}.");
        }

        Add(typedElement);
    }
}

