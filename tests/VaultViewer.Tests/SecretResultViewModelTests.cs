using VaultViewer.Models;
using VaultViewer.Services;
using VaultViewer.ViewModels;
using Xunit;

namespace VaultViewer.Tests;

public class SecretResultViewModelTests
{
    private sealed class FakeAzure : IAzureService
    {
        private readonly string _value;
        private readonly bool _throw;
        public int GetCalls { get; private set; }

        public FakeAzure(string value = "s3cret", bool throwOnGet = false)
        {
            _value = value;
            _throw = throwOnGet;
        }

        public Task<IReadOnlyList<SubscriptionInfo>> DiscoverAsync(IProgress<DiscoveryProgress>? p, CancellationToken ct) =>
            Task.FromResult((IReadOnlyList<SubscriptionInfo>)new List<SubscriptionInfo>());

        public Task SearchSecretsAsync(string q, IReadOnlyList<VaultInfo> v, IProgress<ScanProgress>? p,
            Action<SecretMatch> onMatch, CancellationToken ct) => Task.CompletedTask;

        public Task<string> GetSecretValueAsync(Uri vaultUri, string secretName, CancellationToken ct)
        {
            GetCalls++;
            if (_throw)
                throw new InvalidOperationException("denied");
            return Task.FromResult(_value);
        }
    }

    private sealed class FakeClipboard : IClipboardService
    {
        public string? Text { get; private set; }
        public void SetText(string text) => Text = text;
    }

    private static SecretMatch Match() => new()
    {
        SecretName = "connString",
        VaultName = "v1",
        VaultUri = new Uri("https://v1.vault.azure.net/"),
        SubscriptionName = "Sub",
        SubscriptionId = "sub",
        ResourceGroup = "rg"
    };

    private static SecretResultViewModel New(IAzureService azure, IClipboardService? clip = null, Action<string>? status = null) =>
        new(Match(), azure, clip ?? new FakeClipboard(), status ?? (_ => { }), "conn");

    [Fact]
    public void Value_is_masked_until_revealed()
    {
        var vm = New(new FakeAzure());
        Assert.False(vm.IsRevealed);
        Assert.Equal("••••••••", vm.DisplayValue);
    }

    [Fact]
    public void View_reveals_the_value()
    {
        var vm = New(new FakeAzure("hello"));

        vm.ViewCommand.Execute(null);

        Assert.True(vm.IsRevealed);
        Assert.Equal("hello", vm.DisplayValue);
    }

    [Fact]
    public void View_toggles_back_to_masked()
    {
        var vm = New(new FakeAzure("hello"));

        vm.ViewCommand.Execute(null); // reveal
        vm.ViewCommand.Execute(null); // hide

        Assert.False(vm.IsRevealed);
        Assert.Equal("••••••••", vm.DisplayValue);
    }

    [Fact]
    public void Value_is_fetched_once_and_cached()
    {
        var azure = new FakeAzure("hello");
        var vm = New(azure);

        vm.ViewCommand.Execute(null); // fetch
        vm.ViewCommand.Execute(null); // hide
        vm.ViewCommand.Execute(null); // reveal again — should reuse cache

        Assert.Equal(1, azure.GetCalls);
    }

    [Fact]
    public void Copy_places_the_value_on_the_clipboard()
    {
        var clip = new FakeClipboard();
        var vm = New(new FakeAzure("topsecret"), clip);

        vm.CopyCommand.Execute(null);

        Assert.Equal("topsecret", clip.Text);
    }

    [Fact]
    public void View_failure_keeps_it_masked_and_reports_status()
    {
        string? status = null;
        var vm = New(new FakeAzure(throwOnGet: true), status: s => status = s);

        vm.ViewCommand.Execute(null);

        Assert.False(vm.IsRevealed);
        Assert.Equal("••••••••", vm.DisplayValue);
        Assert.NotNull(status);
        Assert.Contains("Could not read", status);
    }

    [Fact]
    public void Copy_failure_reports_status_and_leaves_clipboard_empty()
    {
        string? status = null;
        var clip = new FakeClipboard();
        var vm = New(new FakeAzure(throwOnGet: true), clip, s => status = s);

        vm.CopyCommand.Execute(null);

        Assert.Null(clip.Text);
        Assert.NotNull(status);
        Assert.Contains("Could not copy", status);
    }
}
