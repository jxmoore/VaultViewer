using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VaultViewer.ViewModels;

namespace VaultViewer;

public partial class ThemeSettingsWindow : Window
{
    public ThemeSettingsWindow(ThemeSettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += (_, _) => Close();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ColorSwatch_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not ColorEntryViewModel entry) return;

        var current = ParseHexToDrawingColor(entry.Hex);
        using var dlg = new System.Windows.Forms.ColorDialog
        {
            Color = current,
            FullOpen = true,
            AnyColor = true,
        };

        if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        var c = dlg.Color;
        entry.Hex = $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    private static System.Drawing.Color ParseHexToDrawingColor(string hex)
    {
        var s = hex.TrimStart('#');
        if (s.Length == 6) s = "FF" + s;
        if (s.Length == 8 && uint.TryParse(s, NumberStyles.HexNumber, null, out var argb))
            return System.Drawing.Color.FromArgb(
                (byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        return System.Drawing.Color.White;
    }
}
