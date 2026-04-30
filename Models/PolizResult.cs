namespace ExpressionCompilerWpf.Models;

public sealed class PolizResult
{
    public PolizResult(bool isSuccess, IReadOnlyList<string> items, long? value, IReadOnlyList<Diagnostic> diagnostics)
    {
        IsSuccess = isSuccess;
        Items = items;
        Value = value;
        Diagnostics = diagnostics;
    }

    public bool IsSuccess { get; }
    public IReadOnlyList<string> Items { get; }
    public long? Value { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public string AsText => string.Join(" ", Items);
}