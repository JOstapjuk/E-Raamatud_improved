using E_Raamatud.Model;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VersOne.Epub;

namespace E_Raamatud.View
{
    public partial class LibraryPage : ContentPage
    {
        public LibraryPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel.LibraryViewModel();
        }

        private async void OnReadTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            var book = frame?.BindingContext as BookWithProgress;

            if (book == null || string.IsNullOrWhiteSpace(book.Tekstifail))
                return;

            try
            {
                Stream epubStream;

                if (Path.IsPathRooted(book.Tekstifail) && File.Exists(book.Tekstifail))
                    epubStream = File.OpenRead(book.Tekstifail);
                else
                    epubStream = await FileSystem.OpenAppPackageFileAsync(book.Tekstifail);

                var epubBook = await EpubReader.ReadBookAsync(epubStream);

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

                var rawHtml = string.Join("<hr/>", chapters);

                // Strip img tags
                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<img[^>]*>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Strip SVG blocks
                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<svg[\s\S]*?</svg>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Strip style blocks
                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<style[\s\S]*?</style>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                Debug.WriteLine($"[EPUB] rawHtml length: {rawHtml.Length}");
                Debug.WriteLine($"[EPUB] first 200 chars: {rawHtml.Substring(0, Math.Min(200, rawHtml.Length))}");

                // Pass description so SummarizationService has extra context
                await Navigation.PushAsync(new BookReaderPage(
                    raamatId:    book.Raamat_ID,
                    title:       book.Pealkiri,
                    htmlContent: rawHtml,
                    description: book.Kirjeldus ?? ""));
            }
            catch (FileNotFoundException)
            {
                await DisplayAlert("Viga", "Raamatu tekstifaili ei leitud.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Viga raamatu avamisel: {ex.Message}");
                await DisplayAlert("Viga", "Raamatu avamisel tekkis probleem.", "OK");
            }
        }

        private async void OnListenTapped(object sender, EventArgs e)
        {
            var button = sender as Button;
            var book = button?.BindingContext as BookWithProgress;

            if (book == null || string.IsNullOrWhiteSpace(book.Audiofail))
                return;

            await Navigation.PushAsync(new AudioPlayerPage(
                book.Raamat_ID, book.Pealkiri, book.Audiofail, book.Pilt));
        }
    }
}