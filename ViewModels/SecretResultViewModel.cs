using System.Windows;
using VaultViewer.Models;
using VaultViewer.Services;

namespace VaultViewer.ViewModels;

/// <summary>
/// A single global-search result row: the matched secret plus lazy view/copy of its value.
/// The value is only ever fetched on demand (View or Copy), never during the scan.
/// </summary>
public sealed class SecretResultViewModel : ViewModelBase
{
    private readonly IAzureService _azure;
    private readonly Action<string> _setStatus;

    private string? _revealedValue;
    private bool _isRevealed;
    private bool _isBusy;

    public SecretResultViewModel(SecretMatch match, IAzureService azure, Action<string> setStatus, string query)
    {
        Match = match;
        _azure = azure;
        _setStatus = setStatus;
        Query = query;
        ViewCommand = new AsyncRelayCommand(_ => ToggleRevealAsync());
        CopyCommand = new AsyncRelayCommand(_ => CopyAsync());
    }

    public SecretMatch Match { get; }

    /// <summary>The search term that produced this match, used to highlight the secret name.</summary>
    public string Query { get; }

    public string SecretName => Match.SecretName;
    public string VaultName => Match.VaultName;
    public string SubscriptionName => Match.SubscriptionName;
    public string ResourceGroup => Match.ResourceGroup;
    public bool Enabled => Match.Enabled;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetField(ref _isBusy, value);
    }

    public bool IsRevealed
    {
        get => _isRevealed;
        private set
        {
            if (SetField(ref _isRevealed, value))
                OnPropertyChanged(nameof(DisplayValue));
        }
    }

    /// <summary>Masked until the user reveals it.</summary>
    public string DisplayValue => IsRevealed ? _revealedValue ?? string.Empty : "••••••••";

    public AsyncRelayCommand ViewCommand { get; }
    public AsyncRelayCommand CopyCommand { get; }

    private async Task ToggleRevealAsync()
    {
        if (IsRevealed)
        {
            IsRevealed = false;
            return;
        }

        try
        {
            IsBusy = true;
            _revealedValue ??= await _azure.GetSecretValueAsync(Match.VaultUri, Match.SecretName, CancellationToken.None);
            IsRevealed = true;
        }
        catch (Exception ex)
        {
            _setStatus($"Could not read \"{Match.SecretName}\": {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CopyAsync()
    {
        try
        {
            IsBusy = true;
            _revealedValue ??= await _azure.GetSecretValueAsync(Match.VaultUri, Match.SecretName, CancellationToken.None);
            Clipboard.SetText(_revealedValue);
            _setStatus($"Copied \"{Match.SecretName}\" to clipboard.");
        }
        catch (Exception ex)
        {
            _setStatus($"Could not copy \"{Match.SecretName}\": {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
