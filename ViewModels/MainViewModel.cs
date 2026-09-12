using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using VaultViewer.Models;
using VaultViewer.Services;

namespace VaultViewer.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IAzureService _azure;
    private readonly IThemeService _theme;
    private readonly IClipboardService _clipboard;

    // How results marshal back onto the UI thread. Injectable so tests can run synchronously.
    private readonly Action<Action> _dispatch;

    private AppTheme _currentTheme = AppTheme.Dark;

    private string _vaultFilter = string.Empty;
    private string _subscriptionFilter = string.Empty;
    private string _searchQuery = string.Empty;
    private string _statusText = "Ready.";
    private bool _isLoading;
    private bool _isSearching;
    private bool _hasLoaded;
    private int _scanProgress;
    private int _scanTotal;
    private int _loadProgress;
    private int _loadTotal;
    private bool _hasSearched;
    private string _lastSearchQuery = string.Empty;
    private CancellationTokenSource? _searchCts;

    /// <summary>Production constructor used by the view — real Azure + theme + clipboard services.</summary>
    public MainViewModel()
        : this(new AzureService(new DefaultCredentialFactory()), new ThemeService(), new WpfClipboardService())
    {
    }

    /// <summary>
    /// Testable constructor. <paramref name="dispatch"/> defaults to marshalling onto the
    /// current Dispatcher; tests pass a synchronous version.
    /// </summary>
    public MainViewModel(IAzureService azure, IThemeService theme, IClipboardService clipboard,
        Action<Action>? dispatch = null)
    {
        _azure = azure;
        _theme = theme;
        _clipboard = clipboard;
        var dispatcher = Dispatcher.CurrentDispatcher;
        _dispatch = dispatch ?? (action => dispatcher.BeginInvoke(action));

        VaultsView = CollectionViewSource.GetDefaultView(Vaults);
        VaultsView.Filter = FilterVault;

        SubscriptionsView = CollectionViewSource.GetDefaultView(Subscriptions);
        SubscriptionsView.Filter = o => o is SubscriptionGroupViewModel g && g.IsVisible;

        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync(), _ => !IsLoading);
        SearchCommand = new AsyncRelayCommand(_ => SearchAsync(), _ => !IsSearching && HasLoaded);
        CancelSearchCommand = new RelayCommand(_ => _searchCts?.Cancel(), _ => IsSearching);
        ToggleSelectionCommand = new RelayCommand(_ => SetAllSelected(SelectedVaultCount == 0), _ => Vaults.Count > 0);
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
    }

    // ---- Theme toggle ----
    public RelayCommand ToggleThemeCommand { get; }

    /// <summary>Label/icon for the toggle — it advertises the theme you'd switch TO.</summary>
    public string ThemeToggleContent => _currentTheme == AppTheme.Dark ? "☀  Light" : "🌙  Dark";

    public string ThemeToggleTooltip =>
        _currentTheme == AppTheme.Dark ? "Switch to light mode" : "Switch to dark mode";

    private void ToggleTheme()
    {
        _currentTheme = _currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        _theme.Apply(_currentTheme);
        OnPropertyChanged(nameof(ThemeToggleContent));
        OnPropertyChanged(nameof(ThemeToggleTooltip));
    }

    // ---- Left pane: flat vault list (shared selection instances) ----
    public ObservableCollection<SelectableVault> Vaults { get; } = new();
    public ICollectionView VaultsView { get; }

    // ---- Left pane: subscription groups ----
    public ObservableCollection<SubscriptionGroupViewModel> Subscriptions { get; } = new();
    public ICollectionView SubscriptionsView { get; }

    // ---- Right pane: global search results ----
    public ObservableCollection<SecretResultViewModel> Results { get; } = new();

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public RelayCommand CancelSearchCommand { get; }
    public RelayCommand ToggleSelectionCommand { get; }

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
        private set
        {
            if (SetField(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsReady));
                OnPropertyChanged(nameof(SelectionSummary));
            }
        }
    }

    public bool IsReady => !IsLoading;

    /// <summary>The left-pane header text: "Loading…" during discovery, otherwise the selected count.</summary>
    public string SelectionSummary =>
        IsLoading ? "Loading…" : $"{SelectedVaultCount} of {VaultCount} selected";

    public bool IsSearching
    {
        get => _isSearching;
        private set { if (SetField(ref _isSearching, value)) RaiseResultStates(); }
    }

    /// <summary>The query the current results belong to, shown in the empty-state message.</summary>
    public string LastSearchQuery
    {
        get => _lastSearchQuery;
        private set => SetField(ref _lastSearchQuery, value);
    }

    /// <summary>True while a search is running and nothing has come back yet — show the skeleton.</summary>
    public bool ShowSearchSkeleton => SearchViewState.ShowSkeleton(IsSearching, Results.Count);

    /// <summary>True when a finished search found nothing — show the empty-state message.</summary>
    public bool ShowNoResults => SearchViewState.ShowNoResults(_hasSearched, IsSearching, Results.Count);

    private void RaiseResultStates()
    {
        OnPropertyChanged(nameof(ShowSearchSkeleton));
        OnPropertyChanged(nameof(ShowNoResults));
    }

    public bool HasLoaded
    {
        get => _hasLoaded;
        private set => SetField(ref _hasLoaded, value);
    }

    public int VaultCount => Vaults.Count;
    public int SubscriptionCount => Subscriptions.Count;
    public int SelectedVaultCount => Vaults.Count(v => v.IsSelected);

    /// <summary>Label for the selection link: "Select all" when nothing is selected, else "Clear selection".</summary>
    public string SelectionActionLabel => SelectedVaultCount == 0 ? "Select all" : "Clear selection";

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

    /// <summary>Subscriptions discovered so far during the initial load.</summary>
    public int LoadProgress
    {
        get => _loadProgress;
        private set => SetField(ref _loadProgress, value);
    }

    /// <summary>Total subscriptions to discover (0 while the list is still being enumerated).</summary>
    public int LoadTotal
    {
        get => _loadTotal;
        private set => SetField(ref _loadTotal, value);
    }

    /// <summary>Discover subscriptions and vaults. Runs on startup and on Refresh.</summary>
    public async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        HasLoaded = false;
        LoadProgress = 0;
        LoadTotal = 0;
        StatusText = "Signing in and discovering vaults…";
        foreach (var sv in Vaults)
            sv.PropertyChanged -= OnVaultSelectionChanged;
        Vaults.Clear();
        Subscriptions.Clear();

        try
        {
            var progress = new Progress<DiscoveryProgress>(p =>
            {
                LoadProgress = p.Completed;
                LoadTotal = p.Total;
                StatusText = p.Message;
            });
            var subs = await Task.Run(() => _azure.DiscoverAsync(progress, CancellationToken.None));

            // One SelectableVault per discovered vault, shared between both tabs so
            // selection is consistent everywhere.
            var selectableByVault = subs.SelectMany(s => s.Vaults)
                                        .ToDictionary(v => v, v => new SelectableVault(v));

            foreach (var sv in selectableByVault.Values.OrderBy(sv => sv.Vault.Name, StringComparer.OrdinalIgnoreCase))
            {
                sv.PropertyChanged += OnVaultSelectionChanged;
                Vaults.Add(sv);
            }

            foreach (var s in subs.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                var groupVaults = s.Vaults.Select(v => selectableByVault[v]).ToList();
                Subscriptions.Add(new SubscriptionGroupViewModel(s, groupVaults));
            }

            OnPropertyChanged(nameof(VaultCount));
            OnPropertyChanged(nameof(SubscriptionCount));
            OnPropertyChanged(nameof(SelectedVaultCount));
            OnPropertyChanged(nameof(SelectionActionLabel));
            OnPropertyChanged(nameof(SelectionSummary));
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

    public async Task SearchAsync()
    {
        var query = SearchQuery?.Trim() ?? string.Empty;
        if (query.Length == 0)
        {
            StatusText = "Enter part of a secret name to search for.";
            return;
        }

        var vaultsSnapshot = Vaults.Where(v => v.IsSelected).Select(v => v.Vault).ToList();
        if (vaultsSnapshot.Count == 0)
        {
            StatusText = "No vaults selected — tick at least one vault on the left to search.";
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        Results.Clear();
        LastSearchQuery = query;
        _hasSearched = true;
        IsSearching = true;
        ScanProgress = 0;
        ScanTotal = vaultsSnapshot.Count;
        StatusText = $"Searching {vaultsSnapshot.Count} selected vault(s) for secrets containing \"{query}\"…";

        var found = 0;

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
                _dispatch(() =>
                {
                    Results.Add(new SecretResultViewModel(match, _azure, _clipboard, m => StatusText = m, query));
                    RaiseResultStates(); // first result hides the skeleton
                });
            }

            await Task.Run(() => _azure.SearchSecretsAsync(query, vaultsSnapshot, progress, OnMatch, ct), ct);

            StatusText = found == 0
                ? $"No secrets found containing \"{query}\" in {vaultsSnapshot.Count} selected vault(s)."
                : $"Found {found} secret(s) containing \"{query}\" across {vaultsSnapshot.Count} selected vault(s).";
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
        if (obj is not SelectableVault sv)
            return false;
        var v = sv.Vault;
        return v.Name.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase) ||
               v.SubscriptionName.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase) ||
               v.ResourceGroup.Contains(_vaultFilter, StringComparison.OrdinalIgnoreCase);
    }

    private void SetAllSelected(bool selected)
    {
        foreach (var v in Vaults)
            v.IsSelected = selected;
    }

    private void OnVaultSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableVault.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedVaultCount));
            OnPropertyChanged(nameof(SelectionActionLabel));
            OnPropertyChanged(nameof(SelectionSummary));
        }
    }
}
