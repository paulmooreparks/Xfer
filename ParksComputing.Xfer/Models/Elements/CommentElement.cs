using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CommentElement : XferElement {
    public CommentElement() : base("Comment", new Delimiter('!')) { }
}
