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
        ["BgBrush"] = Rgb(0x16, 0x1A, 0x21),
        ["PanelBrush"] = Rgb(0x1E, 0x24, 0x2E),
        ["PanelAltBrush"] = Rgb(0x23, 0x2B, 0x37),
        ["BorderBrush"] = Rgb(0x31, 0x3A, 0x48),
        ["AccentBrush"] = Rgb(0x63, 0x66, 0xF1),
        ["AccentHoverBrush"] = Rgb(0x7C, 0x7F, 0xF5),
        ["TextBrush"] = Rgb(0xE6, 0xE9, 0xEF),
        ["MutedBrush"] = Rgb(0x97, 0xA0, 0xB0),
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
