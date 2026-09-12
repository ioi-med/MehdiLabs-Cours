using Markdig;

namespace MehdiLabs.Cours.Services;

/// <summary>
/// Service de parsing Markdown via Markdig.
/// Remplace le parser custom Python (281 lignes) par une bibliothèque professionnelle.
/// </summary>
public static class MarkdownService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .UseTaskLists()
        .Build();

    /// <summary>
    /// Convertit du Markdown en HTML avec un style complet.
    /// </summary>
    public static string ToHtml(string markdown, string theme = "dark")
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        var htmlBody = Markdig.Markdown.ToHtml(markdown, Pipeline);

        var colors = theme switch
        {
            "light" => new ThemeColors("#F5F5F7", "#1D1D1F", "#0A84FF", "#FFFFFF", "#D1D1D6", "#6E6E73"),
            "sepia" => new ThemeColors("#F4ECD8", "#433422", "#D97D54", "#FAF5E9", "#D5C9B3", "#7D6B56"),
            "hacker" => new ThemeColors("#0D1117", "#00FF00", "#00FF00", "#161B22", "#30363D", "#008800"),
            _ => new ThemeColors("#1E1E1E", "#FFFFFF", "#5E9DFF", "#333333", "#3C3C3C", "#858585")
        };

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            font-size: 16px;
            line-height: 1.7;
            color: {colors.Fg};
            background-color: {colors.Bg};
            padding: 30px 40px;
            margin: 0;
        }}
        h1, h2, h3, h4, h5, h6 {{ color: {colors.Accent}; margin-top: 24px; }}
        h1 {{ font-size: 28px; border-bottom: 1px solid {colors.Border}; padding-bottom: 8px; }}
        h2 {{ font-size: 22px; border-bottom: 1px solid {colors.Border}; padding-bottom: 6px; }}
        h3 {{ font-size: 18px; }}
        a {{ color: #5AC8FA; text-decoration: none; }}
        a:hover {{ text-decoration: underline; }}
        code {{
            background: {colors.CodeBg};
            padding: 2px 8px;
            border-radius: 4px;
            font-family: 'Cascadia Code', 'Fira Code', Consolas, monospace;
            font-size: 14px;
        }}
        pre {{
            background: {colors.CodeBg};
            padding: 16px;
            border-radius: 8px;
            border: 1px solid {colors.Border};
            overflow-x: auto;
            margin: 16px 0;
        }}
        pre code {{ background: none; padding: 0; }}
        blockquote {{
            border-left: 4px solid {colors.Accent};
            padding-left: 16px;
            color: {colors.Muted};
            font-style: italic;
            margin: 16px 0;
        }}
        table {{ border-collapse: collapse; width: 100%; margin: 16px 0; }}
        th, td {{ border: 1px solid {colors.Border}; padding: 8px 12px; text-align: left; }}
        th {{ background: {colors.CodeBg}; color: {colors.Accent}; font-weight: bold; }}
        img {{ max-width: 100%; border-radius: 8px; margin: 12px 0; }}
        hr {{ border: none; border-top: 1px solid {colors.Border}; margin: 24px 0; }}
        ul, ol {{ padding-left: 24px; }}
        li {{ margin: 4px 0; }}
        input[type='checkbox'] {{ margin-right: 8px; }}
    </style>
</head>
<body>
{htmlBody}
</body>
</html>";
    }

    /// <summary>
    /// Convertit du Markdown en HTML brut (sans wrapper).
    /// </summary>
    public static string ToRawHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml(markdown, Pipeline);
    }

    private record ThemeColors(string Bg, string Fg, string Accent, string CodeBg, string Border, string Muted);
}
