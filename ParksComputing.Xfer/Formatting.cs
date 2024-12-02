using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer;
public enum Formatting {
    None = 0x00,
    Indented = 0x01,
    Spaced = 0x10,
    Pretty = Indented | Spaced
}
