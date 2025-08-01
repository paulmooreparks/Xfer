using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;
public class IdProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "id";
    public IdProcessingInstruction(TextElement value) : base(value, Keyword) { }
}
