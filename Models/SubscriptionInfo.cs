namespace VaultViewer.Models;

/// <summary>
/// An Azure subscription the signed-in identity can see.
/// </summary>
public sealed class SubscriptionInfo
{
    public required string SubscriptionId { get; init; }

    public required string DisplayName { get; init; }

    /// <summary>Vaults discovered in this subscription. Populated during discovery.</summary>
    public List<VaultInfo> Vaults { get; } = new();

    public override string ToString() => DisplayName;
}
