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
        // Unicode icon identifiers
        { "x", 0x274C },            // ❌ (alias for cross)
        // Common emoji
        { "smile", 0x1F604 },       // 😄
        { "grin", 0x1F601 },        // 😁
        { "joy", 0x1F602 },         // 😂
        { "wink", 0x1F609 },        // 😉
        { "heart", 0x2764 },        // ❤
        { "thumbsup", 0x1F44D },    // 👍
        { "thumbsdown", 0x1F44E },  // 👎
        { "clap", 0x1F44F },        // 👏
        { "fire", 0x1F525 },        // 🔥
        { "star", 0x2B50 },         // ⭐
        { "rocket", 0x1F680 },      // 🚀
        { "tada", 0x1F389 },        // 🎉
        { "eyes", 0x1F440 },        // 👀
        { "thinking", 0x1F914 },    // 🤔
        { "shrug", 0x1F937 },       // 🤷
        { "wave", 0x1F44B },        // 👋
        { "ok", 0x1F197 },          // 🆗
        // CI/build/task emoji
        { "lightbulb", 0x1F4A1 },   // 💡
        { "warning", 0x26A0 },      // ⚠
        { "check", 0x2705 },        // ✅
        { "cross", 0x274C },        // ❌
        { "hourglass", 0x23F3 },    // ⏳
        { "hammer", 0x1F528 },      // 🔨
        { "package", 0x1F4E6 },     // 📦
        { "construction", 0x1F6A7 },// 🚧
        { "bug", 0x1F41B },         // 🐛
        { "recycle", 0x267B },      // ♻
        { "gear", 0x2699 },         // ⚙
        { "wrench", 0x1F527 },      // 🔧
        { "memo", 0x1F4DD },        // 📝
        { "lock", 0x1F512 },        // 🔒
        { "unlock", 0x1F513 },      // 🔓
        { "pushpin", 0x1F4CC },     // 📌
        { "mag", 0x1F50D },         // 🔍
        { "repeat", 0x1F501 },      // 🔁
        { "repeatone", 0x1F502 },   // 🔂
        { "stop", 0x23F9 },         // ⏹
        { "play", 0x25B6 },         // ▶
        { "pause", 0x23F8 },        // ⏸
        { "fastforward", 0x23E9 },  // ⏩
        { "rewind", 0x23EA },       // ⏪
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
