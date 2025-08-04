using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents an array element in XferLang, containing an ordered collection of elements
/// enclosed in square brackets []. Arrays can contain elements of any type and maintain
/// insertion order. This class extends ListElement to provide array-specific functionality
/// including type tracking and validation.
/// </summary>
public class ArrayElement : ListElement {
    /// <summary>
    /// The element name used in XferLang serialization for array elements.
    /// </summary>
    public static readonly string ElementName = "array";

    /// <summary>
    /// The opening delimiter character (left square bracket) for array elements.
    /// </summary>
    public const char OpeningSpecifier = '[';

    /// <summary>
    /// The closing delimiter character (right square bracket) for array elements.
    /// </summary>
    public const char ClosingSpecifier = ']';

    /// <summary>
    /// The delimiter configuration for array elements using square bracket characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: ElementStyle.Compact);

    private Type? _elementType = null; // Track the expected element type

    /// <summary>
    /// Adds an element to this array with type validation.
    /// Enforces homogeneous typing - all elements must be the same type after the first element is added.
    /// Processing instructions and comments are added to the children collection without type checking.
    /// </summary>
    /// <param name="element">The element to add to the array</param>
    /// <returns>True if the element was added successfully, false if type validation failed</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add an element of a different type than the established array type</exception>
    public override bool Add(Element element) {
        if (element is ProcessingInstruction || element is CommentElement) {
            // Non-semantic: add only to Children
            if (!Children.Contains(element)) {
                Children.Add(element);
                element.Parent = this;
            }
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
        if (!Children.Contains(element)) {
            Children.Add(element);
            element.Parent = this;
        }
        return true;
    }


    /// <summary>
    /// Gets the element type for this homogeneous array, or null if empty.
    /// </summary>
    public Type? ElementType => _elementType;

    /// <summary>
    /// Gets or sets the element at the specified index with type validation.
    /// Setting an element enforces the homogeneous typing constraint of the array.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set</param>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to set an element of a different type than the established array type</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of bounds</exception>
    public new Element this[int index] {
        get { return _items[index]; }
        set {
            if (_elementType != null && value.GetType() != _elementType) {
                throw new InvalidOperationException($"Array elements must be of the same type. Expected {_elementType.Name}, but got {value.GetType().Name}.");
            }
            if (_elementType == null) {
                _elementType = value.GetType();
            }
            _items[index] = value;
        }
    }



    /// <summary>
    /// Initializes a new instance of the ArrayElement class with the specified element style.
    /// </summary>
    /// <param name="style">The element style for delimiter handling</param>
    public ArrayElement(ElementStyle style)
        : base(ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style: style)) {
    }

    /// <summary>
    /// Initializes a new instance of the ArrayElement class with a collection of elements.
    /// Elements are added with type validation to maintain homogeneous typing.
    /// </summary>
    /// <param name="values">The elements to add to the array</param>
    public ArrayElement(IEnumerable<Element> values)
        : base(ElementName, ElementDelimiter) {
        foreach (var value in values) {
            Add(value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the ArrayElement class with a variable number of elements.
    /// Elements are added with type validation to maintain homogeneous typing.
    /// </summary>
    /// <param name="values">The elements to add to the array</param>
    public ArrayElement(params Element[] values)
        : base(ElementName, ElementDelimiter) {
        foreach (var value in values) {
            Add(value);
        }
    }

    /// <summary>
    /// Serializes this array element to its XferLang string representation using default formatting.
    /// </summary>
    /// <returns>The XferLang string representation of this array element</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Serializes this array element to its XferLang string representation with specified formatting.
    /// Uses square bracket delimiters and applies proper indentation for nested elements.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this array element</returns>
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

    /// <summary>
    /// Returns a string representation of this array element using its XferLang serialization.
    /// </summary>
    /// <returns>The string representation of this array element</returns>
    public override string ToString() {
        return ToXfer();
    }
}
