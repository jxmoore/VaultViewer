using VaultViewer.Models;
using VaultViewer.ViewModels;
using Xunit;

namespace VaultViewer.Tests;

public class SubscriptionGroupFilterTests
{
    private static SelectableVault Vault(string name) => new(new VaultInfo
    {
        Name = name,
        VaultUri = new Uri($"https://{name}.vault.azure.net/"),
        SubscriptionId = "p",
        SubscriptionName = "Production",
        ResourceGroup = "rg"
    });

    private static SubscriptionGroupViewModel Group(params string[] vaultNames) =>
        new(new SubscriptionInfo { SubscriptionId = "p", DisplayName = "Production" },
            vaultNames.Select(Vault).ToList());

    private static List<SelectableVault> Visible(SubscriptionGroupViewModel g) =>
        g.VaultsView.Cast<SelectableVault>().ToList();

    [Fact]
    public void No_filter_shows_all_vaults()
    {
        var g = Group("alpha", "beta");
        g.ApplyFilter("");

        Assert.True(g.IsVisible);
        Assert.Equal(2, Visible(g).Count);
    }

    [Fact]
    public void Filtering_by_vault_name_shows_only_matches()
    {
        var g = Group("alpha", "beta");
        g.ApplyFilter("alp");

        Assert.True(g.IsVisible);
        Assert.Equal("alpha", Assert.Single(Visible(g)).Vault.Name);
    }

    [Fact]
    public void Matching_the_subscription_name_shows_all_its_vaults()
    {
        var g = Group("alpha", "beta");
        g.ApplyFilter("prod"); // matches "Production"

        Assert.True(g.IsVisible);
        Assert.Equal(2, Visible(g).Count);
    }

    [Fact]
    public void No_match_hides_the_group_and_all_vaults()
    {
        var g = Group("alpha", "beta");
        g.ApplyFilter("zzz");

        Assert.False(g.IsVisible);
        Assert.Empty(Visible(g));
    }

    [Fact]
    public void Filtering_is_case_insensitive()
    {
        var g = Group("Alpha", "beta");
        g.ApplyFilter("ALPHA");

        Assert.True(g.IsVisible);
        Assert.Equal("Alpha", Assert.Single(Visible(g)).Vault.Name);
    }
}
