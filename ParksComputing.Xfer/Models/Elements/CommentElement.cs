using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CommentElement : Element {
    public const char OpeningMarker = '!';
    public const char ClosingMarker = OpeningMarker;

    public CommentElement() : base("comment", new(OpeningMarker, ClosingMarker)) { }
}
