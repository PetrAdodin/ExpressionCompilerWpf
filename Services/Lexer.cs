using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class Lexer
{
    private const int MaxDiagnostics = 50;

    public LexerResult Analyze(string? source)
    {
        source ??= string.Empty;

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

            if (IsAsciiLetter(current))
            {
                var start = position;
                position++;

                while (position < source.Length && IsIdentifierPart(source[position]))
                    position++;

                tokens.Add(new Token(TokenType.Identifier, source[start..position], start));
                continue;
            }

            if (IsAsciiDigit(current))
            {
                var start = position;
                position++;

                while (position < source.Length && IsAsciiDigit(source[position]))
                    position++;

                tokens.Add(new Token(TokenType.Number, source[start..position], start));
                continue;
            }

            switch (current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+", position));
                    position++;
                    break;

                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-", position));
                    position++;
                    break;

                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*", position));
                    position++;
                    break;

                case '/':
                    tokens.Add(new Token(TokenType.Divide, "/", position));
                    position++;
                    break;

                case '%':
                    tokens.Add(new Token(TokenType.Modulo, "%", position));
                    position++;
                    break;

                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "(", position));
                    position++;
                    break;

                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")", position));
                    position++;
                    break;

                default:
                    var badLexeme = ReadInvalidLexeme(source, position);

                    AddDiagnostic(
                        diagnostics,
                        DiagnosticSeverity.Error,
                        $"Неверный символ '{badLexeme}'. Разрешены латинские буквы, цифры, _, операторы + - * / %, скобки.",
                        position);

                    position += badLexeme.Length;
                    break;
            }

            if (diagnostics.Count >= MaxDiagnostics)
            {
                AddDiagnostic(
                    diagnostics,
                    DiagnosticSeverity.Warning,
                    "Найдено слишком много ошибок. Дальнейшая лексическая диагностика остановлена.",
                    position);

                break;
            }
        }

        tokens.Add(new Token(TokenType.End, string.Empty, source.Length));
        return new LexerResult(tokens, diagnostics);
    }

    private static bool IsIdentifierPart(char ch) =>
        IsAsciiLetter(ch) || IsAsciiDigit(ch) || ch == '_';

    private static bool IsAsciiLetter(char ch) =>
        ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    private static bool IsAsciiDigit(char ch) =>
        ch is >= '0' and <= '9';

    private static string ReadInvalidLexeme(string source, int position)
    {
        if (position + 1 < source.Length &&
            char.IsHighSurrogate(source[position]) &&
            char.IsLowSurrogate(source[position + 1]))
        {
            return source.Substring(position, 2);
        }

        return source[position].ToString();
    }

    private static void AddDiagnostic(
        List<Diagnostic> diagnostics,
        DiagnosticSeverity severity,
        string message,
        int position)
    {
        var errorCount = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

        if (severity == DiagnosticSeverity.Error && errorCount >= MaxDiagnostics)
            return;

        diagnostics.Add(new Diagnostic(severity, message, position));
    }
}