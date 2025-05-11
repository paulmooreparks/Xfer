using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang;

public class XferParser {
    private static IXferParser _parser;

    static XferParser() {
        _parser = new Parser();
    }

    public static XferDocument Parse(byte[] input) => _parser.Parse(input);
    public static XferDocument Parse(string input) => _parser.Parse(input);
}
