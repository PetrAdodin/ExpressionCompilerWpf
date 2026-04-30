namespace ExpressionCompilerWpf.Models;

public sealed record Token(TokenType Type, string Lexeme, int Position)
{
    public override string ToString() => $"{Type}: '{Lexeme}' @ {Position}";
}