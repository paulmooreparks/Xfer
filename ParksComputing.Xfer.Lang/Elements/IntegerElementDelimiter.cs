using System;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Delimiter variant whose compact closing sequence is empty (used by dereference &amp; numeric forms).
/// </summary>
public class EmptyClosingElementDelimiter : ElementDelimiter {
    /// <summary>
    /// The opening delimiter character for integer elements.
    /// </summary>
    public override string CompactOpening { get => base.CompactOpening; protected set => base.CompactOpening = value; }

    /// <summary>
    /// The closing delimiter character for integer elements.
    /// </summary>
    public override string CompactClosing { get => string.Empty; protected set => base.CompactClosing = value; }

    /// <summary>Create a delimiter with empty closing sequence and specifier count 1.</summary>
    /// <param name="openingSpecifier">Opening delimiter character.</param>
    /// <param name="closingSpecifier">Closing delimiter character (same as opening).</param>
    /// <param name="style">Delimiter style.</param>
    public EmptyClosingElementDelimiter(char openingSpecifier, char closingSpecifier, ElementStyle style = ElementStyle.Compact) : base(openingSpecifier, closingSpecifier, 1, style) {
        // No additional initialization needed
    }

    /// <summary>Create a delimiter with an explicit specifier count.</summary>
    /// <param name="openingSpecifier">Opening delimiter character.</param>
    /// <param name="closingSpecifier">Closing delimiter character (same as opening).</param>
    /// <param name="specifierCount">Number of specifier characters.</param>
    /// <param name="style">Delimiter style.</param>
    public EmptyClosingElementDelimiter(char openingSpecifier, char closingSpecifier, int specifierCount, ElementStyle style = ElementStyle.Compact) : base(openingSpecifier, closingSpecifier, specifierCount, style) {
        // No additional initialization needed
    }
}
