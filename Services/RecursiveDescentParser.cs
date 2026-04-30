using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class RecursiveDescentParser
{
    private IReadOnlyList<Token> _tokens = Array.Empty<Token>();
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly List<Quadruple> _quadruples = new();
    private int _position;
    private int _tempCounter;

    public ParseResult Parse(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
        _diagnostics.Clear();
        _quadruples.Clear();
        _position = 0;
        _tempCounter = 0;

        var root = ParseE();

        if (Current.Type != TokenType.End)
        {
            if (Current.Type == TokenType.RightParen)
            {
                AddError("Лишняя закрывающая скобка.", Current.Position);
            }
            else
            {
                AddError($"Лишний токен '{Current.Lexeme}' после конца выражения.", Current.Position);
            }
        }

        return new ParseResult(_diagnostics.Count == 0, root, _quadruples.ToList(), _diagnostics.ToList());
    }

    // E → T A. Здесь я возвращаю не узел дерева, а "место" результата: число, id или временную переменную.
    private string ParseE()
    {
        var left = ParseT();

        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current.Lexeme;
            Advance();
            var right = ParseT();
            left = Emit(op, left, right);
        }

        return left;
    }

    // T → F B. Цикл удобнее рекурсивной функции B и сохраняет левую ассоциативность операций.
    private string ParseT()
    {
        var left = ParseF();

        while (Current.Type is TokenType.Multiply or TokenType.Divide or TokenType.Modulo)
        {
            var op = Current.Lexeme;
            Advance();
            var right = ParseF();
            left = Emit(op, left, right);
        }

        return left;
    }

    private string ParseF()
    {
        if (Current.Type is TokenType.Number or TokenType.Identifier)
        {
            var lexeme = Current.Lexeme;
            Advance();
            return lexeme;
        }

        if (Current.Type == TokenType.LeftParen)
        {
            var openPosition = Current.Position;
            Advance();
            var expression = ParseE();

            if (Current.Type != TokenType.RightParen)
            {
                AddError("Пропущена закрывающая скобка.", openPosition);
                return expression;
            }

            Advance();
            return expression;
        }

        if (Current.Type == TokenType.RightParen)
        {
            AddError("Пропущен операнд перед закрывающей скобкой.", Current.Position);
            return ErrorPlace();
        }

        if (Current.Type == TokenType.End)
        {
            AddError("Пропущен операнд в конце выражения.", Current.Position);
            return ErrorPlace();
        }

        AddError($"Пропущен операнд перед '{Current.Lexeme}'.", Current.Position);
        Advance();
        return ErrorPlace();
    }

    private string Emit(string op, string arg1, string arg2)
    {
        var temp = $"t{++_tempCounter}";
        _quadruples.Add(new Quadruple(_quadruples.Count + 1, op, arg1, arg2, temp));
        return temp;
    }

    private Token Current => _position < _tokens.Count ? _tokens[_position] : _tokens[^1];

    private void Advance()
    {
        if (_position < _tokens.Count - 1)
            _position++;
    }

    private void AddError(string message, int position) =>
        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, position));

    private static string ErrorPlace() => "<error>";
}