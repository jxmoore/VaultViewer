namespace VaultViewer.Models;

public sealed class CustomThemeSettings
{
    public Dictionary<string, string> Colors { get; set; } = new();
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 13;
    public double WindowOpacity { get; set; } = 1.0;
}
