using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

public class XferParser {
    private static readonly ThreadLocal<IXferParser> _parser =
        new(() => new Parser());

    public static XferDocument Parse(byte[] input) => _parser.Value!.Parse(input);
    public static XferDocument Parse(string input) => _parser.Value!.Parse(input);
}
