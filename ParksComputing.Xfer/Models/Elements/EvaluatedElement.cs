using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class EvaluatedElement : TextElement {
    public static readonly string ElementName = "eval";
    public const char OpeningSpecifier = '`';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public EvaluatedElement(string text, int specifierCount = 1) : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount)) { 
    }
}
