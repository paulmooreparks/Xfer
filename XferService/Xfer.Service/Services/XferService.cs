using ParksComputing.Xfer;
using ParksComputing.Xfer.Elements;
using ParksComputing.Xfer.Services;

namespace Xfer.Service.Services;

public class XferService {
    public static string Serialize<T>(T data) {
        var xferDocument = XferConvert.Serialize(data!, Formatting.Pretty);
        return xferDocument;
    }

    public static T Deserialize<T>(string xfer) where T : new() {
        return XferConvert.Deserialize<T>(xfer);
    }
}
