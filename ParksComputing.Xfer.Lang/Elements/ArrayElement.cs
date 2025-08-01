using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang.Elements;

public class ArrayElement : CollectionElement
{
    public static readonly string ElementName = "array";
    public const char OpeningSpecifier = '[';
    public const char ClosingSpecifier = ']';
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    private List<Element> _items = [];
    private Type? _elementType = null; // Track the expected element type

    public override int Count => _items.Count;

    public override Element? GetElementAt(int index) => 
        index >= 0 && index < Count ? _items[index] : null;

    public override bool Add(Element element) {
        // Skip ProcessingInstructions for type checking - they're metadata
        if (element is ProcessingInstruction pi) {
            Children.Add(pi);
            pi.Parent = this;
            return true;
        }

        // Establish type from first non-PI element
        if (_elementType == null) {
            _elementType = element.GetType();
        }
        else {
            // Enforce homogeneous typing - all elements must be the same type
            if (element.GetType() != _elementType) {
                throw new InvalidOperationException(
                    $"Array elements must be of the same type. Expected {_elementType.Name}, but got {element.GetType().Name}. " +
                    $"Use TupleElement for mixed-type collections.");
            }
        }

        _items.Add(element);
        Children.Add(element);
        element.Parent = this;
        return true;
    }

    public Element[] Value {
        get {
            return [.. _items];
        }
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
            // Also validate the setter for type consistency
            if (_elementType != null && value.GetType() != _elementType) {
                throw new InvalidOperationException(
                    $"Array elements must be of the same type. Expected {_elementType.Name}, but got {value.GetType().Name}.");
            }
            
            // If this is the first element being set, establish the type
            if (_elementType == null) {
                _elementType = value.GetType();
            }
            
            _items[index] = value;
        }
    }

    /// <summary>
    /// Gets the element type for this homogeneous array, or null if empty.
    /// </summary>
    public Type? ElementType => _elementType;

    public ArrayElement(ElementStyle style = ElementStyle.Compact)
        : base(ElementName, new(OpeningSpecifier, ClosingSpecifier, style)) {
    }

    public ArrayElement(IEnumerable<Element> values) : this() {
        foreach (var value in values) {
            Add(value);
        }
    }

    public ArrayElement(params Element[] values) : this() {
        foreach (var value in values) {
            Add(value);
        }
    }

    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        bool isIndented = (formatting & Formatting.Indented) == Formatting.Indented;
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        string rootIndent = string.Empty;
        string nestIndent = string.Empty;

        var sb = new StringBuilder();

        if (isIndented) {
            rootIndent = new string(indentChar, indentation * depth);
            nestIndent = new string(indentChar, indentation * (depth + 1));
        }

        switch (Delimiter.Style) {
            case ElementStyle.Explicit:
                sb.Append(Delimiter.Opening);
                break;
            case ElementStyle.Compact:
                sb.Append(Delimiter.MinOpening);
                break;
        }

        if (isIndented) {
            sb.Append(Environment.NewLine);
        }

        // Output all children (valid elements and metadata) in order
        for (var i = 0; i < Children.Count; ++i) {
            var item = Children[i];
            if (isIndented) {
                sb.Append(nestIndent);
            }
            sb.Append(item.ToXfer(formatting, indentChar, indentation, depth + 1));
            if (item.Delimiter.Style is ElementStyle.Implicit or ElementStyle.Compact && i + 1 < Children.Count) {
                sb.Append(' ');
            }
            if (isIndented) {
                sb.Append(Environment.NewLine);
            }
        }

        if (isIndented) {
            sb.Append(rootIndent);
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
