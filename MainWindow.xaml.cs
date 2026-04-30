using System.Windows;
using ExpressionCompilerWpf.ViewModels;

namespace ExpressionCompilerWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}