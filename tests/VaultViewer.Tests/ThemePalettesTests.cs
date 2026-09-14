using VaultViewer;
using Xunit;

namespace VaultViewer.Tests;

public class ThemePalettesTests
{
    [Fact]
    public void Both_themes_define_every_key()
    {
        foreach (var key in ThemePalettes.Keys)
        {
            Assert.True(ThemePalettes.For(AppTheme.Dark).ContainsKey(key), $"Dark missing {key}");
            Assert.True(ThemePalettes.For(AppTheme.Light).ContainsKey(key), $"Light missing {key}");
        }
    }

    [Fact]
    public void Dark_background_matches_the_app_default()
    {
        // App.xaml's BgColor is #FF141413; toggling back to dark must restore it exactly.
        var bg = ThemePalettes.For(AppTheme.Dark)["BgBrush"];
        Assert.Equal((0xFF, 0x14, 0x14, 0x13), (bg.A, bg.R, bg.G, bg.B));
    }

    [Fact]
    public void Light_background_is_lighter_than_dark_background()
    {
        var dark = ThemePalettes.For(AppTheme.Dark)["BgBrush"];
        var light = ThemePalettes.For(AppTheme.Light)["BgBrush"];
        Assert.True(light.R + light.G + light.B > dark.R + dark.G + dark.B);
    }

    [Fact]
    public void Accent_differs_per_theme_coral_in_dark_amber_in_light()
    {
        var dark = ThemePalettes.For(AppTheme.Dark)["AccentBrush"];
        var light = ThemePalettes.For(AppTheme.Light)["AccentBrush"];

        Assert.NotEqual(dark, light);
        // Dark accent is coral (red dominates). Light accent is amber (red dominates, blue near-zero).
        Assert.True(dark.R > dark.B);
        Assert.True(light.R > light.B);
    }

    [Fact]
    public void Accent2_is_cyan_in_dark_and_red_in_light()
    {
        var dark = ThemePalettes.For(AppTheme.Dark)["Accent2Brush"];
        var light = ThemePalettes.For(AppTheme.Light)["Accent2Brush"];

        // Dark accent 2 is cyan (blue/green dominate, red near-zero).
        Assert.True(dark.B > dark.R);
        // Light accent 2 (used for progress bars) is red (red dominates green and blue).
        Assert.True(light.R > light.G);
        Assert.True(light.R > light.B);
    }
}
