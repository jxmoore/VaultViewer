using VaultViewer;
using Xunit;

namespace VaultViewer.Tests;

public class HighlightingTests
{
    [Fact]
    public void Splits_around_a_single_match()
    {
        var segments = Highlighting.Split("Developer", "Dev");

        Assert.Collection(segments,
            s => { Assert.Equal("Dev", s.Text); Assert.True(s.IsMatch); },
            s => { Assert.Equal("eloper", s.Text); Assert.False(s.IsMatch); });
    }

    [Fact]
    public void Match_is_case_insensitive_but_preserves_original_casing()
    {
        var segments = Highlighting.Split("Developer", "dev");

        Assert.Equal("Dev", segments[0].Text); // original casing kept
        Assert.True(segments[0].IsMatch);
    }

    [Fact]
    public void Highlights_every_occurrence()
    {
        var segments = Highlighting.Split("dev-Dev-DEV", "dev");

        Assert.Equal(5, segments.Count);
        Assert.True(segments[0].IsMatch);
        Assert.Equal("-", segments[1].Text);
        Assert.True(segments[2].IsMatch);
        Assert.Equal("-", segments[3].Text);
        Assert.True(segments[4].IsMatch);
    }

    [Fact]
    public void No_match_returns_the_whole_string_unhighlighted()
    {
        var segments = Highlighting.Split("secret", "dev");

        Assert.Single(segments);
        Assert.Equal("secret", segments[0].Text);
        Assert.False(segments[0].IsMatch);
    }

    [Fact]
    public void Empty_query_returns_the_whole_string_unhighlighted()
    {
        var segments = Highlighting.Split("secret", "");

        Assert.Single(segments);
        Assert.False(segments[0].IsMatch);
    }
}
