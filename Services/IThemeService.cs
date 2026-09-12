namespace VaultViewer.Services;

/// <summary>Applies a colour theme to the running application.</summary>
public interface IThemeService
{
    void Apply(AppTheme theme);
}
