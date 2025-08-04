using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Extensions;

/// <summary>
/// Provides extension methods for character operations specific to XferLang parsing and validation.
/// Contains utility methods for identifying different types of characters used in XferLang syntax.
/// </summary>
public static class CharExtensions {
    /// <summary>
    /// Determines whether a character is valid for use in XferLang keywords.
    /// Valid keyword characters include letters, digits, underscore, hyphen, and period.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character is valid for keywords; otherwise, false.</returns>
    public static bool IsKeywordChar(this char c) {
        return char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.';
    }

    /// <summary>
    /// Determines whether a character can be the first character of an integer literal.
    /// Includes digits, hexadecimal prefix, binary prefix, and sign characters.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character can start an integer literal; otherwise, false.</returns>
    public static bool IsIntegerLeadingChar(this char c) {
        return char.IsNumber(c) || c == Element.HexadecimalPrefix || c == Element.BinaryPrefix || c == '+' || c == '-';
    }

    /// <summary>
    /// Determines whether a character can serve as an opening delimiter for XferLang elements.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character can be an opening delimiter; otherwise, false.</returns>
    public static bool IsElementOpeningCharacter(this char c) {
        return c == Element.ElementOpeningCharacter;
    }

    /// <summary>
    /// Determines whether a character can serve as a closing delimiter for XferLang elements.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character can be a closing delimiter; otherwise, false.</returns>
    public static bool IsElementClosingCharacter(this char c) {
        return c == Element.ElementClosingCharacter;
    }

    /// <summary>
    /// Determines whether a character is a valid opening specifier for any XferLang element type.
    /// Checks against all known element opening specifiers including identifiers, numbers, booleans, etc.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character is a valid opening specifier; otherwise, false.</returns>
    public static bool IsElementOpeningSpecifier(this char c) {
        return
               c == IdentifierElement.OpeningSpecifier
            || c == IntegerElement.OpeningSpecifier
            || c == LongElement.OpeningSpecifier
            || c == DecimalElement.OpeningSpecifier
            || c == DoubleElement.OpeningSpecifier
            || c == BooleanElement.OpeningSpecifier
            || c == DateTimeElement.OpeningSpecifier
            || c == CharacterElement.OpeningSpecifier
            || c == InterpolatedElement.OpeningSpecifier
            || c == DynamicElement.OpeningSpecifier
            || c == NullElement.OpeningSpecifier
            || c == CommentElement.OpeningSpecifier
            || c == StringElement.OpeningSpecifier
            || c == ArrayElement.OpeningSpecifier
            || c == ObjectElement.OpeningSpecifier
            || c == TupleElement.OpeningSpecifier
            ;
    }

    /// <summary>
    /// Determines whether a character is a valid closing specifier for any XferLang element type.
    /// Checks against all known element closing specifiers including identifiers, numbers, booleans, etc.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character is a valid closing specifier; otherwise, false.</returns>
    public static bool IsElementClosingSpecifier(this char c) {
        return
               c == IdentifierElement.ClosingSpecifier
            || c == IntegerElement.ClosingSpecifier
            || c == LongElement.ClosingSpecifier
            || c == DecimalElement.ClosingSpecifier
            || c == DoubleElement.ClosingSpecifier
            || c == BooleanElement.ClosingSpecifier
            || c == DateTimeElement.ClosingSpecifier
            || c == CharacterElement.ClosingSpecifier
            || c == InterpolatedElement.ClosingSpecifier
            || c == DynamicElement.ClosingSpecifier
            || c == NullElement.ClosingSpecifier
            || c == CommentElement.ClosingSpecifier
            || c == StringElement.ClosingSpecifier
            || c == ArrayElement.ClosingSpecifier
            || c == ObjectElement.ClosingSpecifier
            || c == TupleElement.ClosingSpecifier
            ;
    }

    /// <summary>
    /// Determines whether a character is a valid opening specifier for collection elements.
    /// Includes objects, arrays, and tuples.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character opens a collection element; otherwise, false.</returns>
    public static bool IsCollectionOpeningSpecifier(this char c) {
        return
               c == ObjectElement.OpeningSpecifier
            || c == ArrayElement.OpeningSpecifier
            || c == TupleElement.OpeningSpecifier
            ;
    }

    /// <summary>
    /// Determines whether a character is a valid closing specifier for collection elements.
    /// Includes objects, arrays, and tuples.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character closes a collection element; otherwise, false.</returns>
    public static bool IsCollectionClosingSpecifier(this char c) {
        return
               c == ObjectElement.ClosingSpecifier
            || c == ArrayElement.ClosingSpecifier
            || c == TupleElement.ClosingSpecifier
            ;
    }

    /// <summary>
    /// Determines whether a character is considered whitespace.
    /// Uses the standard .NET char.IsWhiteSpace method.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character is whitespace; otherwise, false.</returns>
    public static bool IsWhiteSpace(this char c) {
        return char.IsWhiteSpace(c);
    }
}
