using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.Impl;
public static class HttpGetApiExtension {
    public static string GetStatus(this XferKitApi xk) {
        return "OK";
    }
}

