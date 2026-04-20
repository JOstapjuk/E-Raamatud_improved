using E_Raamatud.Model;
using E_Raamatud.Services;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace E_Raamatud.View
{
    public partial class BookReaderPage : ContentPage
    {
        private int _currentPage = 0;
        private int _totalPages = 1;
        private int _raamatId;
        private int _fontSize = 18;
        private bool _isDarkMode = false;
        private const int MinFontSize = 12;
        private const int MaxFontSize = 28;

        private string _htmlContent;
        private string _bookTitle;
        private string _bookDescription;

        // Pass description + title so the summarizer has full context
        public BookReaderPage(int raamatId, string title, string htmlContent, string description = "")
        {
            InitializeComponent();
            Title = title;
            _raamatId = raamatId;
            _htmlContent = htmlContent;
            _bookTitle = title;
            _bookDescription = description;
            LoadBook(htmlContent);
            _ = InitializeAfterLoad();
        }

        // Summarize button handler
        private async void OnSummarize(object sender, EventArgs e)
        {
            SummarizeButton.IsEnabled = false;
            SummarizeButton.Text = "⏳";

            try
            {
                var service = new SummarizationService();

                // publishYear = 0 → service will use text-based strategy
                string summary = await service.SummarizeAsync(
                    title: _bookTitle,
                    description: _bookDescription,
                    rawHtml: _htmlContent,
                    publishYear: 0);

                await DisplayAlert($"📋 {_bookTitle}", summary, "Sulge");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Summarize error: {ex.Message}");
                await DisplayAlert("Viga", "Kokkuvõtte loomine ebaõnnestus. Kontrolli API võtit ja internetiühendust.", "OK");
            }
            finally
            {
                SummarizeButton.IsEnabled = true;
                SummarizeButton.Text = "📋";
            }
        }

        // Existing methods below (unchanged)

        private void LoadBook(string htmlContent)
        {
            var pagedHtml = BuildHtml(htmlContent, _fontSize, _isDarkMode);
            BookWebView.Source = new HtmlWebViewSource { Html = pagedHtml };
        }

        private string BuildHtml(string htmlContent, int fontSize, bool darkMode)
        {
            var bgColor = darkMode ? "#1a1a1a" : "#f5f0e8";
            var textColor  = darkMode ? "#e0e0e0" : "#1a1a1a";
            var headingColor = darkMode ? "#cccccc" : "#2c2c2c";

            return $@"
            <html>
            <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
            <style>
                * {{ box-sizing: border-box; margin: 0; padding: 0; }}
                html, body {{
                    width: 100%;
                    height: 100%;
                    overflow: hidden;
                    background-color: {bgColor};
                }}
                #hidden-content {{
                    position: absolute;
                    visibility: hidden;
                    width: 100%;
                    padding: 20px;
                    font-family: Georgia, serif;
                    font-size: {fontSize}px;
                    line-height: 1.8;
                    color: {textColor};
                }}
                #page-display {{
                    width: 100%;
                    height: 100vh;
                    padding: 20px;
                    font-family: Georgia, serif;
                    font-size: {fontSize}px;
                    line-height: 1.8;
                    color: {textColor};
                    background-color: {bgColor};
                    overflow: hidden;
                }}
                p {{ margin-bottom: 16px; text-align: justify; }}
                h1, h2, h3 {{ color: {headingColor}; margin: 20px 0 10px 0; }}
                hr {{ border: none; border-top: 1px solid #ccc; margin: 20px 0; }}
                img, image, svg {{ display: none !important; }}
            </style>
            </head>
            <body>
                <div id='hidden-content'>{htmlContent}</div>
                <div id='page-display'></div>
                <script>
                    var pages = [];
                    var currentPage = 0;

                    function buildPages() {{
                        var hidden = document.getElementById('hidden-content');
                        var display = document.getElementById('page-display');
                        var maxHeight = window.innerHeight;
                        var words = hidden.innerText.split(/\s+/);
                        pages = [];
                        var current = '';
                        display.innerHTML = '';

                        for (var i = 0; i < words.length; i++) {{
                            var test = current + (current ? ' ' : '') + words[i];
                            display.innerHTML = '<p>' + test + '</p>';
                            if (display.scrollHeight > maxHeight && current) {{
                                pages.push(current);
                                current = words[i];
                            }} else {{
                                current = test;
                            }}
                        }}
                        if (current) pages.push(current);
                        showPage(0);
                        dotnet.invokeMethodAsync('E_Raamatud', 'SetTotalPages', pages.length);
                    }}

                    function showPage(index) {{
                        var display = document.getElementById('page-display');
                        if (index >= 0 && index < pages.length) {{
                            currentPage = index;
                            display.innerHTML = '<p>' + pages[index] + '</p>';
                        }}
                    }}

                    window.onload = buildPages;
                    window.onresize = buildPages;
                </script>
            </body>
            </html>";
        }

        private async Task InitializeAfterLoad()
        {
            await Task.Delay(500);
        }

        private void OnFontDecrease(object sender, EventArgs e)
        {
            if (_fontSize > MinFontSize)
            {
                _fontSize--;
                FontSizeLabel.Text = $"{_fontSize}px";
                LoadBook(_htmlContent);
            }
        }

        private void OnFontIncrease(object sender, EventArgs e)
        {
            if (_fontSize < MaxFontSize)
            {
                _fontSize++;
                FontSizeLabel.Text = $"{_fontSize}px";
                LoadBook(_htmlContent);
            }
        }

        private void OnThemeToggle(object sender, EventArgs e)
        {
            _isDarkMode       = !_isDarkMode;
            ThemeButton.Text  = _isDarkMode ? "☀️" : "🌙";
            LoadBook(_htmlContent);
        }

        private void OnPreviousPage(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                UpdatePageLabel();
            }
        }

        private void OnNextPage(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                UpdatePageLabel();
            }
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)  => OnNextPage(sender, e);
        private void OnSwipeRight(object sender, SwipedEventArgs e) => OnPreviousPage(sender, e);

        private void UpdatePageLabel()
        {
            PageLabel.Text = $"{_currentPage + 1} / {_totalPages}";
        }
    }
}