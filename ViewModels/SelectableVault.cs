using VaultViewer.Models;

namespace VaultViewer.ViewModels;

/// <summary>
/// A vault plus its "included in search" checkbox state. A single instance is shared
/// between the Vaults list and the Subscriptions tree, so ticking it in one tab is
/// reflected in the other.
/// </summary>
public sealed class SelectableVault : ViewModelBase
{
    private bool _isSelected = true;

    public SelectableVault(VaultInfo vault) => Vault = vault;

    public VaultInfo Vault { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}
