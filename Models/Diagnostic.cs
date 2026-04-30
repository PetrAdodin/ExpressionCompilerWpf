namespace ExpressionCompilerWpf.Models;

public sealed record Diagnostic(DiagnosticSeverity Severity, string Message, int Position)
{
    public override string ToString() => Position >= 0
        ? $"{Severity}: {Message} Позиция: {Position}"
        : $"{Severity}: {Message}";
}