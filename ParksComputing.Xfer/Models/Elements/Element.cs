using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class Element {
    public const char HexadecimalPrefix = '$';
    public const char BinaryPrefix = '%';
    public const char ElementOpeningCharacter = '<';
    public const char ElementClosingCharacter = '>';
    public string Name { get; }
    public ElementDelimiter Delimiter { get; set; } = new ElementDelimiter('\0', '\0');

    public Element(string name, ElementDelimiter delimiter) {
        Name = name;
        Delimiter = delimiter;
    }

    public abstract string ToXfer();

    public abstract string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0);
}
