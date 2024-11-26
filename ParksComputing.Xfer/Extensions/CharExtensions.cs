using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xfer.Extensions;

public static class CharExtensions {
    // Extension method for checking if a character is a valid keyword character.
    public static bool IsKeywordChar(this char c) {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    public static bool IsIntegerLeadingChar(this char c) {
        return char.IsNumber(c) || c == Element.HexadecimalPrefix || c == Element.BinaryPrefix || c == '+' || c == '-';
    }
}
