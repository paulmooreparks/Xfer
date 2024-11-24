using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CommentElement : Element {
    public static readonly string ElementName = "comment";
    public const char OpeningSpecifier = '/';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public CommentElement() : base(ElementName, new(OpeningSpecifier, ClosingSpecifier)) { }
}
