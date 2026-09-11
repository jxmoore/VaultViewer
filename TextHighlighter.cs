using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace VaultViewer;

/// <summary>
/// Attached behavior that fills a <see cref="TextBlock"/> with runs, highlighting the
/// parts of <see cref="SourceTextProperty"/> that match <see cref="QueryProperty"/>.
/// </summary>
public static class TextHighlighter
{
    private static readonly Brush HighlightBackground =
        new SolidColorBrush(Color.FromArgb(0x66, 0x63, 0x66, 0xF1)); // translucent accent
    private static readonly Brush HighlightForeground = Brushes.White;

    static TextHighlighter()
    {
        if (HighlightBackground.CanFreeze) HighlightBackground.Freeze();
    }

    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.RegisterAttached("SourceText", typeof(string), typeof(TextHighlighter),
            new PropertyMetadata(string.Empty, OnChanged));

    public static readonly DependencyProperty QueryProperty =
        DependencyProperty.RegisterAttached("Query", typeof(string), typeof(TextHighlighter),
            new PropertyMetadata(string.Empty, OnChanged));

    public static string GetSourceText(DependencyObject o) => (string)o.GetValue(SourceTextProperty);
    public static void SetSourceText(DependencyObject o, string v) => o.SetValue(SourceTextProperty, v);
    public static string GetQuery(DependencyObject o) => (string)o.GetValue(QueryProperty);
    public static void SetQuery(DependencyObject o, string v) => o.SetValue(QueryProperty, v);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb)
            return;

        tb.Inlines.Clear();
        foreach (var seg in Highlighting.Split(GetSourceText(tb), GetQuery(tb)))
        {
            var run = new Run(seg.Text);
            if (seg.IsMatch)
            {
                run.Background = HighlightBackground;
                run.Foreground = HighlightForeground;
                run.FontWeight = FontWeights.Bold;
            }
            tb.Inlines.Add(run);
        }
    }
}
