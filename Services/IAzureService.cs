using VaultViewer.Models;

namespace VaultViewer.Services;

/// <summary>Azure control-plane discovery and Key Vault data-plane access.</summary>
public interface IAzureService
{
    /// <summary>Enumerate every accessible subscription and its Key Vaults.</summary>
    Task<IReadOnlyList<SubscriptionInfo>> DiscoverAsync(IProgress<DiscoveryProgress>? progress, CancellationToken ct);

    /// <summary>Scan the given vaults for secrets whose name contains <paramref name="query"/>.</summary>
    Task SearchSecretsAsync(
        string query,
        IReadOnlyList<VaultInfo> vaults,
        IProgress<ScanProgress>? progress,
        Action<SecretMatch> onMatch,
        CancellationToken ct);

    /// <summary>Fetch the current value of a single secret.</summary>
    Task<string> GetSecretValueAsync(Uri vaultUri, string secretName, CancellationToken ct);
}
