using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models;

internal class Token {
    internal string Lexeme { get; set; }
    internal TokenType Type { get; set; }
    internal object? Literal { get; set; } = null;

    public Token(string lexeme, TokenType type) {
        Lexeme = lexeme;
        Type = type;
    }

    public Token(string lexeme, TokenType type, object literal) : this(lexeme, type) {
        Literal = literal;
    }

    public override string ToString() {
        return Lexeme;
    }
}
