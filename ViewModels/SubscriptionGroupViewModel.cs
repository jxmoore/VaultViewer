using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using VaultViewer.Models;

namespace VaultViewer.ViewModels;

/// <summary>
/// A subscription plus its vaults, for the "Subscriptions" tab tree. Applies the
/// shared filter text against both the subscription name and its vault names, and
/// offers a tri-state "select all vaults in this subscription" checkbox.
/// </summary>
public sealed class SubscriptionGroupViewModel : ViewModelBase
{
    private string _filter = string.Empty;

    public SubscriptionGroupViewModel(SubscriptionInfo info, IReadOnlyList<SelectableVault> vaults)
    {
        SubscriptionName = info.DisplayName;
        SubscriptionId = info.SubscriptionId;
        Vaults = new ObservableCollection<SelectableVault>(
            vaults.OrderBy(v => v.Vault.Name, StringComparer.OrdinalIgnoreCase));
        VaultsView = CollectionViewSource.GetDefaultView(Vaults);
        VaultsView.Filter = VaultMatchesFilter;

        foreach (var v in Vaults)
            v.PropertyChanged += OnVaultChanged;

        ToggleAllCommand = new RelayCommand(_ => ToggleAll());
    }

    public string SubscriptionName { get; }

    public string SubscriptionId { get; }

    public int VaultCount => Vaults.Count;

    public ObservableCollection<SelectableVault> Vaults { get; }

    public ICollectionView VaultsView { get; }

    public RelayCommand ToggleAllCommand { get; }

    /// <summary>True only when every vault in the subscription is selected (plain two-state, no mixed).</summary>
    public bool IsAllSelected => Vaults.All(v => v.IsSelected);

    /// <summary>True when the group itself or any of its vaults match the current filter.</summary>
    public bool IsVisible { get; private set; } = true;

    public void ApplyFilter(string filter)
    {
        _filter = filter ?? string.Empty;
        VaultsView.Refresh();

        var subMatches = _filter.Length == 0 ||
                         SubscriptionName.Contains(_filter, StringComparison.OrdinalIgnoreCase);
        var anyVaultMatches = _filter.Length == 0 ||
                              Vaults.Any(v => v.Vault.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase));

        IsVisible = subMatches || anyVaultMatches;
        OnPropertyChanged(nameof(IsVisible));
    }

    private void ToggleAll()
    {
        // If anything is unselected, select all; otherwise clear all.
        var target = Vaults.Any(v => !v.IsSelected);
        foreach (var v in Vaults)
            v.IsSelected = target;
    }

    private void OnVaultChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableVault.IsSelected))
            OnPropertyChanged(nameof(IsAllSelected));
    }

    private bool VaultMatchesFilter(object obj)
    {
        if (_filter.Length == 0)
            return true;
        if (SubscriptionName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            return true; // matching the subscription shows all its vaults
        return obj is SelectableVault v && v.Vault.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase);
    }
}
