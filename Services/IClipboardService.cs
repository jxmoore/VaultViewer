namespace VaultViewer.Services;

/// <summary>Abstraction over the system clipboard so copy logic can be unit tested.</summary>
public interface IClipboardService
{
    void SetText(string text);
}
