using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using VaultViewer.Models;

namespace VaultViewer.ViewModels;

/// <summary>
/// A subscription plus its vaults, for the "Subscriptions" tab tree. Applies the
/// shared filter text against both the subscription name and its vault names.
/// </summary>
public sealed class SubscriptionGroupViewModel : ViewModelBase
{
    private string _filter = string.Empty;

    public SubscriptionGroupViewModel(SubscriptionInfo info)
    {
        SubscriptionName = info.DisplayName;
        SubscriptionId = info.SubscriptionId;
        Vaults = new ObservableCollection<VaultInfo>(info.Vaults.OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase));
        VaultsView = CollectionViewSource.GetDefaultView(Vaults);
        VaultsView.Filter = VaultMatchesFilter;
    }

    public string SubscriptionName { get; }

    public string SubscriptionId { get; }

    public int VaultCount => Vaults.Count;

    public ObservableCollection<VaultInfo> Vaults { get; }

    public ICollectionView VaultsView { get; }

    /// <summary>True when the group itself or any of its vaults match the current filter.</summary>
    public bool IsVisible { get; private set; } = true;

    public void ApplyFilter(string filter)
    {
        _filter = filter ?? string.Empty;
        VaultsView.Refresh();

        var subMatches = _filter.Length == 0 ||
                         SubscriptionName.Contains(_filter, StringComparison.OrdinalIgnoreCase);
        var anyVaultMatches = _filter.Length == 0 ||
                              Vaults.Any(v => v.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase));

        IsVisible = subMatches || anyVaultMatches;
        OnPropertyChanged(nameof(IsVisible));
    }

    private bool VaultMatchesFilter(object obj)
    {
        if (_filter.Length == 0)
            return true;
        if (SubscriptionName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            return true; // matching the subscription shows all its vaults
        return obj is VaultInfo v && v.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase);
    }
}
