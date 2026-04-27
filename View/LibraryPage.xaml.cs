using E_Raamatud.Model;
using E_Raamatud.ViewModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VersOne.Epub;

namespace E_Raamatud.View
{
    public partial class LibraryPage : ContentPage
    {
        private LibraryViewModel _vm;

        public LibraryPage()
        {
            InitializeComponent();
            _vm = new LibraryViewModel();
            BindingContext = _vm;

            // Подписываемся на изменения коллекции, чтобы обновить UI когда книги загрузятся
            _vm.LibraryBooks.CollectionChanged += LibraryBooks_CollectionChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindBooksCollection();
        }

        private void LibraryBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Обновляем UI на главном потоке
            MainThread.BeginInvokeOnMainThread(BindBooksCollection);
        }

        private void BindBooksCollection()
        {
            if (_vm == null || BooksList == null)
                return;

            // Привязываем коллекцию (привязка идемпотентна - повторное присваивание не страшно)
            if (BooksList.ItemsSource != _vm.LibraryBooks)
                BooksList.ItemsSource = _vm.LibraryBooks;

            int count = _vm.LibraryBooks.Count;

            BooksCountLabel.Text = count switch
            {
                0 => "Sinu raamatukogu on tühi",
                1 => "1 raamat",
                _ => $"{count} raamatut"
            };

            EmptyView.IsVisible = count == 0;
            BooksScroll.IsVisible = count > 0;
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            // адаптив можно добавить позже
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ===== Фильтры =====
        private void OnFilterAllTapped(object sender, EventArgs e) => SetActiveFilter(FilterAll);
        private void OnFilterUnreadTapped(object sender, EventArgs e) => SetActiveFilter(FilterUnread);
        private void OnFilterReadingTapped(object sender, EventArgs e) => SetActiveFilter(FilterReading);
        private void OnFilterAudioTapped(object sender, EventArgs e) => SetActiveFilter(FilterAudio);

        private void SetActiveFilter(Border active)
        {
            var allFilters = new[] { FilterAll, FilterUnread, FilterReading, FilterAudio };
            foreach (var f in allFilters)
            {
                if (f == active)
                {
                    f.BackgroundColor = Colors.White;
                    f.Stroke = Colors.Transparent;
                    if (f.Content is Label lbl) lbl.TextColor = Color.FromArgb("#2d6e68");
                }
                else
                {
                    f.BackgroundColor = Colors.Transparent;
                    f.Stroke = Color.FromArgb("#ffffff60");
                    if (f.Content is Label lbl) lbl.TextColor = Colors.White;
                }
            }
        }

        // ===== Чтение =====
        private async void OnReadTapped(object sender, EventArgs e)
        {
            var view = sender as BindableObject;
            var book = view?.BindingContext as BookWithProgress;

            if (book == null || string.IsNullOrWhiteSpace(book.Tekstifail))
            {
                await DisplayAlert("Viga", "Raamatul puudub tekstifail.", "OK");
                return;
            }

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

                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<img[^>]*>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<svg[\s\S]*?</svg>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                rawHtml = System.Text.RegularExpressions.Regex.Replace(
                    rawHtml, @"<style[\s\S]*?</style>", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                await Navigation.PushAsync(new BookReaderPage(
                    raamatId: book.Raamat_ID,
                    title: book.Pealkiri,
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

        // ===== Прослушивание =====
        private async void OnListenTapped(object sender, EventArgs e)
        {
            var view = sender as BindableObject;
            var book = view?.BindingContext as BookWithProgress;

            if (book == null || string.IsNullOrWhiteSpace(book.Audiofail))
            {
                await DisplayAlert("Audioraamat", "Sellel raamatul pole audiofaili.", "OK");
                return;
            }

            await Navigation.PushAsync(new AudioPlayerPage(
                book.Raamat_ID, book.Pealkiri, book.Audiofail, book.Pilt));
        }
    }
}