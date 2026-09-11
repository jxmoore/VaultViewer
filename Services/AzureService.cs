using System.Collections.Concurrent;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using VaultViewer.Models;

namespace VaultViewer.Services;

/// <summary>
/// Progress payload for a global secret scan.
/// </summary>
public readonly record struct ScanProgress(int VaultsScanned, int VaultsTotal, string CurrentVault);

/// <summary>
/// Progress payload for subscription/vault discovery. <see cref="Total"/> is 0 until the
/// subscription list has been enumerated (the "listing subscriptions" phase).
/// </summary>
public readonly record struct DiscoveryProgress(int Completed, int Total, string Message);

/// <summary>
/// Wraps Azure Resource Manager (control plane) and Key Vault (data plane) access.
/// One credential is shared across every call so the user authenticates once.
/// </summary>
public sealed class AzureService : IAzureService
{
    // How many vaults we hit in parallel during a global search. Kept modest to
    // avoid throttling and to keep the UI responsive.
    private const int MaxConcurrency = 8;

    private readonly TokenCredential _credential;
    private readonly ArmClient _arm;

    // Cache one SecretClient per vault so repeated view/copy/search calls reuse it.
    private readonly ConcurrentDictionary<Uri, SecretClient> _secretClients = new();

    public AzureService(ICredentialFactory credentialFactory)
    {
        _credential = credentialFactory.Create();
        _arm = new ArmClient(_credential);
    }

    /// <summary>
    /// Enumerate every subscription the identity can see, and every Key Vault within.
    /// Vault listing runs per-subscription; failures on one subscription don't abort the rest.
    /// </summary>
    public async Task<IReadOnlyList<SubscriptionInfo>> DiscoverAsync(
        IProgress<DiscoveryProgress>? progress,
        CancellationToken ct)
    {
        // Phase 1: enumerate the subscription list so we know the total up front
        // (the count isn't known until this completes).
        progress?.Report(new DiscoveryProgress(0, 0, "Listing subscriptions…"));

        var subscriptions = new List<SubscriptionResource>();
        await foreach (SubscriptionResource sub in _arm.GetSubscriptions().GetAllAsync(ct))
        {
            ct.ThrowIfCancellationRequested();
            subscriptions.Add(sub);
        }

        // Phase 2: walk each subscription's vaults, reporting "x of N".
        var total = subscriptions.Count;
        var result = new List<SubscriptionInfo>(total);
        var index = 0;

        foreach (SubscriptionResource sub in subscriptions)
        {
            ct.ThrowIfCancellationRequested();
            index++;

            var subId = sub.Data.SubscriptionId ?? sub.Data.Id?.SubscriptionId ?? "unknown";
            var subName = string.IsNullOrWhiteSpace(sub.Data.DisplayName) ? subId : sub.Data.DisplayName;
            progress?.Report(new DiscoveryProgress(index, total, $"Scanning subscription \"{subName}\" ({index}/{total})…"));

            var info = new SubscriptionInfo { SubscriptionId = subId, DisplayName = subName };

            try
            {
                await foreach (KeyVaultResource vault in sub.GetKeyVaultsAsync(cancellationToken: ct))
                {
                    ct.ThrowIfCancellationRequested();

                    var uri = ResolveVaultUri(vault);
                    if (uri is null)
                        continue;

                    info.Vaults.Add(new VaultInfo
                    {
                        Name = vault.Data.Name,
                        VaultUri = uri,
                        SubscriptionId = subId,
                        SubscriptionName = subName,
                        ResourceGroup = vault.Id.ResourceGroupName ?? "(unknown)",
                        Location = vault.Data.Location.ToString()
                    });
                }
            }
            catch (Exception ex) when (ex is RequestFailedException or AuthenticationFailedException)
            {
                // No access to list vaults in this subscription — record it and move on.
                progress?.Report(new DiscoveryProgress(index, total, $"Skipped \"{subName}\": {ex.Message}"));
            }

            result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Scan the given vaults for secrets whose names contain <paramref name="query"/>
    /// (case-insensitive). Matches are pushed to <paramref name="onMatch"/> as they're
    /// found so the UI can populate incrementally.
    /// </summary>
    public async Task SearchSecretsAsync(
        string query,
        IReadOnlyList<VaultInfo> vaults,
        IProgress<ScanProgress>? progress,
        Action<SecretMatch> onMatch,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        var scanned = 0;
        var total = vaults.Count;
        using var gate = new SemaphoreSlim(MaxConcurrency);

        var tasks = vaults.Select(async vault =>
        {
            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();
                var client = GetSecretClient(vault.VaultUri);

                await foreach (SecretProperties props in client.GetPropertiesOfSecretsAsync(ct).ConfigureAwait(false))
                {
                    if (props.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        onMatch(new SecretMatch
                        {
                            SecretName = props.Name,
                            VaultName = vault.Name,
                            VaultUri = vault.VaultUri,
                            SubscriptionName = vault.SubscriptionName,
                            SubscriptionId = vault.SubscriptionId,
                            ResourceGroup = vault.ResourceGroup,
                            Enabled = props.Enabled ?? true
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is RequestFailedException or AuthenticationFailedException)
            {
                // No list permission on this vault — silently skip; it's expected across many vaults.
                _ = ex;
            }
            finally
            {
                var done = Interlocked.Increment(ref scanned);
                progress?.Report(new ScanProgress(done, total, vault.Name));
                gate.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>Fetch the current value of a single secret.</summary>
    public async Task<string> GetSecretValueAsync(Uri vaultUri, string secretName, CancellationToken ct)
    {
        var client = GetSecretClient(vaultUri);
        KeyVaultSecret secret = await client.GetSecretAsync(secretName, cancellationToken: ct).ConfigureAwait(false);
        return secret.Value;
    }

    private SecretClient GetSecretClient(Uri vaultUri) =>
        _secretClients.GetOrAdd(vaultUri, uri => new SecretClient(uri, _credential));

    private static Uri? ResolveVaultUri(KeyVaultResource vault)
    {
        var uri = vault.Data.Properties?.VaultUri;
        if (uri is not null)
            return uri;

        // Fall back to the conventional public-cloud data-plane host if the ARM
        // payload didn't include the URI for some reason.
        return Uri.TryCreate($"https://{vault.Data.Name}.vault.azure.net/", UriKind.Absolute, out var built)
            ? built
            : null;
    }
}
