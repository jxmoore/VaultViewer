namespace VaultViewer;

/// <summary>
/// Decides which of the three global-search result states to show: the loading
/// skeleton, the "no results" message, or the results themselves. Pure so it can
/// be unit tested without the view-model's Azure dependencies.
/// </summary>
public static class SearchViewState
{
    /// <summary>Show the skeleton while a search is running and no results have arrived yet.</summary>
    public static bool ShowSkeleton(bool isSearching, int resultCount) =>
        isSearching && resultCount == 0;

    /// <summary>Show the empty message once a search has finished and found nothing.</summary>
    public static bool ShowNoResults(bool hasSearched, bool isSearching, int resultCount) =>
        hasSearched && !isSearching && resultCount == 0;
}
