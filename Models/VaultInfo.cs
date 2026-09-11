namespace VaultViewer.Models;

/// <summary>
/// A single Azure Key Vault, tagged with where it lives.
/// </summary>
public sealed class VaultInfo
{
    public required string Name { get; init; }

    /// <summary>The data-plane URI, e.g. https://myvault.vault.azure.net/</summary>
    public required Uri VaultUri { get; init; }

    public required string SubscriptionId { get; init; }

    public required string SubscriptionName { get; init; }

    public required string ResourceGroup { get; init; }

    public string? Location { get; init; }

    public override string ToString() => Name;
}
