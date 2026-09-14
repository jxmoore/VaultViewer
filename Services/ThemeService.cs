using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VaultViewer.Models;

namespace VaultViewer.Services;

/// <summary>
/// Swaps the theme by replacing the shared brush resources. Controls reference these
/// brushes via DynamicResource, so replacing an entry repaints the whole UI live
/// (and works even though the brushes are frozen).
/// </summary>
public sealed class ThemeService : IThemeService
{
    public AppTheme Current { get; private set; } = AppTheme.Dark;

    public void Apply(AppTheme theme)
    {
        Current = theme;

        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        foreach (var (key, color) in ThemePalettes.For(theme))
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[key] = brush;
        }
    }

    public void ApplyCustomSettings(CustomThemeSettings settings)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        foreach (var (key, hex) in settings.Colors)
        {
            var color = ParseHex(hex);
            if (color is null) continue;
            var brush = new SolidColorBrush(color.Value);
            brush.Freeze();
            resources[key] = brush;
        }

        if (!string.IsNullOrWhiteSpace(settings.FontFamily))
            resources["AppFontFamily"] = new FontFamily(settings.FontFamily);

        if (settings.FontSize > 0)
            resources["AppFontSize"] = settings.FontSize;

        if (Application.Current?.MainWindow is { } win)
            win.Opacity = Math.Clamp(settings.WindowOpacity, 0.3, 1.0);
    }

    private static Color? ParseHex(string? hex)
    {
        var s = hex?.TrimStart('#') ?? "";
        if (s.Length == 6) s = "FF" + s;
        if (s.Length == 8 && uint.TryParse(s, NumberStyles.HexNumber, null, out var argb))
            return Color.FromArgb(
                (byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        return null;
    }
}
