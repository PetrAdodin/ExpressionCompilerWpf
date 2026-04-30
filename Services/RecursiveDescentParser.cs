using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class RecursiveDescentParser
{
    private const string ErrorValue = "<error>";

    private IReadOnlyList<Token> _tokens = Array.Empty<Token>();
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly List<Quadruple> _quadruples = new();
    private readonly HashSet<int> _reportedPositions = new();

    private int _position;
    private int _tempCounter;

    public ParseResult Parse(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
        _diagnostics.Clear();
        _quadruples.Clear();
        _reportedPositions.Clear();

        _position = 0;
        _tempCounter = 0;

        var root = ParseE();
s
        if (!HasErrors && Current.Type != TokenType.End)
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

        return new ParseResult(!HasErrors, root, _quadruples.ToList(), _diagnostics.ToList());
    }

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

            if (Current.Type == TokenType.RightParen)
            {
                AddError("Пропущен операнд внутри скобок.", Current.Position);
                Advance();
                return ErrorValue;
            }

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
            return ErrorValue;
        }

        if (Current.Type == TokenType.End)
        {
            AddError("Пропущен операнд в конце выражения.", Current.Position);
            return ErrorValue;
        }

        if (Current.Type is TokenType.Plus
            or TokenType.Minus
            or TokenType.Multiply
            or TokenType.Divide
            or TokenType.Modulo)
        {
            AddError($"Пропущен операнд перед '{Current.Lexeme}'.", Current.Position);
            return ErrorValue;
        }

        AddError($"Неожиданный токен '{Current.Lexeme}'.", Current.Position);
        Advance();
        return ErrorValue;
    }

    private string Emit(string op, string arg1, string arg2)
    {
        if (arg1 == ErrorValue || arg2 == ErrorValue)
            return ErrorValue;

        var temp = $"t{++_tempCounter}";
        _quadruples.Add(new Quadruple(_quadruples.Count + 1, op, arg1, arg2, temp));
        return temp;
    }

    private Token Current => _position < _tokens.Count ? _tokens[_position] : _tokens[^1];

    private bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    private void Advance()
    {
        if (_position < _tokens.Count - 1)
            _position++;
    }

    private void AddError(string message, int position)
    {
        if (!_reportedPositions.Add(position))
            return;

        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, position));
    }
}