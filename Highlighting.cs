namespace VaultViewer;

/// <summary>A run of text and whether it matched the search query.</summary>
public readonly record struct HighlightSegment(string Text, bool IsMatch);

/// <summary>
/// Splits text into matched / unmatched segments for highlighting. Pure and UI-free
/// so it can be unit tested; the WPF behavior turns the segments into styled runs.
/// </summary>
public static class Highlighting
{
    public static IReadOnlyList<HighlightSegment> Split(string? text, string? query)
    {
        text ??= string.Empty;
        var segments = new List<HighlightSegment>();

        if (string.IsNullOrEmpty(query))
        {
            if (text.Length > 0)
                segments.Add(new HighlightSegment(text, false));
            return segments;
        }

        var i = 0;
        while (i < text.Length)
        {
            var idx = text.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                segments.Add(new HighlightSegment(text[i..], false));
                break;
            }

            if (idx > i)
                segments.Add(new HighlightSegment(text[i..idx], false));

            segments.Add(new HighlightSegment(text.Substring(idx, query.Length), true));
            i = idx + query.Length;
        }

        return segments;
    }
}
