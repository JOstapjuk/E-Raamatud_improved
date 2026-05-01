using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;
using System.Diagnostics;
using E_Raamatud.View;
using System.Net.Http;
using System.Linq;

namespace E_Raamatud;

public partial class BookDetailPage : ContentPage
{
    private readonly Raamat _book;

    public BookDetailPage(Raamat selectedBook, string zanrNimi)
    {
        InitializeComponent();

        if (selectedBook == null)
            throw new ArgumentNullException(nameof(selectedBook));

        _book = selectedBook;
        BindingContext = new BookDetailViewModel(selectedBook, zanrNimi);

        ListenButton.IsVisible = !string.IsNullOrWhiteSpace(selectedBook.Audiofail);

        AudioAvailableLabel.Text = !string.IsNullOrWhiteSpace(selectedBook.Audiofail)
            ? "Saadaval"
            : "Pole saadaval";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyResponsiveLayout(this.Width);
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        ApplyResponsiveLayout(this.Width);
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (ContentRoot == null || CoverBorder == null || InfoPanel == null) return;
        if (width <= 0) return;

        ContentRoot.RowDefinitions.Clear();
        ContentRoot.ColumnDefinitions.Clear();

        if (width >= 900)
        {
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(CoverBorder, 0); Grid.SetRow(CoverBorder, 0);
            Grid.SetColumn(InfoPanel, 1); Grid.SetRow(InfoPanel, 0);

            CoverBorder.WidthRequest = 320;
            CoverBorder.HeightRequest = 460;
            CoverBorder.HorizontalOptions = LayoutOptions.Start;

            SetInfoBlocksColumns(3);
        }
        else
        {
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CoverBorder, 0); Grid.SetRow(CoverBorder, 0);
            Grid.SetColumn(InfoPanel, 0); Grid.SetRow(InfoPanel, 1);

            double coverWidth = Math.Min(260, width - 80);
            CoverBorder.WidthRequest = coverWidth;
            CoverBorder.HeightRequest = coverWidth * 1.45;
            CoverBorder.HorizontalOptions = LayoutOptions.Center;

            SetInfoBlocksColumns(1);
        }
    }

    private void SetInfoBlocksColumns(int columns)
    {
        if (InfoBlocksGrid == null) return;
        if (InfoBlocksGrid.Children.Count < 3) return;

        InfoBlocksGrid.ColumnDefinitions.Clear();
        InfoBlocksGrid.RowDefinitions.Clear();

        for (int i = 0; i < columns; i++)
            InfoBlocksGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        int rows = (int)Math.Ceiling(3.0 / columns);
        for (int i = 0; i < rows; i++)
            InfoBlocksGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < InfoBlocksGrid.Children.Count && i < 3; i++)
        {
            var child = (Microsoft.Maui.Controls.View)InfoBlocksGrid.Children[i];
            Grid.SetColumn(child, i % columns);
            Grid.SetRow(child, i / columns);
        }
    }

    private async void OnReadTapped(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_book.Tekstifail))
        {
            await DisplayAlert(AppResources.Error, AppResources.NoTextFile, AppResources.OK);
            return;
        }

        try
        {
            Stream bookStream;
            var ext = System.IO.Path.GetExtension(_book.Tekstifail)?.ToLowerInvariant();

            if (_book.Tekstifail.StartsWith("http"))
            {
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(_book.Tekstifail);
                bookStream = new System.IO.MemoryStream(bytes);

                if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5)
                    ext = ".epub";
            }
            else if (System.IO.File.Exists(_book.Tekstifail))
            {
                bookStream = System.IO.File.OpenRead(_book.Tekstifail);
            }
            else
            {
                await DisplayAlert(AppResources.Error, AppResources.NoTextFile, AppResources.OK);
                return;
            }

            string rawHtml;

            if (ext == ".epub")
            {
                var epubBook = await VersOne.Epub.EpubReader.ReadBookAsync(bookStream);
                var chapters = epubBook.ReadingOrder.Select(item =>
                {
                    var content = item.Content ?? "";
                    var bodyStart = content.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                    var bodyEnd = content.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                    if (bodyStart >= 0 && bodyEnd >= 0)
                    {
                        var innerStart = content.IndexOf('>', bodyStart) + 1;
                        return content.Substring(innerStart, bodyEnd - innerStart);
                    }
                    return content;
                });

                rawHtml = string.Join("<hr/>", chapters);
                rawHtml = System.Text.RegularExpressions.Regex.Replace(rawHtml, @"<img[^>]*>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                rawHtml = System.Text.RegularExpressions.Regex.Replace(rawHtml, @"<svg[\s\S]*?</svg>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                rawHtml = System.Text.RegularExpressions.Regex.Replace(rawHtml, @"<style[\s\S]*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            else if (ext == ".txt")
            {
                using var reader = new System.IO.StreamReader(bookStream);
                var text = await reader.ReadToEndAsync();

                var lines = text.Split('\n');
                var paragraphs = new System.Text.StringBuilder();
                var currentParagraph = new System.Text.StringBuilder();

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        if (currentParagraph.Length > 0)
                        {
                            paragraphs.Append($"<p>{System.Net.WebUtility.HtmlEncode(currentParagraph.ToString().Trim())}</p>");
                            currentParagraph.Clear();
                        }
                    }
                    else
                    {
                        if (currentParagraph.Length > 0)
                            currentParagraph.Append(' ');
                        currentParagraph.Append(trimmed);
                    }
                }

                if (currentParagraph.Length > 0)
                    paragraphs.Append($"<p>{System.Net.WebUtility.HtmlEncode(currentParagraph.ToString().Trim())}</p>");

                rawHtml = paragraphs.ToString();
            }
            else if (ext == ".pdf")
            {
                await Navigation.PushAsync(new E_Raamatud.View.PdfReaderPage(
                    _book.Pealkiri, _book.Tekstifail));
                return;
            }
            else
            {
                await DisplayAlert(AppResources.Error, AppResources.BookLoadError, AppResources.OK);
                return;
            }

            await Navigation.PushAsync(new E_Raamatud.View.BookReaderPage(
                _book.Raamat_ID, _book.Pealkiri, rawHtml, _book.Kirjeldus ?? ""));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnReadTapped error: {ex.Message}");
            await DisplayAlert(AppResources.Error, AppResources.BookLoadError, AppResources.OK);
        }
    }

    private async void OnListenTapped(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_book.Audiofail))
        {
            await DisplayAlert(AppResources.Error, AppResources.NoAudioFile, AppResources.OK);
            return;
        }
        await Navigation.PushAsync(new AudioPlayerPage(
            _book.Raamat_ID, _book.Pealkiri, _book.Audiofail, _book.Pilt));
    }
}