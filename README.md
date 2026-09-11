# VaultViewer

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

Uses `DefaultAzureCredential`, which tries, in order: environment variables →
workload identity → managed identity → Visual Studio → **Azure CLI** → Azure
PowerShell → Azure Developer CLI.

The simplest path on a dev box:

```bash
az login
```

Then launch the app and click **Refresh** if it started before you signed in.

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
| `Services/AzureService.cs` | ARM + Key Vault access, concurrent search |
| `ViewModels/` | MVVM: `MainViewModel`, per-result and per-subscription VMs |
| `MainWindow.xaml` | Two-pane UI |
| `App.xaml` | Dark theme + styles |
