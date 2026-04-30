namespace ExpressionCompilerWpf.Models;

public sealed record Quadruple(int Index, string Op, string Arg1, string Arg2, string Result)
{
    public override string ToString() => $"({Op}, {Arg1}, {Arg2}, {Result})";
}