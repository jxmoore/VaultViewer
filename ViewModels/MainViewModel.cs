using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using VaultViewer.Models;
using VaultViewer.Services;

namespace VaultViewer.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly AzureService _azure = new();
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

    private string _vaultFilter = string.Empty;
    private string _subscriptionFilter = string.Empty;
    private string _searchQuery = string.Empty;
    private string _statusText = "Ready.";
    private bool _isLoading;
    private bool _isSearching;
    private bool _hasLoaded;
    private int _scanProgress;
    private int _scanTotal;
    private CancellationTokenSource? _searchCts;

    public MainViewModel()
    {
        VaultsView = CollectionViewSource.GetDefaultView(Vaults);
        VaultsView.Filter = FilterVault;

        SubscriptionsView = CollectionViewSource.GetDefaultView(Subscriptions);
        SubscriptionsView.Filter = o => o is SubscriptionGroupViewModel g && g.IsVisible;

        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync(), _ => !IsLoading);
        SearchCommand = new AsyncRelayCommand(_ => SearchAsync(), _ => !IsSearching && HasLoaded);
        CancelSearchCommand = new RelayCommand(_ => _searchCts?.Cancel(), _ => IsSearching);
    }

    // ---- Left pane: flat vault list ----
    public ObservableCollection<VaultInfo> Vaults { get; } = new();
    public ICollectionView VaultsView { get; }

    // ---- Left pane: subscription groups ----
    public ObservableCollection<SubscriptionGroupViewModel> Subscriptions { get; } = new();
    public ICollectionView SubscriptionsView { get; }

    // ---- Right pane: global search results ----
    public ObservableCollection<SecretResultViewModel> Results { get; } = new();

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public RelayCommand CancelSearchCommand { get; }

    public string VaultFilter
    {
        get => _vaultFilter;
        set { if (SetField(ref _vaultFilter, value)) VaultsView.Refresh(); }
    }

    public string SubscriptionFilter
    {
        get => _subscriptionFilter;
        set
        {
            if (!SetField(ref _subscriptionFilter, value)) return;
            foreach (var group in Subscriptions)
                group.ApplyFilter(value);
            SubscriptionsView.Refresh();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetField(ref _searchQuery, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { if (SetField(ref _isLoading, value)) OnPropertyChanged(nameof(IsReady)); }
    }

    public bool IsReady => !IsLoading;

    public bool IsSearching
    {
        get => _isSearching;
        private set => SetField(ref _isSearching, value);
    }

    public bool HasLoaded
    {
        get => _hasLoaded;
        private set => SetField(ref _hasLoaded, value);
    }

    public int VaultCount => Vaults.Count;
    public int SubscriptionCount => Subscriptions.Count;

    public int ScanProgress
    {
        get => _scanProgress;
        private set => SetField(ref _scanProgress, value);
    }

    public int ScanTotal
    {
        get => _scanTotal;
        private set => SetField(ref _scanTotal, value);
    }

    /// <summary>Discover subscriptions and vaults. Runs on startup and on Refresh.</summary>
    public async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        HasLoaded = false;
        StatusText = "Signing in and discovering vaults…";
        Vaults.Clear();
        Subscriptions.Clear();

        try
        {
            var progress = new Progress<string>(msg => StatusText = msg);
            var subs = await Task.Run(() => _azure.DiscoverAsync(progress, CancellationToken.None));

            var allVaults = subs.SelectMany(s => s.Vaults)
                                .OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var v in allVaults)
                Vaults.Add(v);

            foreach (var s in subs.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase))
                Subscriptions.Add(new SubscriptionGroupViewModel(s));

            OnPropertyChanged(nameof(VaultCount));
            OnPropertyChanged(nameof(SubscriptionCount));
            HasLoaded = true;
            StatusText = $"Found {Vaults.Count} vault(s) across {Subscriptions.Count} subscription(s).";
        }
        catch (Exception ex)
        {
            StatusText = $"Discovery failed: {ex.Message}. Try running 'az login' and click Refresh.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchAsync()
    {
        var query = SearchQuery?.Trim() ?? string.Empty;
        if (query.Length == 0)
        {
            StatusText = "Enter part of a secret name to search for.";
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        Results.Clear();
        IsSearching = true;
        ScanProgress = 0;
        ScanTotal = Vaults.Count;
        StatusText = $"Searching {Vaults.Count} vault(s) for secrets containing \"{query}\"…";

        var found = 0;
        var vaultsSnapshot = Vaults.ToList();

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                ScanProgress = p.VaultsScanned;
                ScanTotal = p.VaultsTotal;
            });

            void OnMatch(SecretMatch match)
            {
                Interlocked.Increment(ref found);
                // Marshal back to the UI thread to touch the ObservableCollection.
                _dispatcher.BeginInvoke(() =>
                    Results.Add(new SecretResultViewModel(match, _azure, m => StatusText = m)));
            }

            await Task.Run(() => _azure.SearchSecretsAsync(query, vaultsSnapshot, progress, OnMatch, ct), ct);

            StatusText = found == 0
                ? $"No secrets found containing \"{query}\"."
                : $"Found {found} secret(s) containing \"{query}\" across {Vaults.Count} vault(s).";
        }
        catch (OperationCanceledException)
        {
            StatusText = $"Search cancelled. {found} match(es) so far.";
        }
        catch (Exception ex)
        {
            StatusText = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool FilterVault(object obj)
    {
        if (_vaultFilter.Length == 0)
            return true;
        return obj is VaultInfo v &&
               (v.Name.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase) ||
                v.SubscriptionName.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase) ||
                v.ResourceGroup.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase));
    }
}
