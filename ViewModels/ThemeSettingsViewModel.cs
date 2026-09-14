using System.Collections.ObjectModel;
using System.Drawing.Text;
using VaultViewer.Models;
using VaultViewer.Services;

namespace VaultViewer.ViewModels;

public sealed class ColorEntryViewModel : ViewModelBase
{
    private string _hex;

    public string Label { get; }
    public string Key { get; }

    public string Hex
    {
        get => _hex;
        set => SetField(ref _hex, value);
    }

    public ColorEntryViewModel(string key, string hex)
    {
        Key = key;
        Label = FriendlyLabel(key);
        _hex = hex;
    }

    private static string FriendlyLabel(string key) => key switch
    {
        "BgBrush"          => "Background",
        "PanelBrush"       => "Panel",
        "PanelAltBrush"    => "Panel Alt",
        "BorderBrush"      => "Border",
        "AccentBrush"      => "Accent",
        "AccentHoverBrush" => "Accent Hover",
        "AccentSoftBrush"  => "Accent Soft",
        "Accent2Brush"     => "Accent 2",
        "TextBrush"        => "Text",
        "MutedBrush"       => "Muted Text",
        _                  => key.Replace("Brush", ""),
    };
}

public sealed class ThemeSettingsViewModel : ViewModelBase
{
    private readonly IThemeSettingsService _service;
    private readonly IThemeService _themeService;
    private string _selectedFont = "Segoe UI";
    private double _fontSize = 13;
    private double _windowOpacity = 1.0;

    public event EventHandler? CloseRequested;

    public ObservableCollection<ColorEntryViewModel> ColorEntries { get; } = new();

    public IReadOnlyList<string> AvailableFonts { get; }

    public string SelectedFont
    {
        get => _selectedFont;
        set => SetField(ref _selectedFont, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetField(ref _fontSize, Math.Clamp(value, 9, 20));
    }

    public double WindowOpacity
    {
        get => _windowOpacity;
        set => SetField(ref _windowOpacity, Math.Clamp(value, 0.3, 1.0));
    }

    public string SettingsFilePath => _service.SettingsFilePath;

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand RestoreDefaultsCommand { get; }

    public ThemeSettingsViewModel(IThemeSettingsService service, IThemeService themeService)
    {
        _service = service;
        _themeService = themeService;

        using var fc = new InstalledFontCollection();
        AvailableFonts = fc.Families
            .Select(f => f.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var key in ThemePalettes.Keys)
            ColorEntries.Add(new ColorEntryViewModel(key, DefaultHex(key)));

        Load();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        RestoreDefaultsCommand = new RelayCommand(_ => RestoreDefaults());
    }

    private void RestoreDefaults()
    {
        foreach (var entry in ColorEntries)
            entry.Hex = DefaultHex(entry.Key);

        SelectedFont = "Segoe UI";
        FontSize = 13;
        WindowOpacity = 1.0;
    }

    private void Load()
    {
        var settings = _service.Load();
        var colors = ColorsFor(settings);

        foreach (var entry in ColorEntries)
        {
            if (colors.TryGetValue(entry.Key, out var hex) && !string.IsNullOrWhiteSpace(hex))
                entry.Hex = hex;
        }

        if (!string.IsNullOrWhiteSpace(settings.FontFamily))
            SelectedFont = settings.FontFamily;

        FontSize = settings.FontSize > 0 ? settings.FontSize : 13;
        WindowOpacity = settings.WindowOpacity > 0 ? settings.WindowOpacity : 1.0;
    }

    private void ExecuteSave()
    {
        // Re-load rather than start from a blank settings object, so the *other* theme's
        // saved colors aren't wiped out by a save made while this theme is active.
        var settings = _service.Load();
        settings.FontFamily = SelectedFont;
        settings.FontSize = FontSize;
        settings.WindowOpacity = WindowOpacity;

        var colors = ColorsFor(settings);
        colors.Clear();
        foreach (var entry in ColorEntries)
            colors[entry.Key] = entry.Hex;

        _service.Save(settings);
        _themeService.ApplyCustomSettings(settings);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private Dictionary<string, string> ColorsFor(CustomThemeSettings settings) =>
        _themeService.Current == AppTheme.Light ? settings.LightColors : settings.DarkColors;

    private string DefaultHex(string key)
    {
        var color = ThemePalettes.For(_themeService.Current)[key];
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
