using System;

namespace ParksComputing.Xfer.Lang.Configuration
{
    /// <summary>
    /// Defines preferences for element serialization styles.
    /// </summary>
    public enum ElementStylePreference
    {
        /// <summary>
        /// Use explicit style for maximum safety and compatibility (default).
        /// Strings use angle brackets: &lt;"value"&gt;
        /// </summary>
        Explicit,

        /// <summary>
        /// Use compact style when safe, explicit when necessary.
        /// Strings use quotes: "value" (when safe)
        /// </summary>
        CompactWhenSafe,

        /// <summary>
        /// Use most compact form possible, including implicit syntax.
        /// Integers without # prefix, strings without quotes when possible.
        /// </summary>
        MinimalWhenSafe,

        /// <summary>
        /// Always use compact style, even if potentially unsafe.
        /// Use with caution - may produce unparseable output.
        /// </summary>
        ForceCompact
    }
}
