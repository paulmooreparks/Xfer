using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for assigning unique identifiers to elements in XferLang.
/// The ID processing instruction associates a string identifier with an element,
/// enabling element referencing and uniqueness validation within the document.
/// </summary>
public class IdProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "id";
    public IdProcessingInstruction(TextElement value) : base(value, Keyword) { }

    public override void ElementHandler(Element element) {
        element.Id = Value.ToString();
    }
}
