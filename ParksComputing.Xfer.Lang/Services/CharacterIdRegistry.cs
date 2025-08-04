using System;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Services;

/// <summary>
/// Provides a registry for mapping character IDs to Unicode code points in XferLang.
/// Manages both built-in character definitions (ASCII control characters, common symbols)
/// and custom character mappings defined by users or documents.
/// </summary>
public static class CharacterIdRegistry {
    private static readonly Dictionary<string, int> _builtin = new(StringComparer.OrdinalIgnoreCase) {
        // ASCII control characters
        { "nul", '\0' },    // Null character
        { "soh", '\x01' },  // Start of Heading
        { "stx", '\x02' },  // Start of Text
        { "etx", '\x03' },  // End of Text
        { "eot", '\x04' },  // End of Transmission
        { "enq", '\x05' },  // Enquiry
        { "ack", '\x06' },  // Acknowledge
        { "bel", '\a' },    // Bell
        { "bksp", '\b' },   // Backspace
        { "tab", '\t' },    // Horizontal Tab
        { "vtab", '\v' },   // Vertical Tab
        { "lf", '\n' },     // Line Feed
        { "nl", '\n' },     // New line (alias for lf)
        { "cr", '\r' },     // Carriage Return
        { "ff", '\f' },     // Form Feed
        { "so", '\x0E' },   // Shift Out
        { "si", '\x0F' },   // Shift In
        { "dle", '\x10' },  // Data Link Escape
        { "dc1", '\x11' },  // Device Control 1 (XON)
        { "dc2", '\x12' },  // Device Control 2
        { "dc3", '\x13' },  // Device Control 3 (XOFF)
        { "dc4", '\x14' },  // Device Control 4
        { "nak", '\x15' },  // Negative Acknowledge
        { "syn", '\x16' },  // Synchronous Idle
        { "etb", '\x17' },  // End of Transmission Block
        { "can", '\x18' },  // Cancel
        { "em", '\x19' },   // End of Medium
        { "sub", '\x1A' },  // Substitute
        { "esc", '\x1B' },  // Escape
        { "fs", '\x1C' },   // File Separator
        { "gs", '\x1D' },   // Group Separator
        { "rs", '\x1E' },   // Record Separator
        { "us", '\x1F' },   // Unit Separator
        // Angle brackets
        { "lt", '<' },       // Less-than sign
        { "gt", '>' },       // Greater-than sign
        // Common currency symbols
        { "dollar", '$' },   // Dollar sign
        { "euro", '€' },     // Euro sign
        { "pound", '£' },    // Pound sterling
        { "yen", '¥' },      // Yen sign
        { "cent", '¢' },     // Cent sign
        { "rupee", '₹' },    // Indian Rupee
        { "won", '₩' },      // Korean Won
        { "franc", '₣' },    // French Franc
        { "peso", '₱' },     // Peso sign
        { "bitcoin", '₿' },  // Bitcoin symbol
        { "litecoin", 'Ł' }, // Litecoin symbol
        { "dogecoin", 'Ð' }, // Dogecoin symbol
        { "ruble", '₽' },    // Russian Ruble
        { "shekel", '₪' },   // Israeli Shekel
        { "dong", '₫' },     // Vietnamese Dong
        { "baht", '฿' },     // Thai Baht
        { "lira", '₺' },     // Turkish Lira
        { "rand", 'R' },     // South African Rand
        { "naira", '₦' },    // Nigerian Naira
        { "taka", '৳' },     // Bangladeshi Taka
        // Unicode mathematical symbols
        { "plus", '+' },     // Plus sign
        { "minus", '-' },    // Minus sign
        { "multiply", '×' }, // Multiplication sign
        { "divide", '÷' },   // Division sign
        { "equals", '=' },   // Equals sign
        { "notEquals", '≠' },// Not equal sign
        { "lessThan", '<' }, // Less-than sign (alias for lt)
        { "greaterThan", '>' }, // Greater-than sign (alias for gt)
        { "lessThanOrEqualTo", '≤' }, // Less-than or equal to
        { "greaterThanOrEqualTo", '≥' }, // Greater-than or equal to
        { "pi", 'π' },       // Pi symbol
        { "infinity", '∞' }, // Infinity symbol
        { "sqrt", '√' },     // Square root
        { "integral", '∫' }, // Integral sign
        { "summation", '∑' },// Summation sign
        { "product", '∏' },  // Product sign
        { "angle", '∠' },    // Angle symbol
        { "degree", '°' },   // Degree symbol
        // Miscellaneous Unicode symbols
        { "arrowleft", '←' },        // Leftwards arrow
        { "arrowright", '→' },       // Rightwards arrow
        { "arrowup", '↑' },          // Upwards arrow
        { "arrowdown", '↓' },        // Downwards arrow
        { "checkmark", '✓' },        // Check mark
        { "crossmark", '✗' },        // Cross mark
        { "snowman", '☃' },          // Snowman
        { "musicnote", '♪' },        // Eighth note
        { "spade", '♠' },            // Spade suit
        { "club", '♣' },             // Club suit
        { "heartcard", '♥' },        // Heart suit (card)
        { "diamond", '♦' },          // Diamond suit
        { "smiley", '☺' },           // White smiling face
        { "sun", '☀' },              // Black sun with rays
        { "umbrella", '☂' },         // Umbrella
        { "phone", '☎' },            // Telephone
        { "peace", '☮' },            // Peace symbol
        { "yinYang", '☯' },          // Yin Yang
        { "anchor", '⚓' },           // Anchor
        { "scissors", '✂' },         // Scissors
        { "hourglassdone", '⌛' },    // Hourglass done
        { "percent", '%' },  // Percent sign
        // Greek letters
        { "alpha", 'α' },    // Greek alpha
        { "beta", 'β' },     // Greek beta
        { "gamma", 'γ' },    // Greek gamma
        { "delta", 'δ' },    // Greek delta
        { "epsilon", 'ε' },  // Greek epsilon
        { "zeta", 'ζ' },     // Greek zeta
        { "eta", 'η' },      // Greek eta
        { "theta", 'θ' },    // Greek theta
        { "iota", 'ι' },     // Greek iota
        { "kappa", 'κ' },    // Greek kappa
        { "lambda", 'λ' },   // Greek lambda
        { "mu", 'μ' },       // Greek mu
        { "nu", 'ν' },       // Greek nu
        { "xi", 'ξ' },       // Greek xi
        { "omicron", 'ο' },  // Greek omicron
        { "rho", 'ρ' },      // Greek rho
        { "sigma", 'σ' },    // Greek sigma
        { "tau", 'τ' },      // Greek tau
        { "upsilon", 'υ' },  // Greek upsilon
        { "phi", 'φ' },      // Greek phi
        { "chi", 'χ' },      // Greek chi
        { "psi", 'ψ' },      // Greek psi
        { "omega", 'ω' },    // Greek omega
        // Dashes and bullets
        { "emdash", '—' },   // Em dash
        { "endash", '–' },   // En dash
        { "bullet", '•' },   // Bullet point
        { "ellipsis", '…' }, // Ellipsis
        { "section", '§' },      // Section sign
        { "paragraph", '¶' },    // Pilcrow/paragraph sign
        { "micro", 'µ' },        // Micro sign
        { "copyright", '©' },    // Copyright sign
        { "registered", '®' },   // Registered trademark
        { "trademark", '™' },    // Trademark
        { "currency", '¤' },     // Generic currency sign
        { "brokenbar", '¦' },    // Broken bar
        { "dagger", '†' },       // Dagger
        { "ddagger", '‡' },      // Double dagger
        { "perthousand", '‰' },  // Per mille sign
        { "prime", '′' },        // Prime (minutes/feet)
        { "doubleprime", '″' },  // Double prime (seconds/inches)
        { "middot", '·' },       // Middle dot
        { "lozenge", '◊' },      // Lozenge
        { "caret", '‸' },        // Inverted caret
        { "invertedexclamation", '¡' }, // Inverted exclamation mark
        { "invertedquestion", '¿' },    // Inverted question mark
        { "leftguillemet", '«' },   // Left guillemet
        { "rightguillemet", '»' },  // Right guillemet
        { "ohm", 'Ω' },             // Ohm sign
        { "angstrom", 'Å' },        // Angstrom sign
        { "numero", '№' },          // Numero sign
        { "partial", '∂' },         // Partial differential
        { "nabla", '∇' },           // Nabla (del)
        { "forall", '∀' },          // For all
        { "exists", '∃' },          // There exists
        { "emptyset", '∅' },        // Empty set
        { "elementof", '∈' },       // Element of
        { "notelementof", '∉' },    // Not element of
        { "intersection", '∩' },    // Intersection
        { "union", '∪' },           // Union
        { "logicaland", '∧' },      // Logical and
        { "logicalor", '∨' },       // Logical or
        { "therefore", '∴' },       // Therefore
        { "because", '∵' },         // Because
        { "nbsp", 0x00A0 },  // Non-breaking space
        // Unicode icon identifiers
        // Common emoji
        { "smile", 0x1F604 },       // 😄 Smiling face with open mouth and smiling eyes
        { "grin", 0x1F601 },        // 😁 Grinning face with smiling eyes
        { "joy", 0x1F602 },         // 😂 Face with tears of joy
        { "laugh", 0x1F606 },       // 😆 Smiling face with open mouth and tightly-closed eyes
        { "sad", 0x1F622 },         // 😢 Crying face
        { "angry", 0x1F620 },       // 😠 Angry face
        { "surprised", 0x1F62E },   // 😮 Face with open mouth
        { "confused", 0x1F615 },    // 😕 Confused face
        { "cool", 0x1F60E },        // 😎 Smiling face with sunglasses
        { "wink", 0x1F609 },        // 😉 Winking face
        { "heart", 0x2764 },        // ❤ Red heart
        { "thumbsup", 0x1F44D },    // 👍 Thumbs up
        { "thumbsdown", 0x1F44E },  // 👎 Thumbs down
        { "clap", 0x1F44F },        // 👏 Clapping hands
        { "fire", 0x1F525 },        // 🔥 Fire
        { "star", 0x2B50 },         // ⭐ Star
        { "rocket", 0x1F680 },      // 🚀 Rocket
        { "tada", 0x1F389 },        // 🎉 Party popper
        { "eyes", 0x1F440 },        // 👀 Eyes
        { "thinking", 0x1F914 },    // 🤔 Thinking face
        { "shrug", 0x1F937 },       // 🤷 Person shrugging
        { "wave", 0x1F44B },        // 👋 Waving hand
        { "ok", 0x1F197 },          // 🆗 OK button
        // CI/build/task emoji
        { "lightbulb", 0x1F4A1 },   // 💡 Light bulb
        { "warning", 0x26A0 },      // ⚠ Warning sign
        { "check", 0x2705 },        // ✅ Check mark button
        { "cross", 0x274C },        // ❌ Cross mark
        { "hourglass", 0x23F3 },    // ⏳ Hourglass
        { "hammer", 0x1F528 },      // 🔨 Hammer
        { "package", 0x1F4E6 },     // 📦 Package
        { "construction", 0x1F6A7 },// 🚧 Construction
        { "bug", 0x1F41B },         // 🐛 Bug
        { "recycle", 0x267B },      // ♻ Recycling symbol
        { "gear", 0x2699 },         // ⚙ Gear
        { "wrench", 0x1F527 },      // 🔧 Wrench
        { "memo", 0x1F4DD },        // 📝 Memo
        { "lock", 0x1F512 },        // 🔒 Lock
        { "unlock", 0x1F513 },      // 🔓 Unlock
        { "pushpin", 0x1F4CC },     // 📌 Pushpin
        { "mag", 0x1F50D },         // 🔍 Magnifying glass
        { "repeat", 0x1F501 },      // 🔁 Repeat
        { "repeatone", 0x1F502 },   // 🔂 Repeat single
        { "stop", 0x23F9 },         // ⏹ Stop button
        { "play", 0x25B6 },         // ▶ Play button
        { "pause", 0x23F8 },        // ⏸ Pause button
        { "fastforward", 0x23E9 },  // ⏩ Fast-forward
        { "rewind", 0x23EA },       // ⏪ Rewind
        { "vstext", 0xFE0E },       // Variation selector for text
        { "vsemoji", 0xFE0F }       // Variation selector for emoji
    };

    private static Dictionary<string, int> _custom = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets the custom character ID mappings for the registry.
    /// This replaces any existing custom mappings with the provided dictionary.
    /// </summary>
    /// <param name="custom">A dictionary mapping custom character IDs to their Unicode code points.</param>
    public static void SetCustomIds(Dictionary<string, int> custom) {
        _custom = new Dictionary<string, int>(custom, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves a character ID to its corresponding Unicode code point.
    /// Searches first in custom mappings, then in built-in mappings.
    /// </summary>
    /// <param name="id">The character ID to resolve (case-insensitive).</param>
    /// <returns>The Unicode code point if found; otherwise, null.</returns>
    public static int? Resolve(string id) {
        if (_custom.TryGetValue(id, out int value)) {
            return value;
        }

        if (_builtin.TryGetValue(id, out value)) {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets a read-only view of the built-in character ID mappings.
    /// Contains standard ASCII control characters and common symbols.
    /// </summary>
    public static IReadOnlyDictionary<string, int> Builtin => _builtin;

    /// <summary>
    /// Gets a read-only view of the custom character ID mappings.
    /// Contains user-defined or document-specific character definitions.
    /// </summary>
    public static IReadOnlyDictionary<string, int> Custom => _custom;
}
