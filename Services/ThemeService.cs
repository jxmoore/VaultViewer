using System.Windows;
using System.Windows.Media;

namespace VaultViewer.Services;

/// <summary>
/// Swaps the theme by replacing the shared brush resources. Controls reference these
/// brushes via DynamicResource, so replacing an entry repaints the whole UI live
/// (and works even though the brushes are frozen).
/// </summary>
public sealed class ThemeService : IThemeService
{
    public void Apply(AppTheme theme)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        foreach (var (key, color) in ThemePalettes.For(theme))
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[key] = brush;
        }
    }
}
