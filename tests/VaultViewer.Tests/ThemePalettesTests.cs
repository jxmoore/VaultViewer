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
        // App.xaml's BgColor is #FF161A21; toggling back to dark must restore it exactly.
        var bg = ThemePalettes.For(AppTheme.Dark)["BgBrush"];
        Assert.Equal((0xFF, 0x16, 0x1A, 0x21), (bg.A, bg.R, bg.G, bg.B));
    }

    [Fact]
    public void Light_background_is_lighter_than_dark_background()
    {
        var dark = ThemePalettes.For(AppTheme.Dark)["BgBrush"];
        var light = ThemePalettes.For(AppTheme.Light)["BgBrush"];
        Assert.True(light.R + light.G + light.B > dark.R + dark.G + dark.B);
    }

    [Fact]
    public void Accent_is_shared_across_themes()
    {
        Assert.Equal(ThemePalettes.For(AppTheme.Dark)["AccentBrush"],
                     ThemePalettes.For(AppTheme.Light)["AccentBrush"]);
    }
}
