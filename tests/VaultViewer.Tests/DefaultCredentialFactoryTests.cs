using Azure.Identity;
using VaultViewer.Services;
using Xunit;

namespace VaultViewer.Tests;

public class DefaultCredentialFactoryTests
{
    [Fact]
    public void BuildChain_tries_default_chain_first_then_interactive_browser()
    {
        var chain = new DefaultCredentialFactory().BuildChain();

        Assert.Equal(2, chain.Count);
        Assert.IsType<DefaultAzureCredential>(chain[0]);
        Assert.IsType<InteractiveBrowserCredential>(chain[^1]);
    }

    [Fact]
    public void Create_returns_a_chained_credential()
    {
        var credential = new DefaultCredentialFactory().Create();

        Assert.IsType<ChainedTokenCredential>(credential);
    }
}
