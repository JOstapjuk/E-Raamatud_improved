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

        public BookReaderPage(int raamatId, string title, string htmlContent)
        {
            InitializeComponent();
            Title = title;
            _raamatId = raamatId;
            _htmlContent = htmlContent;
            LoadBook(htmlContent);
            _ = InitializeAfterLoad();
        }

        private void LoadBook(string htmlContent)
        {
            var pagedHtml = BuildHtml(htmlContent, _fontSize, _isDarkMode);
            BookWebView.Source = new HtmlWebViewSource { Html = pagedHtml };
        }

        private string BuildHtml(string htmlContent, int fontSize, bool darkMode)
        {
            var bgColor = darkMode ? "#1a1a1a" : "#f5f0e8";
            var textColor = darkMode ? "#e0e0e0" : "#1a1a1a";
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

                function initPaging() {{
                    var hidden = document.getElementById('hidden-content');
                    var display = document.getElementById('page-display');
                    var pageHeight = window.innerHeight;

                    var nodes = Array.from(hidden.childNodes);
                    pages = [];
                    var currentPageHtml = '';
                    var tempDiv = document.createElement('div');
                    tempDiv.style.visibility = 'hidden';
                    tempDiv.style.position = 'absolute';
                    tempDiv.style.width = display.offsetWidth + 'px';
                    tempDiv.style.padding = '20px';
                    tempDiv.style.fontSize = '{fontSize}px';
                    tempDiv.style.lineHeight = '1.8';
                    tempDiv.style.fontFamily = 'Georgia, serif';
                    document.body.appendChild(tempDiv);

                    for (var i = 0; i < nodes.length; i++) {{
                        var node = nodes[i];
                        var nodeHtml = node.outerHTML || node.textContent;
                        if (!nodeHtml || nodeHtml.trim() === '') continue;

                        tempDiv.innerHTML = currentPageHtml + nodeHtml;
                        if (tempDiv.scrollHeight > pageHeight && currentPageHtml !== '') {{
                            pages.push(currentPageHtml);
                            currentPageHtml = nodeHtml;
                        }} else {{
                            currentPageHtml += nodeHtml;
                        }}
                    }}
                    if (currentPageHtml) pages.push(currentPageHtml);
                    document.body.removeChild(tempDiv);
                    if (pages.length === 0) pages.push(hidden.innerHTML);

                    showPage(0);
                    return pages.length;
                }}

                function showPage(index) {{
                    var display = document.getElementById('page-display');
                    display.innerHTML = pages[index];
                    currentPage = index;
                }}

                function goToPage(index) {{
                    if (index >= 0 && index < pages.length) {{
                        showPage(index);
                        currentPage = index;
                    }}
                }}

                function getTotalPages() {{ return pages.length; }}
                function getCurrentPage() {{ return currentPage; }}
            </script>
            </body>
            </html>";
        }

        private async Task InitializeAfterLoad()
        {
            await Task.Delay(2000);

            var totalResult = await BookWebView.EvaluateJavaScriptAsync("initPaging()");
            if (int.TryParse(totalResult, out int total))
                _totalPages = Math.Max(1, total);

            int userId = SessionService.CurrentUser?.Id ?? 0;
            if (userId > 0)
            {
                var progress = await DatabaseService.Instance.GetReadingProgressAsync(userId, _raamatId);
                if (progress != null && progress.CurrentPage > 0)
                {
                    _currentPage = progress.CurrentPage;
                    await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                }
            }

            MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
        }

        // Re-renders the WebView with new font/theme, restoring current page
        private async Task ReloadWithSettings()
        {
            double proportion = _totalPages > 1 ? (double)_currentPage / (_totalPages - 1) : 0;

            BookWebView.Source = new HtmlWebViewSource
            {
                Html = BuildHtml(_htmlContent, _fontSize, _isDarkMode)
            };

            await Task.Delay(2000);

            var totalResult = await BookWebView.EvaluateJavaScriptAsync("initPaging()");
            if (int.TryParse(totalResult, out int total))
                _totalPages = Math.Max(1, total);

            _currentPage = (int)Math.Round(proportion * (_totalPages - 1));
            _currentPage = Math.Max(0, Math.Min(_currentPage, _totalPages - 1));

            await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
            MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
        }

        private async void OnFontIncrease(object sender, EventArgs e)
        {
            if (_fontSize >= MaxFontSize) return;
            _fontSize += 2;
            MainThread.BeginInvokeOnMainThread(() => FontSizeLabel.Text = $"{_fontSize}px");
            await ReloadWithSettings();
        }

        private async void OnFontDecrease(object sender, EventArgs e)
        {
            if (_fontSize <= MinFontSize) return;
            _fontSize -= 2;
            MainThread.BeginInvokeOnMainThread(() => FontSizeLabel.Text = $"{_fontSize}px");
            await ReloadWithSettings();
        }

        private async void OnThemeToggle(object sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            MainThread.BeginInvokeOnMainThread(() =>
                ThemeButton.Text = _isDarkMode ? "☀️" : "🌙");
            await ReloadWithSettings();
        }

        private async void OnNextPage(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
                await SaveProgress();
            }
        }

        private async void OnPreviousPage(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
                await SaveProgress();
            }
        }

        private async void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
                await SaveProgress();
            }
        }

        private async void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
                await SaveProgress();
            }
        }

        private void UpdatePageLabel()
        {
            PageLabel.Text = $"{_currentPage + 1} / {_totalPages}";
        }

        private async Task SaveProgress()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId <= 0) return;

                var existing = await DatabaseService.Instance.GetReadingProgressAsync(userId, _raamatId);
                if (existing != null)
                {
                    existing.CurrentPage = _currentPage;
                    existing.TotalPages = _totalPages;
                    await DatabaseService.Instance.UpdateReadingProgressAsync(existing);
                }
                else
                {
                    await DatabaseService.Instance.InsertReadingProgressAsync(new ReadingProgress
                    {
                        Kasutaja_ID = userId,
                        Raamat_ID = _raamatId,
                        CurrentPage = _currentPage,
                        TotalPages = _totalPages
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveProgress error: {ex.Message}");
            }
        }
    }
}