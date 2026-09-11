namespace VaultViewer.Models;

/// <summary>
/// One secret whose name matched a global search, plus provenance so the user
/// can tell identically-named secrets in different vaults apart.
/// </summary>
public sealed class SecretMatch
{
    public required string SecretName { get; init; }

    public required string VaultName { get; init; }

    public required Uri VaultUri { get; init; }

    public required string SubscriptionName { get; init; }

    public required string SubscriptionId { get; init; }

    public required string ResourceGroup { get; init; }

    public bool Enabled { get; init; } = true;
}
