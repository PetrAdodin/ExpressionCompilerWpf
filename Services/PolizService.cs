using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class PolizService
{
    public PolizResult BuildAndEvaluate(IReadOnlyList<Token> tokens)
    {
        var diagnostics = new List<Diagnostic>();

        if (tokens.Any(t => t.Type == TokenType.Identifier))
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Warning,
                "ПОЛИЗ и вычисление доступны только для выражений из целых чисел. Найден идентификатор.",
                tokens.First(t => t.Type == TokenType.Identifier).Position));
            return new PolizResult(false, Array.Empty<string>(), null, diagnostics);
        }

        var poliz = BuildPoliz(tokens, diagnostics);
        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            return new PolizResult(false, poliz, null, diagnostics);

        var value = Evaluate(poliz, diagnostics);
        return new PolizResult(
            !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            poliz,
            value,
            diagnostics);
    }

    private static IReadOnlyList<string> BuildPoliz(IReadOnlyList<Token> tokens, List<Diagnostic> diagnostics)
    {
        var output = new List<string>();
        var operators = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    output.Add(token.Lexeme);
                    break;

                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Modulo:
                    while (operators.Count > 0 &&
                           operators.Peek().Type != TokenType.LeftParen &&
                           Priority(operators.Peek()) >= Priority(token))
                    {
                        output.Add(operators.Pop().Lexeme);
                    }

                    operators.Push(token);
                    break;

                case TokenType.LeftParen:
                    operators.Push(token);
                    break;

                case TokenType.RightParen:
                    while (operators.Count > 0 && operators.Peek().Type != TokenType.LeftParen)
                        output.Add(operators.Pop().Lexeme);

                    if (operators.Count == 0)
                    {
                        diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Лишняя закрывающая скобка в ПОЛИЗ.", token.Position));
                        break;
                    }

                    operators.Pop();
                    break;

                case TokenType.End:
                    break;
            }
        }

        while (operators.Count > 0)
        {
            var op = operators.Pop();
            if (op.Type == TokenType.LeftParen)
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Лишняя открывающая скобка в ПОЛИЗ.", op.Position));
            else
                output.Add(op.Lexeme);
        }

        return output;
    }

    private static long? Evaluate(IReadOnlyList<string> poliz, List<Diagnostic> diagnostics)
    {
        var stack = new Stack<long>();

        foreach (var item in poliz)
        {
            if (long.TryParse(item, out var number))
            {
                stack.Push(number);
                continue;
            }

            if (stack.Count < 2)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Недостаточно операндов при вычислении ПОЛИЗ.", -1));
                return null;
            }

            var right = stack.Pop();
            var left = stack.Pop();

            if ((item == "/" || item == "%") && right == 0)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Деление на ноль при вычислении выражения.", -1));
                return null;
            }

            var result = item switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                "%" => left % right,
                _ => throw new InvalidOperationException($"Неизвестная операция '{item}'.")
            };

            stack.Push(result);
        }

        if (stack.Count != 1)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "После вычисления ПОЛИЗ в стеке осталось некорректное количество значений.", -1));
            return null;
        }

        return stack.Pop();
    }

    private static int Priority(Token token) => token.Type switch
    {
        TokenType.Plus or TokenType.Minus => 1,
        TokenType.Multiply or TokenType.Divide or TokenType.Modulo => 2,
        _ => 0
    };
}