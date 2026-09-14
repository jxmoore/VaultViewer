namespace VaultViewer.Models;

public sealed class CustomThemeSettings
{
    // Colors are kept separately per theme so toggling Dark/Light doesn't leak one
    // theme's saved palette into the other (font/size/opacity stay shared).
    public Dictionary<string, string> DarkColors { get; set; } = new();
    public Dictionary<string, string> LightColors { get; set; } = new();
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 13;
    public double WindowOpacity { get; set; } = 1.0;
}
