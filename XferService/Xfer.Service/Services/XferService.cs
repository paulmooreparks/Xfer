using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace Xfer.Service.Services;

public class XferService {
    public static string Serialize<T>(T data) {
        var xferDocument = XferConvert.Serialize(data!, Formatting.Pretty);
        return xferDocument;
    }

    public static T? Deserialize<T>(string xfer) where T : new() {
        return XferConvert.Deserialize<T>(xfer);
    }
}
