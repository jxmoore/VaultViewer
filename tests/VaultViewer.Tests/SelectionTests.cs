using VaultViewer.Models;
using VaultViewer.ViewModels;
using Xunit;

namespace VaultViewer.Tests;

public class SelectionTests
{
    private static SelectableVault MakeVault(string name) => new(new VaultInfo
    {
        Name = name,
        VaultUri = new Uri($"https://{name}.vault.azure.net/"),
        SubscriptionId = "sub-1",
        SubscriptionName = "Sub One",
        ResourceGroup = "rg-1"
    });

    private static SubscriptionGroupViewModel MakeGroup(params SelectableVault[] vaults) =>
        new(new SubscriptionInfo { SubscriptionId = "sub-1", DisplayName = "Sub One" }, vaults);

    [Fact]
    public void Vaults_are_selected_by_default()
    {
        Assert.True(MakeVault("a").IsSelected);
    }

    [Fact]
    public void Group_is_all_selected_when_every_vault_is_selected()
    {
        var group = MakeGroup(MakeVault("a"), MakeVault("b"));
        Assert.True(group.IsAllSelected);
    }

    [Fact]
    public void Group_is_not_all_selected_when_selection_is_mixed()
    {
        var a = MakeVault("a");
        var b = MakeVault("b");
        var group = MakeGroup(a, b);

        a.IsSelected = false;

        Assert.False(group.IsAllSelected);
    }

    [Fact]
    public void Group_is_unselected_when_no_vault_is_selected()
    {
        var a = MakeVault("a");
        var b = MakeVault("b");
        var group = MakeGroup(a, b);

        a.IsSelected = false;
        b.IsSelected = false;

        Assert.False(group.IsAllSelected);
    }

    [Fact]
    public void Empty_group_is_not_selected_and_cannot_toggle()
    {
        var group = MakeGroup();

        Assert.False(group.HasVaults);
        Assert.False(group.IsAllSelected);
        Assert.False(group.ToggleAllCommand.CanExecute(null));
    }

    [Fact]
    public void Nonempty_group_can_toggle()
    {
        var group = MakeGroup(MakeVault("a"));

        Assert.True(group.ToggleAllCommand.CanExecute(null));
    }

    [Fact]
    public void ToggleAll_clears_when_all_selected()
    {
        var a = MakeVault("a");
        var b = MakeVault("b");
        var group = MakeGroup(a, b);

        group.ToggleAllCommand.Execute(null);

        Assert.False(a.IsSelected);
        Assert.False(b.IsSelected);
    }

    [Fact]
    public void ToggleAll_selects_all_when_some_are_unselected()
    {
        var a = MakeVault("a");
        var b = MakeVault("b");
        var group = MakeGroup(a, b);
        a.IsSelected = false;

        group.ToggleAllCommand.Execute(null);

        Assert.True(a.IsSelected);
        Assert.True(b.IsSelected);
    }
}
