using System.Text;

namespace ParksComputing.Xfer.Lang.Services;
public interface IXferParser {
    Encoding Encoding { get; }

    XferDocument Parse(byte[] input);
    XferDocument Parse(string input);
}