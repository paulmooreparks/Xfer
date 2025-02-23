using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Extensions;

public static class CharExtensions {
    // Extension method for checking if a character is a valid keyword character.
    public static bool IsKeywordChar(this char c) {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.';
    }

    public static bool IsIntegerLeadingChar(this char c) {
        return char.IsNumber(c) || c == Element.HexadecimalPrefix || c == Element.BinaryPrefix || c == '+' || c == '-';
    }

    public static bool IsElementOpeningCharacter(this char c) {
        return c == Element.ElementOpeningCharacter;
    }

    public static bool IsElementClosingCharacter(this char c) {
        return c == Element.ElementClosingCharacter;
    }

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
            || c == EvaluatedElement.OpeningSpecifier
            || c == PlaceholderElement.OpeningSpecifier
            || c == NullElement.OpeningSpecifier
            || c == CommentElement.OpeningSpecifier
            || c == StringElement.OpeningSpecifier
            || c == ArrayElement.OpeningSpecifier
            || c == ObjectElement.OpeningSpecifier
            || c == PropertyBagElement.OpeningSpecifier
            ;
    }

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
            || c == EvaluatedElement.ClosingSpecifier
            || c == PlaceholderElement.ClosingSpecifier
            || c == NullElement.ClosingSpecifier
            || c == CommentElement.ClosingSpecifier
            || c == StringElement.ClosingSpecifier
            || c == ArrayElement.ClosingSpecifier
            || c == ObjectElement.ClosingSpecifier
            || c == PropertyBagElement.ClosingSpecifier
            ;
    }

    public static bool IsCollectionOpeningSpecifier(this char c) {
        return  
               c == ObjectElement.OpeningSpecifier
            || c == ArrayElement.OpeningSpecifier
            || c == PropertyBagElement.OpeningSpecifier
            ;
    }

    public static bool IsCollectionClosingSpecifier(this char c) {
        return  
               c == ObjectElement.ClosingSpecifier
            || c == ArrayElement.ClosingSpecifier
            || c == PropertyBagElement.ClosingSpecifier
            ;
    }

    public static bool IsWhiteSpace(this char c) {
        return char.IsWhiteSpace(c);
    }
}
