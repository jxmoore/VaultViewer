using System.Windows;
using VaultViewer.Services;
using VaultViewer.ViewModels;

namespace VaultViewer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        Loaded += async (_, _) =>
        {
            await _vm.LoadAsync();
            // Restore saved opacity; colors/fonts are already applied from App.xaml.cs OnStartup.
            var saved = new ThemeSettingsService().Load();
            Opacity = Math.Clamp(saved.WindowOpacity, 0.3, 1.0);
        };
        StateChanged += (_, _) =>
            RootGrid.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
    }

    private void MinButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaxButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void SettingsItem_Click(object sender, RoutedEventArgs e) => SettingsButton.IsChecked = false;

    private void OpenThemeSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = false;
        var vm = new ThemeSettingsViewModel(new ThemeSettingsService(), _vm.Theme);
        var win = new ThemeSettingsWindow(vm);
        win.Owner = this;
        win.ShowDialog();
    }
}
