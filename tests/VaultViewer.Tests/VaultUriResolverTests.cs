using VaultViewer;
using Xunit;

namespace VaultViewer.Tests;

public class VaultUriResolverTests
{
    [Fact]
    public void Prefers_the_uri_from_the_arm_payload()
    {
        var given = new Uri("https://explicit.vault.azure.net/");

        var result = VaultUriResolver.Resolve(given, "somethingelse");

        Assert.Same(given, result);
    }

    [Fact]
    public void Falls_back_to_the_conventional_host_from_the_name()
    {
        var result = VaultUriResolver.Resolve(null, "myvault");

        Assert.Equal(new Uri("https://myvault.vault.azure.net/"), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Returns_null_when_no_uri_and_no_name(string? name)
    {
        Assert.Null(VaultUriResolver.Resolve(null, name));
    }
}
