using System.Windows;
using System.Windows.Media;
using VaultViewer.Services;

namespace VaultViewer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // WinForms interop: enable visual styles before any dialog is shown.
        System.Windows.Forms.Application.EnableVisualStyles();

        base.OnStartup(e);

        // Seed the typography resources so DynamicResource bindings in styles resolve
        // before the first frame renders. These are overridden by ApplyCustomSettings below.
        Resources["AppFontFamily"] = new FontFamily("Segoe UI");
        Resources["AppFontSize"] = 13.0;

        // Re-apply any saved custom colours, font, and size (opacity is set in MainWindow.Loaded
        // since MainWindow doesn't exist yet at this point).
        var settings = new ThemeSettingsService().Load();
        new ThemeService().ApplyCustomSettings(settings);
    }
}
