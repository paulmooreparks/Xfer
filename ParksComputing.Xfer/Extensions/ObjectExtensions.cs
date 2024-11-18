using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Extensions;

public static class ObjectExtensions {
    public static string ToXfer(this object obj) {
        string xfer = XferConvert.Serialize(obj);
        return xfer;
    }
}
