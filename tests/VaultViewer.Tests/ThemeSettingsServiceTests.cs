using System.IO;
using VaultViewer.Models;
using VaultViewer.Services;

namespace VaultViewer.Tests;

public class ThemeSettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;

    public ThemeSettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "VaultViewer.Tests." + Guid.NewGuid().ToString("N"));
        _tempFile = Path.Combine(_tempDir, "theme-settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Save_creates_file_and_Load_round_trips_settings()
    {
        var service = new ThemeSettingsService(_tempFile);
        var original = new CustomThemeSettings
        {
            FontFamily = "Consolas",
            FontSize = 15,
            WindowOpacity = 0.8,
            DarkColors = new Dictionary<string, string>
            {
                ["BgBrush"] = "#FF141413",
                ["AccentBrush"] = "#FFFF5A2C",
            },
            LightColors = new Dictionary<string, string>
            {
                ["BgBrush"] = "#FFF4F5F7",
            }
        };

        service.Save(original);
        var loaded = service.Load();

        Assert.Equal(original.FontFamily, loaded.FontFamily);
        Assert.Equal(original.FontSize, loaded.FontSize);
        Assert.Equal(original.WindowOpacity, loaded.WindowOpacity);
        Assert.Equal(original.DarkColors["BgBrush"], loaded.DarkColors["BgBrush"]);
        Assert.Equal(original.DarkColors["AccentBrush"], loaded.DarkColors["AccentBrush"]);
        Assert.Equal(original.LightColors["BgBrush"], loaded.LightColors["BgBrush"]);
    }

    [Fact]
    public void SettingsFilePath_returns_the_constructed_path()
    {
        var service = new ThemeSettingsService(_tempFile);

        Assert.Equal(_tempFile, service.SettingsFilePath);
    }

    [Fact]
    public void Load_returns_defaults_when_file_does_not_exist()
    {
        var service = new ThemeSettingsService(_tempFile);
        var settings = service.Load();

        Assert.Equal("Segoe UI", settings.FontFamily);
        Assert.Equal(13, settings.FontSize);
        Assert.Equal(1.0, settings.WindowOpacity);
        Assert.Empty(settings.DarkColors);
        Assert.Empty(settings.LightColors);
    }
}
