using System.Windows;

namespace VaultViewer.Services;

/// <summary>The real clipboard, backed by WPF's <see cref="Clipboard"/>.</summary>
public sealed class WpfClipboardService : IClipboardService
{
    public void SetText(string text) => Clipboard.SetText(text);
}
