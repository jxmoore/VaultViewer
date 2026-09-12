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
        "AccentBrush", "AccentHoverBrush", "TextBrush", "MutedBrush"
    };

    private static readonly IReadOnlyDictionary<string, Color> Dark = new Dictionary<string, Color>
    {
        ["BgBrush"] = Rgb(0x14, 0x14, 0x13),
        ["PanelBrush"] = Rgb(0x17, 0x16, 0x15),
        ["PanelAltBrush"] = Rgb(0x2A, 0x28, 0x25),
        ["BorderBrush"] = Rgb(0x38, 0x35, 0x2F),
        ["AccentBrush"] = Rgb(0xD9, 0x77, 0x57),
        ["AccentHoverBrush"] = Rgb(0xE0, 0x8A, 0x6B),
        ["TextBrush"] = Rgb(0xED, 0xEB, 0xE4),
        ["MutedBrush"] = Rgb(0xA2, 0x9D, 0x93),
    };

    private static readonly IReadOnlyDictionary<string, Color> Light = new Dictionary<string, Color>
    {
        ["BgBrush"] = Rgb(0xF4, 0xF5, 0xF7),
        ["PanelBrush"] = Rgb(0xFF, 0xFF, 0xFF),
        ["PanelAltBrush"] = Rgb(0xED, 0xEF, 0xF3),
        ["BorderBrush"] = Rgb(0xD6, 0xDA, 0xE1),
        ["AccentBrush"] = Rgb(0x63, 0x66, 0xF1),
        ["AccentHoverBrush"] = Rgb(0x54, 0x57, 0xE5),
        ["TextBrush"] = Rgb(0x1B, 0x23, 0x30),
        ["MutedBrush"] = Rgb(0x6B, 0x72, 0x80),
    };

    public static IReadOnlyDictionary<string, Color> For(AppTheme theme) =>
        theme == AppTheme.Light ? Light : Dark;

    private static Color Rgb(byte r, byte g, byte b) => Color.FromArgb(0xFF, r, g, b);
}
