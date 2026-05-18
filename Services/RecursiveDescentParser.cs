using ExpressionCompilerWpf.Models;

namespace ExpressionCompilerWpf.Services;

public sealed class RecursiveDescentParser
{
    private const string ErrorValue = "<error>";
    private const int MaxDiagnostics = 50;

    private IReadOnlyList<Token> _tokens = Array.Empty<Token>();
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly List<Quadruple> _quadruples = new();
    private readonly HashSet<int> _reportedPositions = new();

    private int _position;
    private int _tempCounter;
    private int _parenDepth;

    public ParseResult Parse(IReadOnlyList<Token>? tokens)
    {
        _tokens = tokens is { Count: > 0 }
            ? tokens
            : new[] { new Token(TokenType.End, string.Empty, 0) };

        _diagnostics.Clear();
        _quadruples.Clear();
        _reportedPositions.Clear();

        _position = 0;
        _tempCounter = 0;
        _parenDepth = 0;

        if (Current.Type == TokenType.End)
        {
            AddError("Пустое выражение.", Current.Position);
            return new ParseResult(false, ErrorValue, Array.Empty<Quadruple>(), _diagnostics.ToList());
        }

        var root = ParseE();

        if (Current.Type != TokenType.End)
        {
            if (Current.Type == TokenType.RightParen)
            {
                ReportRemainingExtraClosingParentheses();
            }
            else if (!HasErrors)
            {
                AddError($"Лишний токен '{Current.Lexeme}' после конца выражения.", Current.Position);
            }
        }

        return new ParseResult(!HasErrors, root, _quadruples.ToList(), _diagnostics.ToList());
    }

    // E → T A
    private string ParseE()
    {
        var left = ParseT();

        while (!HasErrors && Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current.Lexeme;
            Advance();

            var right = ParseT();
            left = Emit(op, left, right);
        }

        return left;
    }

    // T → F B
    private string ParseT()
    {
        var left = ParseF();

        while (!HasErrors && Current.Type is TokenType.Multiply or TokenType.Divide or TokenType.Modulo)
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

            _parenDepth++;
            Advance();

            if (Current.Type == TokenType.RightParen)
            {
                AddError("Пропущен операнд внутри скобок.", Current.Position);
                Advance();
                _parenDepth--;
                return ErrorValue;
            }

            if (Current.Type == TokenType.End)
            {
                AddError("Пропущен операнд после открывающей скобки.", openPosition);
                _parenDepth--;
                return ErrorValue;
            }

            var expression = ParseE();

            if (Current.Type != TokenType.RightParen)
            {
                AddError("Пропущена закрывающая скобка.", openPosition);
                _parenDepth--;
                return expression;
            }

            Advance();
            _parenDepth--;
            return expression;
        }

        if (Current.Type == TokenType.RightParen)
        {
            if (_parenDepth == 0)
            {
                AddError("Лишняя закрывающая скобка.", Current.Position);
            }
            else
            {
                AddError("Пропущен операнд перед закрывающей скобкой.", Current.Position);
            }

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

    private void ReportRemainingExtraClosingParentheses()
    {
        while (Current.Type == TokenType.RightParen)
        {
            AddError("Лишняя закрывающая скобка.", Current.Position);
            Advance();
        }

        if (Current.Type != TokenType.End && !HasErrorsAt(Current.Position))
        {
            AddError($"Лишний токен '{Current.Lexeme}' после конца выражения.", Current.Position);
        }
    }

    private string Emit(string op, string arg1, string arg2)
    {
        if (HasErrors || arg1 == ErrorValue || arg2 == ErrorValue)
            return ErrorValue;

        var temp = $"t{++_tempCounter}";
        _quadruples.Add(new Quadruple(_quadruples.Count + 1, op, arg1, arg2, temp));
        return temp;
    }

    private Token Current => _position < _tokens.Count
        ? _tokens[_position]
        : _tokens[^1];

    private bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    private void Advance()
    {
        if (_position < _tokens.Count - 1)
            _position++;
    }

    private void AddError(string message, int position)
    {
        if (_diagnostics.Count >= MaxDiagnostics)
            return;

        if (!_reportedPositions.Add(position))
            return;

        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, position));
    }

    private bool HasErrorsAt(int position) => _reportedPositions.Contains(position);
}