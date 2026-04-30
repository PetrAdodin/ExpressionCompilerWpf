namespace ExpressionCompilerWpf.Models;

public sealed class ParseResult
{
    public ParseResult(bool isSuccess, string? rootPlace, IReadOnlyList<Quadruple> quadruples, IReadOnlyList<Diagnostic> diagnostics)
    {
        IsSuccess = isSuccess;
        RootPlace = rootPlace;
        Quadruples = quadruples;
        Diagnostics = diagnostics;
    }

    public bool IsSuccess { get; }
    public string? RootPlace { get; }
    public IReadOnlyList<Quadruple> Quadruples { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }
}