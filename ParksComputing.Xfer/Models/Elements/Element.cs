using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class Element {
    public const char HexadecimalPrefix = '$';
    public const char BinaryPrefix = '%';
    public const char ElementOpeningSpecifier = '<';
    public const char ElementClosingSpecifier = '>';
    public string Name { get; }
    public ElementDelimiter Delimiter { get; set; } = new ElementDelimiter('\0', '\0');

    public Element(string name, ElementDelimiter delimiter) {
        Name = name;
        Delimiter = delimiter;
    }
}
