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
        // When maximized, a WindowStyle=None window would spill under the taskbar/edges;
        // pad the content by the resize border so it sits within the work area.
        StateChanged += (_, _) =>
            RootGrid.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
    }

    private void MinButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaxButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    // A settings-popup item ran its command; close the popup.
    private void SettingsItem_Click(object sender, RoutedEventArgs e) => SettingsButton.IsChecked = false;
}
