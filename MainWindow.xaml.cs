using System.Windows;
using VaultViewer.ViewModels;

namespace VaultViewer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.LoadAsync();
    }

    private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }
}
