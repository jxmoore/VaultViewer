using System.IO;
using System.Text.Json;
using VaultViewer.Models;

namespace VaultViewer.Services;

public sealed class ThemeSettingsService : IThemeSettingsService
{
    private readonly string _path;

    public ThemeSettingsService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
               "VaultViewer", "theme-settings.json"))
    {
    }

    internal ThemeSettingsService(string path)
    {
        _path = path;
    }

    public string SettingsFilePath => _path;

    public CustomThemeSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
                return new CustomThemeSettings();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<CustomThemeSettings>(json) ?? new CustomThemeSettings();
        }
        catch
        {
            return new CustomThemeSettings();
        }
    }

    public void Save(CustomThemeSettings settings)
    {
        var dir = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(dir);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, options));
    }
}
