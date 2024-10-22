using System.Globalization;
using ParksComputing.Xfer.Models;

namespace ParksComputing.Xfer.Services;

internal class Tokenizer {
    public Tokenizer() { }

    private string _scanString = string.Empty;

    private int Start { get; set; } = 0;

    private int Position { get; set; } = 0;

    private string ScanString {
        get {
            return _scanString;
        }
        set {
            Start = Position = 0;
            _scanString = value;
        }
    }

    private char CurrentChar {
        get {
            if (Position >= ScanString.Length) { return '\0'; }
            return ScanString[Position];
        }
    }

    private string CurrentString {
        get {
            if (Start >= ScanString.Length || Position >= ScanString.Length) { return string.Empty; }
            return ScanString.Substring(Start, Position - (Start - 1));
        }
    }

    private char Peek {
        get {
            if (Position + 1 >= ScanString.Length) { return '\0'; }
            return ScanString[Position + 1];
        }
    }

    private string Remaining {
        get {
            if (Position >= ScanString.Length) { return string.Empty; }
            return ScanString[Position..];
        }
    }

    private char Advance() {
        ++Position;
        Start = Position;
        return CurrentChar;
    }

    private string Expand() {
        ++Position;
        return CurrentString;
    }

    private bool IsIdentifierChar(char c) {
        return char.IsLetterOrDigit(c) | c == '_' | c == '-';
    }

    private readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>() {
        {"encoding", TokenType.EncodingKey},
        {"version", TokenType.VersionKey},
        {"urn", TokenType.UrnKey}
    };

    private bool IsKeyword(string compare, out TokenType tokenType) {
        return Keywords.TryGetValue(compare.ToLower(), out tokenType);
    }


    internal IEnumerable<Token> Tokenize(IEnumerable<string> inputs) {
        var tokenList = new List<Token>();

        foreach (var input in inputs) {
            tokenList.AddRange(Tokenize(input));
        }

        return tokenList;
    }

    internal IEnumerable<Token> Tokenize(string input) {
        if (string.IsNullOrEmpty(input)) {
            throw new ArgumentNullException(nameof(input));
        }

        var tokenList = new List<Token>();
        ScanString = input;

        return TokenizeDocument(tokenList);
    }

    internal IEnumerable<Token> TokenizeDocument(List<Token> tokenList) {
        return TokenizeMetadata(tokenList);
    }

    internal IEnumerable<Token> TokenizeMetadata(List<Token> tokenList) {
        return TokenizeElements(tokenList);
    }

    internal IEnumerable<Token> TokenizeKeyValuePairs(List<Token> tokenList) {
        return TokenizeContent(tokenList); 
    }

    internal IEnumerable<Token> TokenizeElements(List<Token> tokenList) {

        while (CurrentChar != '\0') {
            switch (CurrentChar) {
                case '<':
                    Advance();

                    switch (CurrentChar) {
                        case '!': {
                                tokenList.Add(new Token(CurrentString, TokenType.CommentOpen));
                                Advance();

                                while (CurrentChar != '\0') {
                                    if (CurrentChar == '!' && Peek == '>') {
                                        tokenList.Add(new Token(CurrentString, TokenType.CommentClose));
                                        Advance();
                                        break;
                                    }

                                    Expand();
                                }
                            }
                            break;

                        case '@': {
                                TokenizeMetadata(tokenList);
                            }
                            break;
                        case '(':
                            tokenList.Add(new Token(CurrentString, TokenType.PropertyBagOpen));
                            break;
                        case '{':
                            tokenList.Add(new Token(CurrentString, TokenType.ObjectOpen));
                            break;
                        case '[':
                            tokenList.Add(new Token(CurrentString, TokenType.ArrayOpen));
                            break;
                        case '"': {
                                tokenList.Add(new Token(CurrentString, TokenType.StringOpen));

                                Advance();

                                while (CurrentChar != '\0') {
                                    if (CurrentChar == '"' && Peek != '>') {
                                        tokenList.Add(new Token(CurrentString, TokenType.StringClose));
                                        var str = CurrentString;
                                        Advance();
                                        break;
                                    }

                                    Expand();
                                }
                            }
                            break;
                        default:
                            tokenList.Add(new Token(CurrentString, TokenType.Unknown));
                            break;
                    }
                    break;

                case '>':
                    Advance();

                    switch (CurrentChar) {
                        case '!':
                            tokenList.Add(new Token(CurrentString, TokenType.CommentClose));
                            break;
                        case '@':
                            tokenList.Add(new Token(CurrentString, TokenType.MetadataClose));
                            break;
                        case ')':
                            tokenList.Add(new Token(CurrentString, TokenType.PropertyBagClose));
                            break;
                        case '}':
                            tokenList.Add(new Token(CurrentString, TokenType.ObjectClose));
                            break;
                        case ']':
                            tokenList.Add(new Token(CurrentString, TokenType.ArrayClose));
                            break;
                        case '"':
                            tokenList.Add(new Token(CurrentString, TokenType.StringClose));
                            break;
                        default:
                            tokenList.Add(new Token(CurrentString, TokenType.Unknown));
                            break;
                    }
                    break;

                default:
                    break;
            }

            Advance();
        }

        return tokenList;
    }

    private List<Token> TokenizeContent(List<Token> tokenList) {
        if (char.IsDigit(CurrentChar) || CurrentChar.ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) {
            while (char.IsDigit(Peek) || Peek.ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) {
                Expand();
            }

            if (double.TryParse(CurrentString, out double doubleValue)) {
                tokenList.Add(new Token(CurrentString, TokenType.Number, doubleValue));
            }
            else if (int.TryParse(CurrentString, out int intValue)) {
                tokenList.Add(new Token(CurrentString, TokenType.Integer, intValue));
            }
        }
        else if (IsIdentifierChar(CurrentChar)) {
            while (IsIdentifierChar(Peek)) {
                Expand();
            }

            TokenType tokenType;

            if (!IsKeyword(CurrentString, out tokenType)) {
                tokenType = TokenType.Identifier;
            }

            tokenList.Add(new Token(CurrentString, tokenType, CurrentString));
        }

        return tokenList;
    }

    private void TokenizeMetadata2(List<Token> tokenList) {
        tokenList.Add(new Token(CurrentString, TokenType.MetadataOpen));
        Advance();

        while (CurrentChar != '\0') {
            TokenizeContent(tokenList);

            if (CurrentChar == '@' && Peek == '>') {
                tokenList.Add(new Token(CurrentString, TokenType.MetadataClose));
                Advance();
                break;
            }

            Advance();
        }
    }
}
