using System.Windows.Media;

namespace VaultViewer;

public enum AppTheme
{
    Dark,
    Light
}

/// <summary>
/// The colour value for each named brush resource, per theme. Pure data so the
/// mapping can be unit tested; <c>ThemeService</c> applies it to the live brushes.
/// The dark values match the defaults defined in App.xaml so toggling back is lossless.
/// </summary>
public static class ThemePalettes
{
    // Brush resource keys shared by both themes.
    public static readonly IReadOnlyList<string> Keys = new[]
    {
        "BgBrush", "PanelBrush", "PanelAltBrush", "BorderBrush",
        "AccentBrush", "AccentHoverBrush", "AccentSoftBrush", "Accent2Brush", "TextBrush", "MutedBrush"
    };

    private static readonly IReadOnlyDictionary<string, Color> Dark = new Dictionary<string, Color>
    {
        ["BgBrush"] = Rgb(0x14, 0x14, 0x13),
        ["PanelBrush"] = Rgb(0x17, 0x16, 0x15),
        ["PanelAltBrush"] = Rgb(0x2A, 0x28, 0x25),
        ["BorderBrush"] = Rgb(0x38, 0x35, 0x2F),
        ["AccentBrush"] = Rgb(0xFF, 0x5A, 0x2C),
        ["AccentHoverBrush"] = Rgb(0xFF, 0x71, 0x45),
        ["AccentSoftBrush"] = Argb(0x33, 0xFF, 0x5A, 0x2C),
        ["Accent2Brush"] = Rgb(0x4F, 0xD6, 0xE0),
        ["TextBrush"] = Rgb(0xED, 0xEB, 0xE4),
        ["MutedBrush"] = Rgb(0xA2, 0x9D, 0x93),
    };

    private static readonly IReadOnlyDictionary<string, Color> Light = new Dictionary<string, Color>
    {
        ["BgBrush"] = Rgb(0xF4, 0xF5, 0xF7),
        ["PanelBrush"] = Rgb(0xFF, 0xFF, 0xFF),
        ["PanelAltBrush"] = Rgb(0xED, 0xEF, 0xF3),
        ["BorderBrush"] = Rgb(0xD6, 0xDA, 0xE1),
        ["AccentBrush"] = Rgb(0xF5, 0x9E, 0x0B),
        ["AccentHoverBrush"] = Rgb(0xD9, 0x77, 0x06),
        ["AccentSoftBrush"] = Argb(0x22, 0xF5, 0x9E, 0x0B),
        ["Accent2Brush"] = Rgb(0x10, 0xB9, 0x81),
        ["TextBrush"] = Rgb(0x1B, 0x23, 0x30),
        ["MutedBrush"] = Rgb(0x6B, 0x72, 0x80),
    };

    public static IReadOnlyDictionary<string, Color> For(AppTheme theme) =>
        theme == AppTheme.Light ? Light : Dark;

    private static Color Rgb(byte r, byte g, byte b) => Color.FromArgb(0xFF, r, g, b);

    private static Color Argb(byte a, byte r, byte g, byte b) => Color.FromArgb(a, r, g, b);
}
