﻿namespace ParksComputing.Xfer.Extensions;

public static class CharExtensions {
    // Extension method for checking if a character is a valid keyword character.
    public static bool IsKeywordChar(this char c) {
        return char.IsLetterOrDigit(c) || c == '_' || c == '-';
    }
}
