using System.Collections.Generic;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Elements
{
    // DocumentElement: root element for XferDocument, always present, tuple semantics
    public class DocumentElement : TupleElement
    {
        public new static readonly string ElementName = "document";
        public DocumentElement() : base(ElementStyle.Implicit) { }
        // Optionally override ToXfer if you want document-specific formatting
    }
}
