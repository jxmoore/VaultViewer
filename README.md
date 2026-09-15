# VaultViewer

[![Tests](https://github.com/jxmoore/VaultViewer/actions/workflows/tests.yml/badge.svg?branch=develop)](https://github.com/jxmoore/VaultViewer/actions/workflows/tests.yml)

A Windows desktop app (.NET 8 / WPF) that shows every Azure Key Vault you can
access across all your subscriptions, and lets you search secret **names** globally.

## Features

- **Left pane** – a tabbed action bar:
  - **Vaults** tab: a flat, searchable list of every vault (filter by vault name,
    subscription, or resource group).
  - **Subscriptions** tab: vaults grouped under their subscription in a tree,
    with its own filter.
- **Right pane** – **global secret search**: type part of a secret name and the
  app scans every accessible vault in parallel. Each match shows the secret name,
  vault, subscription, and resource group, with **View** (reveal, masked by
  default) and **Copy** (to clipboard) actions. Secret *values* are only fetched
  on demand — never during the scan.

## Authentication

Credentials are built by `DefaultCredentialFactory` as a two-link chain:

1. **`DefaultAzureCredential`** — tries, in order: environment variables →
   workload identity → managed identity → Visual Studio → **Azure CLI** → Azure
   PowerShell → Azure Developer CLI.
2. **`InteractiveBrowserCredential`** (fallback) — if nothing above is available,
   a browser sign-in opens. The token is cached (DPAPI on Windows), so the prompt
   only appears the first time.

The simplest path on a dev box is still:

```bash
az login
```

…but with no CLI installed the app falls back to the browser sign-in automatically.
Click **Refresh** if the app started before you signed in.

If your tenant blocks the default developer client used by the browser sign-in,
point it at your own Azure AD app registration (client ID + an
`http://localhost` redirect URI) via environment variables — no code change:

```bash
setx VAULTVIEWER_TENANT_ID <tenant-guid>
setx VAULTVIEWER_CLIENT_ID <app-client-id>
```

Your identity needs:
- **Reader** (or higher) on subscriptions to list vaults.
- A data-plane role such as **Key Vault Secrets User** / **Reader**, or an
  access-policy grant, on each vault to list and read secrets. Vaults you can't
  read are skipped silently during search.

## Build & run

```bash
dotnet build
dotnet run
```

The built exe lands in `bin/Debug/net8.0-windows/VaultViewer.exe`.

## Project layout

| Path | Purpose |
|------|---------|
| `Models/` | `SubscriptionInfo`, `VaultInfo`, `SecretMatch` |
| `Services/IAzureService` + `AzureService.cs` | ARM + Key Vault access, concurrent search |
| `Services/ICredentialFactory` + `DefaultCredentialFactory.cs` | Builds the credential chain (incl. browser fallback) |
| `ViewModels/` | MVVM: `MainViewModel`, per-result and per-subscription VMs |
| `MainWindow.xaml` | Two-pane UI |
| `App.xaml` | Dark theme + styles |
| `tests/VaultViewer.Tests/` | xUnit tests |

Run the tests with `dotnet test`.
