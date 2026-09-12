using VaultViewer;
using Xunit;

namespace VaultViewer.Tests;

public class SearchViewStateTests
{
    [Fact]
    public void Skeleton_shows_while_searching_with_no_results_yet()
    {
        Assert.True(SearchViewState.ShowSkeleton(isSearching: true, resultCount: 0));
    }

    [Fact]
    public void Skeleton_hides_once_a_result_arrives()
    {
        Assert.False(SearchViewState.ShowSkeleton(isSearching: true, resultCount: 1));
    }

    [Fact]
    public void Skeleton_hidden_when_not_searching()
    {
        Assert.False(SearchViewState.ShowSkeleton(isSearching: false, resultCount: 0));
    }

    [Fact]
    public void NoResults_shows_after_a_finished_search_with_zero_matches()
    {
        Assert.True(SearchViewState.ShowNoResults(hasSearched: true, isSearching: false, resultCount: 0));
    }

    [Fact]
    public void NoResults_hidden_before_the_first_search()
    {
        Assert.False(SearchViewState.ShowNoResults(hasSearched: false, isSearching: false, resultCount: 0));
    }

    [Fact]
    public void NoResults_hidden_while_still_searching()
    {
        Assert.False(SearchViewState.ShowNoResults(hasSearched: true, isSearching: true, resultCount: 0));
    }

    [Fact]
    public void NoResults_hidden_when_there_are_matches()
    {
        Assert.False(SearchViewState.ShowNoResults(hasSearched: true, isSearching: false, resultCount: 3));
    }
}
