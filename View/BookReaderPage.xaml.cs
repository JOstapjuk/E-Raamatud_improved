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

        private void LoadBook(string htmlContent)
        {
            var pagedHtml = BuildHtml(htmlContent, _fontSize, _isDarkMode);
            BookWebView.Source = new HtmlWebViewSource { Html = pagedHtml };
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
                    _currentPage = Math.Min(progress.CurrentPage, _totalPages - 1);
                    await BookWebView.EvaluateJavaScriptAsync($"goToPage({_currentPage})");
                }
            }

            MainThread.BeginInvokeOnMainThread(() => UpdatePageLabel());
        }

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
                    width: 100%; height: 100%;
                    overflow: hidden;
                    background-color: {bgColor};
                }}
                #hidden-content {{
                    position: absolute;
                    left: -9999px;
                    top: 0;
                    width: calc(100% - 40px);
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
                p  {{ margin-bottom: 16px; text-align: justify; }}
                h1 {{ color: {headingColor}; font-size: {fontSize + 8}px; margin: 24px 0 12px 0; font-family: Georgia, serif; }}
                h2 {{ color: {headingColor}; font-size: {fontSize + 4}px; margin: 20px 0 10px 0; font-family: Georgia, serif; }}
                h3 {{ color: {headingColor}; font-size: {fontSize + 2}px; margin: 16px 0 8px  0; font-family: Georgia, serif; }}
                hr {{ border: none; border-top: 1px solid #ccc; margin: 20px 0; }}
                img, image, svg {{ display: none !important; }}
            </style>
            </head>
            <body>
                <div id='hidden-content'>{htmlContent}</div>
                <div id='page-display'><p style='color:{textColor}'>Laadin...</p></div>
                <script>
                    var pages = [];
                    var currentPage = 0;

                    function initPaging() {{
                        var hidden  = document.getElementById('hidden-content');
                        var display = document.getElementById('page-display');
                        var pageHeight = window.innerHeight;

                        var elements = hidden.querySelectorAll('p, h1, h2, h3, h4, h5, h6, hr');
                        pages = [];
                        var currentHtml = '';

                        // Fallback: if no block elements found, use full innerHTML
                        if (elements.length === 0) {{
                            pages.push(hidden.innerHTML);
                            showPage(0);
                            return 1;
                        }}

                        var tempDiv = document.createElement('div');
                        tempDiv.style.position   = 'absolute';
                        tempDiv.style.left       = '-9999px';
                        tempDiv.style.top        = '0';
                        tempDiv.style.width      = display.offsetWidth + 'px';
                        tempDiv.style.padding    = '20px';
                        tempDiv.style.fontSize   = '{fontSize}px';
                        tempDiv.style.lineHeight = '1.8';
                        tempDiv.style.fontFamily = 'Georgia, serif';
                        document.body.appendChild(tempDiv);

                        for (var i = 0; i < elements.length; i++) {{
                            var elHtml   = elements[i].outerHTML;
                            var testHtml = currentHtml + elHtml;
                            tempDiv.innerHTML = testHtml;

                            if (tempDiv.scrollHeight > pageHeight && currentHtml !== '') {{
                                pages.push(currentHtml);
                                currentHtml = elHtml;
                            }} else {{
                                currentHtml = testHtml;
                            }}
                        }}
                        if (currentHtml) pages.push(currentHtml);
                        document.body.removeChild(tempDiv);

                        if (pages.length === 0) pages.push(hidden.innerHTML);

                        showPage(0);
                        return pages.length;
                    }}

                    function showPage(index) {{
                        if (index < 0 || index >= pages.length) return;
                        currentPage = index;
                        document.getElementById('page-display').innerHTML = pages[index];
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

        private async void OnFontDecrease(object sender, EventArgs e)
        {
            if (_fontSize <= MinFontSize) return;
            _fontSize -= 2;
            MainThread.BeginInvokeOnMainThread(() => FontSizeLabel.Text = $"{_fontSize}px");
            await ReloadWithSettings();
        }

        private async void OnFontIncrease(object sender, EventArgs e)
        {
            if (_fontSize >= MaxFontSize) return;
            _fontSize += 2;
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