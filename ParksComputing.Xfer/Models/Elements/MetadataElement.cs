using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class MetadataElement : XferElement
{
    public MetadataElement() : base("Metadata", new Delimiter('%')) { }
}
