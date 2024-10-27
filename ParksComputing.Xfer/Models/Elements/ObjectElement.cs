using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class ObjectElement : Element {
    public Dictionary<string, Element> Values { get; set; } = new ();

    public const char OpeningMarker = '{';
    public const char ClosingMarker = '}';

    public ObjectElement() : base("object", new(OpeningMarker, ClosingMarker)) { }

    public void Add(KeyValuePairElement value) {
        Values.Add(value.Key.Value, value.Value);
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var kvp in Values) {
            sb.Append($"{Services.Parser.ElementOpeningMarker}{StringElement.OpeningMarker}{kvp.Key}{StringElement.ClosingMarker}{Services.Parser.ElementClosingMarker}{kvp.Value}");
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
