using System.Windows.Threading;
using VaultViewer;
using VaultViewer.Models;
using VaultViewer.Services;
using VaultViewer.ViewModels;
using Xunit;

namespace VaultViewer.Tests;

public class MainViewModelTests
{
    /// <summary>
    /// Runs an async body on a single STA dispatcher thread, pumping the dispatcher until
    /// it completes — so the view-model's collection views (which are thread-affine) behave
    /// exactly as they do on the real UI thread. Exceptions are rethrown to the test.
    /// </summary>
    private static void OnUi(Func<Task> body)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            var frame = new DispatcherFrame();
            _ = body().ContinueWith(t =>
            {
                error = t.Exception?.GetBaseException();
                frame.Continue = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Dispatcher.PushFrame(frame);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        if (error is not null)
            throw error;
    }

    // ---- Fakes ----

    private sealed class FakeAzure : IAzureService
    {
        public List<SubscriptionInfo> Subscriptions { get; } = new();
        public List<VaultInfo>? LastSearchedVaults { get; private set; }
        public bool SearchCalled { get; private set; }
        public bool EmitMatches { get; set; } = true;

        public Task<IReadOnlyList<SubscriptionInfo>> DiscoverAsync(
            IProgress<DiscoveryProgress>? progress, CancellationToken ct) =>
            Task.FromResult((IReadOnlyList<SubscriptionInfo>)Subscriptions);

        public Task SearchSecretsAsync(string query, IReadOnlyList<VaultInfo> vaults,
            IProgress<ScanProgress>? progress, Action<SecretMatch> onMatch, CancellationToken ct)
        {
            SearchCalled = true;
            LastSearchedVaults = vaults.ToList();
            if (!EmitMatches)
                return Task.CompletedTask;
            foreach (var v in vaults)
            {
                onMatch(new SecretMatch
                {
                    SecretName = query + "-secret",
                    VaultName = v.Name,
                    VaultUri = v.VaultUri,
                    SubscriptionName = v.SubscriptionName,
                    SubscriptionId = v.SubscriptionId,
                    ResourceGroup = v.ResourceGroup
                });
            }
            return Task.CompletedTask;
        }

        public Task<string> GetSecretValueAsync(Uri vaultUri, string secretName, CancellationToken ct) =>
            Task.FromResult("value");
    }

    private sealed class FakeTheme : IThemeService
    {
        public List<AppTheme> Applied { get; } = new();
        public void Apply(AppTheme theme) => Applied.Add(theme);
    }

    private sealed class FakeClipboard : IClipboardService
    {
        public string? Text { get; private set; }
        public void SetText(string text) => Text = text;
    }

    // ---- Helpers ----

    private static VaultInfo Vault(string name, string sub) => new()
    {
        Name = name,
        VaultUri = new Uri($"https://{name}.vault.azure.net/"),
        SubscriptionId = sub,
        SubscriptionName = sub,
        ResourceGroup = "rg"
    };

    private static FakeAzure AzureWith(params (string sub, string[] vaults)[] subs)
    {
        var fake = new FakeAzure();
        foreach (var (sub, vaults) in subs)
        {
            var info = new SubscriptionInfo { SubscriptionId = sub, DisplayName = sub };
            foreach (var v in vaults)
                info.Vaults.Add(Vault(v, sub));
            fake.Subscriptions.Add(info);
        }
        return fake;
    }

    private static MainViewModel NewVm(FakeAzure azure, FakeTheme? theme = null) =>
        // Synchronous dispatch so results land inline during the test.
        new(azure, theme ?? new FakeTheme(), new FakeClipboard(), dispatch: a => a());

    // ---- Load ----

    [Fact]
    public void LoadAsync_populates_vaults_and_subscriptions_all_selected() => OnUi(async () =>
    {
        var vm = NewVm(AzureWith(("SubA", new[] { "v1", "v2" }), ("SubB", new[] { "v3" })));

        await vm.LoadAsync();

        Assert.Equal(3, vm.VaultCount);
        Assert.Equal(2, vm.SubscriptionCount);
        Assert.Equal(3, vm.SelectedVaultCount);
        Assert.True(vm.HasLoaded);
        Assert.Equal("3 of 3 selected", vm.SelectionSummary);
    });

    [Fact]
    public void Selection_is_shared_between_the_two_tabs() => OnUi(async () =>
    {
        var vm = NewVm(AzureWith(("SubA", new[] { "v1", "v2" })));
        await vm.LoadAsync();

        var group = vm.Subscriptions.Single();
        var fromList = vm.Vaults.First();
        var fromTree = group.Vaults.Single(v => ReferenceEquals(v, fromList));

        // Same instance, so toggling in one place is seen in the other.
        Assert.Same(fromList, fromTree);
        fromList.IsSelected = false;
        Assert.False(fromTree.IsSelected);
        Assert.Equal(vm.Vaults.Count - 1, vm.SelectedVaultCount);
    });

    // ---- Search scoping ----

    [Fact]
    public void Search_only_scans_selected_vaults() => OnUi(async () =>
    {
        var azure = AzureWith(("SubA", new[] { "v1", "v2", "v3" }));
        var vm = NewVm(azure);
        await vm.LoadAsync();

        vm.Vaults.First(v => v.Vault.Name == "v2").IsSelected = false;
        vm.SearchQuery = "api";
        await vm.SearchAsync();

        Assert.NotNull(azure.LastSearchedVaults);
        Assert.Equal(2, azure.LastSearchedVaults!.Count);
        Assert.DoesNotContain(azure.LastSearchedVaults, v => v.Name == "v2");
        Assert.Equal(2, vm.Results.Count);
    });

    [Fact]
    public void Search_with_no_selection_does_not_call_azure() => OnUi(async () =>
    {
        var azure = AzureWith(("SubA", new[] { "v1" }));
        var vm = NewVm(azure);
        await vm.LoadAsync();

        foreach (var v in vm.Vaults) v.IsSelected = false;
        vm.SearchQuery = "api";
        await vm.SearchAsync();

        Assert.False(azure.SearchCalled);
        Assert.Contains("No vaults selected", vm.StatusText);
    });

    [Fact]
    public void Search_with_blank_query_is_ignored() => OnUi(async () =>
    {
        var azure = AzureWith(("SubA", new[] { "v1" }));
        var vm = NewVm(azure);
        await vm.LoadAsync();

        vm.SearchQuery = "   ";
        await vm.SearchAsync();

        Assert.False(azure.SearchCalled);
    });

    [Fact]
    public void Search_with_no_matches_shows_the_empty_state() => OnUi(async () =>
    {
        var azure = AzureWith(("SubA", new[] { "v1" }));
        azure.EmitMatches = false;
        var vm = NewVm(azure);
        await vm.LoadAsync();

        vm.SearchQuery = "api";
        await vm.SearchAsync();

        Assert.Empty(vm.Results);
        Assert.True(vm.ShowNoResults);
        Assert.Equal("api", vm.LastSearchQuery);
        Assert.Contains("No secrets found", vm.StatusText);
    });

    // ---- Selection toggle ----

    [Fact]
    public void ToggleSelection_clears_when_all_selected_then_selects_all() => OnUi(async () =>
    {
        var vm = NewVm(AzureWith(("SubA", new[] { "v1", "v2" })));
        await vm.LoadAsync();

        vm.ToggleSelectionCommand.Execute(null);
        Assert.Equal(0, vm.SelectedVaultCount);
        Assert.Equal("Select all", vm.SelectionActionLabel);

        vm.ToggleSelectionCommand.Execute(null);
        Assert.Equal(2, vm.SelectedVaultCount);
        Assert.Equal("Clear selection", vm.SelectionActionLabel);
    });

    // ---- Theme ----

    [Fact]
    public void ToggleTheme_flips_theme_and_applies_it() => OnUi(() =>
    {
        var theme = new FakeTheme();
        var vm = NewVm(new FakeAzure(), theme);

        Assert.Equal("☀  Light", vm.ThemeToggleContent); // starts dark

        vm.ToggleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Light, theme.Applied.Single());
        Assert.Equal("🌙  Dark", vm.ThemeToggleContent);

        vm.ToggleThemeCommand.Execute(null);
        Assert.Equal(new[] { AppTheme.Light, AppTheme.Dark }, theme.Applied);
        Assert.Equal("☀  Light", vm.ThemeToggleContent);
        return Task.CompletedTask;
    });
}
