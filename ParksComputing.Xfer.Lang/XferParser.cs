using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

public class XferParser {
    public static XferDocument Parse(byte[] input) => new Parser().Parse(input);
    public static XferDocument Parse(string input) => new Parser().Parse(input);
}
