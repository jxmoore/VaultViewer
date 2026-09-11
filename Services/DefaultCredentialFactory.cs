using Azure.Core;
using Azure.Identity;

namespace VaultViewer.Services;

/// <summary>
/// Builds a credential that first tries the full <see cref="DefaultAzureCredential"/>
/// chain (Azure CLI, Azure PowerShell, Visual Studio, environment vars, managed
/// identity) and, if none of those are available, falls back to an interactive
/// browser sign-in. The interactive sign-in is cached so the prompt only appears once.
/// </summary>
public sealed class DefaultCredentialFactory : ICredentialFactory
{
    private const string TokenCacheName = "VaultViewer";

    public TokenCredential Create() => new ChainedTokenCredential(BuildChain().ToArray());

    /// <summary>
    /// The ordered credential sources. Exposed for tests: index 0 is the
    /// non-interactive chain, the last entry is the interactive browser fallback.
    /// </summary>
    public IReadOnlyList<TokenCredential> BuildChain() => new TokenCredential[]
    {
        new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            Retry = { MaxRetries = 2 }
        }),
        CreateInteractiveBrowser()
    };

    private static InteractiveBrowserCredential CreateInteractiveBrowser()
    {
        var options = new InteractiveBrowserCredentialOptions
        {
            // Persist the sign-in (DPAPI on Windows) so the browser prompt only
            // appears the first time.
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = TokenCacheName }
        };

        // Optional overrides for tenants that block the default developer client:
        // point at your own app registration without touching code. Both ignored if unset.
        var tenantId = Environment.GetEnvironmentVariable("VAULTVIEWER_TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("VAULTVIEWER_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(tenantId)) options.TenantId = tenantId;
        if (!string.IsNullOrWhiteSpace(clientId)) options.ClientId = clientId;

        return new InteractiveBrowserCredential(options);
    }
}
