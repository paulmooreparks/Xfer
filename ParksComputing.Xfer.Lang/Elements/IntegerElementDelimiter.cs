using System;

namespace ParksComputing.Xfer.Lang.Elements;

public class EmptyClosingElementDelimiter : ElementDelimiter {
    /// <summary>
    /// The opening delimiter character for integer elements.
    /// </summary>
    public override string CompactOpening { get => base.CompactOpening; protected set => base.CompactOpening = value; }

    /// <summary>
    /// The closing delimiter character for integer elements.
    /// </summary>
    public override string CompactClosing { get => string.Empty; protected set => base.CompactClosing = value; }

    /// <summary>
    /// Initializes a new instance of the IntegerElementDelimiter class.
    /// </summary>
    public EmptyClosingElementDelimiter(char openingSpecifier, char closingSpecifier, ElementStyle style = ElementStyle.Compact) : base(openingSpecifier, closingSpecifier, 1, style) {
        // No additional initialization needed
    }

    public EmptyClosingElementDelimiter(char openingSpecifier, char closingSpecifier, int specifierCount, ElementStyle style = ElementStyle.Compact) : base(openingSpecifier, closingSpecifier, specifierCount, style) {
        // No additional initialization needed
    }
}
