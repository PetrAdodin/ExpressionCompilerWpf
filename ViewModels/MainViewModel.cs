using System.Collections.ObjectModel;
using System.Windows.Input;
using ExpressionCompilerWpf.Infrastructure;
using ExpressionCompilerWpf.Models;
using ExpressionCompilerWpf.Services;

namespace ExpressionCompilerWpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly Lexer _lexer = new();
    private readonly RecursiveDescentParser _parser = new();
    private readonly PolizService _polizService = new();

    private string _source = "a + 2 * (b - 3)";
    private string _status = "Введите выражение и нажмите «Анализировать».";
    private string _poliz = string.Empty;
    private string _calculation = string.Empty;

    public MainViewModel()
    {
        AnalyzeCommand = new RelayCommand(_ => Analyze());
        ClearCommand = new RelayCommand(_ => Clear());
    }

    public string Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Poliz
    {
        get => _poliz;
        set => SetProperty(ref _poliz, value);
    }

    public string Calculation
    {
        get => _calculation;
        set => SetProperty(ref _calculation, value);
    }

    public ObservableCollection<Token> Tokens { get; } = new();
    public ObservableCollection<Diagnostic> Diagnostics { get; } = new();
    public ObservableCollection<Quadruple> Quadruples { get; } = new();

    public ICommand AnalyzeCommand { get; }
    public ICommand ClearCommand { get; }

    private void Analyze()
    {
        Tokens.Clear();
        Diagnostics.Clear();
        Quadruples.Clear();

        Poliz = string.Empty;
        Calculation = string.Empty;

        var source = Source ?? string.Empty;

        var lexerResult = _lexer.Analyze(source);

        foreach (var token in lexerResult.Tokens.Where(t => t.Type != TokenType.End))
            Tokens.Add(token);

        foreach (var diagnostic in lexerResult.Diagnostics)
            Diagnostics.Add(diagnostic);

        /*
         * Раньше здесь был return при лексических ошибках.
         * Я убрал его специально: теперь приложение показывает и ошибки лексера,
         * и ошибки парсера за один запуск анализа.
         *
         * Например, для строки:
         * 1+!((3/2)!
         *
         * лексер найдёт два символа '!',
         * а парсер дополнительно найдёт незакрытую скобку.
         */
        var parseResult = _parser.Parse(lexerResult.Tokens);

        foreach (var diagnostic in parseResult.Diagnostics)
            Diagnostics.Add(diagnostic);

        var hasLexerErrors = lexerResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        var hasParserErrors = parseResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

        if (hasLexerErrors || hasParserErrors)
        {
            Status = BuildErrorStatus(hasLexerErrors, hasParserErrors);
            return;
        }

        foreach (var quadruple in parseResult.Quadruples)
            Quadruples.Add(quadruple);

        var polizResult = _polizService.BuildAndEvaluate(lexerResult.Tokens);

        foreach (var diagnostic in polizResult.Diagnostics)
            Diagnostics.Add(diagnostic);

        Poliz = polizResult.AsText;
        Calculation = polizResult.Value?.ToString() ?? string.Empty;

        if (polizResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            Status = "Синтаксический разбор корректен, тетрады построены, но вычисление ПОЛИЗ завершилось ошибкой.";
            return;
        }

        if (polizResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning))
        {
            Status = "Синтаксический разбор корректен, тетрады построены. ПОЛИЗ не вычислялся из-за ограничений задания.";
            return;
        }

        Status = "Разбор корректен: тетрады, ПОЛИЗ и значение построены.";
    }

    private static string BuildErrorStatus(bool hasLexerErrors, bool hasParserErrors)
    {
        if (hasLexerErrors && hasParserErrors)
        {
            return "Есть лексические и синтаксические ошибки. Тетрады и ПОЛИЗ не строятся.";
        }

        if (hasLexerErrors)
        {
            return "Есть лексические ошибки. Тетрады и ПОЛИЗ не строятся.";
        }

        return "Есть синтаксические ошибки. Тетрады и ПОЛИЗ не строятся.";
    }

    private void Clear()
    {
        Source = string.Empty;

        Tokens.Clear();
        Diagnostics.Clear();
        Quadruples.Clear();

        Poliz = string.Empty;
        Calculation = string.Empty;
        Status = "Очищено.";
    }
}