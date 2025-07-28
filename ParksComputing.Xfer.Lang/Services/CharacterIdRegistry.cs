using System;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Services;

public static class CharacterIdRegistry {
    private static readonly Dictionary<string, int> _builtin = new(StringComparer.OrdinalIgnoreCase) {
        { "nul", '\0' },
        { "cr", '\r' },
        { "lf", '\n' },
        { "nl", '\n' },
        { "tab", '\t' },
        { "vtab", '\v' },
        { "bksp", '\b' },
        { "esc", '\x1B' },
        { "ff", '\f' },
        { "bel", '\a' },
        { "quote", '"' },
        { "apos", '\'' },
        { "backslash", '\\' },
        { "lt", '<' },
        { "gt", '>' },
    };

    private static Dictionary<string, int> _custom = new(StringComparer.OrdinalIgnoreCase);

    public static void SetCustomIds(Dictionary<string, int> custom) {
        _custom = new Dictionary<string, int>(custom, StringComparer.OrdinalIgnoreCase);
    }

    public static int? Resolve(string id) {
        if (_custom.TryGetValue(id, out int value)) {
            return value;
        }

        if (_builtin.TryGetValue(id, out value)) {
            return value;
        }

        return null;
    }

    public static IReadOnlyDictionary<string, int> Builtin => _builtin;
    public static IReadOnlyDictionary<string, int> Custom => _custom;
}
