namespace VaultViewer;

/// <summary>
/// Decides a vault's data-plane URI: prefer the URI from the ARM payload, otherwise
/// fall back to the conventional public-cloud host built from the vault name. Pure so
/// it can be unit tested without an Azure SDK resource object.
/// </summary>
public static class VaultUriResolver
{
    public static Uri? Resolve(Uri? vaultUri, string? name)
    {
        if (vaultUri is not null)
            return vaultUri;

        if (string.IsNullOrWhiteSpace(name))
            return null;

        return Uri.TryCreate($"https://{name}.vault.azure.net/", UriKind.Absolute, out var built)
            ? built
            : null;
    }
}
