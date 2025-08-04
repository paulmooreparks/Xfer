using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace Xfer.Service.Services;

public class XferService {
    public static string Serialize<T>(T data) {
        return XferConvert.Serialize(data, Formatting.Pretty);
    }

    public static string SerializeCompact<T>(T data) {
        return XferConvert.Serialize(data, Formatting.None);
    }

    public static T? Deserialize<T>(string xfer) {
        return XferConvert.Deserialize<T>(xfer);
    }

    public static object? Deserialize(string xfer, Type type) {
        return XferConvert.Deserialize(xfer, type);
    }

    public static bool TryDeserialize<T>(string xfer, out T? result) {
        try {
            result = XferConvert.Deserialize<T>(xfer);
            return true;
        }
        catch {
            result = default;
            return false;
        }
    }

    public static bool TrySerialize<T>(T data, out string? result) {
        try {
            result = XferConvert.Serialize(data, Formatting.Pretty);
            return true;
        }
        catch {
            result = null;
            return false;
        }
    }
}
