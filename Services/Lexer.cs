using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class Lexer
{
    public LexerResult Analyze(string source)
    {
        var tokens = new List<Token>();
        var diagnostics = new List<Diagnostic>();
        var position = 0;

        while (position < source.Length)
        {
            var current = source[position];

            if (char.IsWhiteSpace(current))
            {
                position++;
                continue;
            }

            if (char.IsLetter(current))
            {
                var start = position;
                position++;

                while (position < source.Length && IsIdentifierPart(source[position]))
                    position++;

                tokens.Add(new Token(TokenType.Identifier, source[start..position], start));
                continue;
            }

            if (char.IsDigit(current))
            {
                var start = position;
                position++;

                while (position < source.Length && char.IsDigit(source[position]))
                    position++;

                tokens.Add(new Token(TokenType.Number, source[start..position], start));
                continue;
            }

            switch (current)
            {
                case '+': tokens.Add(new Token(TokenType.Plus, "+", position)); break;
                case '-': tokens.Add(new Token(TokenType.Minus, "-", position)); break;
                case '*': tokens.Add(new Token(TokenType.Multiply, "*", position)); break;
                case '/': tokens.Add(new Token(TokenType.Divide, "/", position)); break;
                case '%': tokens.Add(new Token(TokenType.Modulo, "%", position)); break;
                case '(': tokens.Add(new Token(TokenType.LeftParen, "(", position)); break;
                case ')': tokens.Add(new Token(TokenType.RightParen, ")", position)); break;
                default:
                    diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        $"Неверный символ '{current}'. Разрешены буквы, цифры, _, операторы + - * / %, скобки.",
                        position));
                    break;
            }

            position++;
        }

        tokens.Add(new Token(TokenType.End, string.Empty, source.Length));
        return new LexerResult(tokens, diagnostics);
    }

    private static bool IsIdentifierPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
}