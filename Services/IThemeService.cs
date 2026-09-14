using VaultViewer.Models;

namespace VaultViewer.Services;

/// <summary>Applies a colour theme to the running application.</summary>
public interface IThemeService
{
    /// <summary>The last theme applied via <see cref="Apply"/> (defaults to <see cref="AppTheme.Dark"/>,
    /// matching the colors baked into App.xaml at startup).</summary>
    AppTheme Current { get; }

    void Apply(AppTheme theme);
    void ApplyCustomSettings(CustomThemeSettings settings);
}
