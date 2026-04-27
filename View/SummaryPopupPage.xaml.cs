using Microsoft.Maui.Controls;

namespace E_Raamatud.View
{
    public partial class SummaryPopupPage : ContentPage
    {
        public SummaryPopupPage(string title, string markdownSummary)
        {
            InitializeComponent();
            TitleLabel.Text = title;
            SummaryWebView.Source = new HtmlWebViewSource
            {
                Html = MarkdownToHtml(markdownSummary)
            };
        }

        private async void OnCloseTapped(object sender, EventArgs e)
            => await Navigation.PopModalAsync();

        private async void OnBackgroundTapped(object sender, EventArgs e)
            => await Navigation.PopModalAsync();

        /// <summary>
        /// Converts basic markdown (headers, bold, line breaks) to styled HTML.
        /// </summary>
        private static string MarkdownToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return "<p></p>";

            var lines = markdown.Split('\n');
            var sb = new System.Text.StringBuilder();

            sb.Append(@"<!DOCTYPE html>
            <html>
            <head>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1'>
            <style>
              body {
                font-family: -apple-system, 'Segoe UI', sans-serif;
                font-size: 15px;
                line-height: 1.7;
                color: #e0e0e0;
                background-color: #1e1e1e;
                margin: 0;
                padding: 18px 22px 22px 22px;
              }
              h1 { font-size: 20px; color: #ffffff; margin: 18px 0 8px 0; }
              h2 { font-size: 17px; color: #cccccc; margin: 16px 0 6px 0; border-bottom: 1px solid #333; padding-bottom: 4px; }
              h3 { font-size: 15px; color: #bbbbbb; margin: 14px 0 4px 0; }
              strong { color: #ffffff; }
              p { margin: 0 0 10px 0; }
              ul { padding-left: 20px; margin: 4px 0 10px 0; }
              li { margin-bottom: 4px; }
            </style>
            </head>
            <body>");

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();

                if (line.StartsWith("### "))
                    sb.Append($"<h3>{InlineFormat(line.Substring(4))}</h3>");
                else if (line.StartsWith("## "))
                    sb.Append($"<h2>{InlineFormat(line.Substring(3))}</h2>");
                else if (line.StartsWith("# "))
                    sb.Append($"<h1>{InlineFormat(line.Substring(2))}</h1>");
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                    sb.Append($"<ul><li>{InlineFormat(line.Substring(2))}</li></ul>");
                else if (string.IsNullOrWhiteSpace(line))
                    sb.Append("<br>");
                else
                    sb.Append($"<p>{InlineFormat(line)}</p>");
            }

            sb.Append("</body></html>");
            return sb.ToString();
        }

        /// <summary>Converts **bold** inline markdown to HTML.</summary>
        private static string InlineFormat(string text)
        {
            // Bold: **text**
            var result = System.Text.RegularExpressions.Regex.Replace(
                text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            // Italic: *text*
            result = System.Text.RegularExpressions.Regex.Replace(
                result, @"\*(.+?)\*", "<em>$1</em>");
            return result;
        }
    }
}
