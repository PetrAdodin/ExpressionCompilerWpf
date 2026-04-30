namespace ExpressionCompilerWpf.Models;

public sealed class LexerResult
{
    public LexerResult(IReadOnlyList<Token> tokens, IReadOnlyList<Diagnostic> diagnostics)
    {
        Tokens = tokens;
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<Token> Tokens { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}