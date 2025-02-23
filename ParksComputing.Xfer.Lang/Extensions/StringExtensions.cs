namespace ParksComputing.Xfer.Lang.Extensions;

internal static class StringExtensions {
    public static bool IsKeyword(this string compare, IDictionary<string, string> keywords, out string? keyword) {
        return keywords.TryGetValue(compare, out keyword);
    }

    public static bool IsKeywordString(this string input) {
        for (int i = 0; i < input.Length; i++) {
            if (!input[i].IsKeywordChar()) {
                return false;
            }
        }
        return true;
    }
}
