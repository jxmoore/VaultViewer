using VaultViewer.Models;

namespace VaultViewer.Services;

public interface IThemeSettingsService
{
    string SettingsFilePath { get; }
    CustomThemeSettings Load();
    void Save(CustomThemeSettings settings);
}
