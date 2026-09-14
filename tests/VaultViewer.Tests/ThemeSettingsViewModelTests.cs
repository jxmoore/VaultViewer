using System.Linq;
using VaultViewer;
using VaultViewer.Models;
using VaultViewer.Services;
using VaultViewer.ViewModels;
using Xunit;

namespace VaultViewer.Tests;

public class ThemeSettingsViewModelTests
{
    private sealed class FakeSettingsService : IThemeSettingsService
    {
        public CustomThemeSettings Stored { get; set; } = new();
        public string SettingsFilePath => @"C:\fake\theme-settings.json";
        public CustomThemeSettings Load() => Stored;
        public void Save(CustomThemeSettings settings) => Stored = settings;
    }

    private sealed class FakeTheme : IThemeService
    {
        public AppTheme Current { get; private set; } = AppTheme.Dark;
        public void Apply(AppTheme theme) => Current = theme;
        public void ApplyCustomSettings(CustomThemeSettings settings) { }
    }

    [Fact]
    public void SettingsFilePath_reflects_the_underlying_service()
    {
        var vm = new ThemeSettingsViewModel(new FakeSettingsService(), new FakeTheme());

        Assert.Equal(@"C:\fake\theme-settings.json", vm.SettingsFilePath);
    }

    [Fact]
    public void RestoreDefaults_resets_font_opacity_and_colors_to_their_defaults()
    {
        var service = new FakeSettingsService
        {
            Stored = new CustomThemeSettings
            {
                FontFamily = "Consolas",
                FontSize = 18,
                WindowOpacity = 0.5,
                Colors = new Dictionary<string, string> { ["BgBrush"] = "#FF000000" }
            }
        };
        var vm = new ThemeSettingsViewModel(service, new FakeTheme());

        // Sanity: the loaded (non-default) values are in effect before restoring.
        Assert.Equal(18, vm.FontSize);
        Assert.Equal(0.5, vm.WindowOpacity);
        Assert.Equal("#FF000000", vm.ColorEntries.Single(e => e.Key == "BgBrush").Hex);

        vm.RestoreDefaultsCommand.Execute(null);

        Assert.Equal("Segoe UI", vm.SelectedFont);
        Assert.Equal(13, vm.FontSize);
        Assert.Equal(1.0, vm.WindowOpacity);
        Assert.NotEqual("#FF000000", vm.ColorEntries.Single(e => e.Key == "BgBrush").Hex);
    }

    [Fact]
    public void Color_defaults_reflect_the_currently_active_theme()
    {
        var theme = new FakeTheme();
        theme.Apply(AppTheme.Light);

        var vm = new ThemeSettingsViewModel(new FakeSettingsService(), theme);

        var expected = ThemePalettes.For(AppTheme.Light)["BgBrush"];
        var expectedHex = $"#{expected.A:X2}{expected.R:X2}{expected.G:X2}{expected.B:X2}";
        Assert.Equal(expectedHex, vm.ColorEntries.Single(e => e.Key == "BgBrush").Hex);
    }
}
