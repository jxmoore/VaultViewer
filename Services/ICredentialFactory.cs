using Azure.Core;

namespace VaultViewer.Services;

/// <summary>Builds the token credential the app uses to talk to Azure.</summary>
public interface ICredentialFactory
{
    TokenCredential Create();
}
