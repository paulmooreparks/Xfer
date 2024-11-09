using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class EvaluatedElement : Element {
    public static readonly string ElementName = "eval";
    public const char OpeningMarker = '_';
    public const char ClosingMarker = OpeningMarker;

    public override string Value { get; } = string.Empty;

    public EvaluatedElement(string text, int markerCount = 1) : base(ElementName, new(OpeningMarker, ClosingMarker, markerCount)) { 
        Value = text;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
