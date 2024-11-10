using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CommentElement : Element {
    public static readonly string ElementName = "comment";
    public const char OpeningMarker = '/';
    public const char ClosingMarker = OpeningMarker;

    public CommentElement() : base(ElementName, new(OpeningMarker, ClosingMarker)) { }
}
