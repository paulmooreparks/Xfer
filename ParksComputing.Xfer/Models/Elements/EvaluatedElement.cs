using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class EvaluatedElement : TextElement {
    public static readonly string ElementName = "eval";
    public const char OpeningMarker = '_';
    public const char ClosingMarker = OpeningMarker;

    public EvaluatedElement(string text, int markerCount = 1) : base(text, ElementName, new(OpeningMarker, ClosingMarker, markerCount)) { 
    }
}
