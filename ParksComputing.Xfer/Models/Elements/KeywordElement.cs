using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : TextElement {
    public static readonly string ElementName = "keyword";

    public KeywordElement(string text) : base(text, ElementName, new()) { 
    }

    public override string ToString() {
        return $"{Value}";
    }
}
