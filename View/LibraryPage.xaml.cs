using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;
using Microsoft.Maui.Controls;
using System.Collections.Specialized;

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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LibraryBooks.CollectionChanged -= LibraryBooks_CollectionChanged;
            _ = ReloadAndBind();
        }

        private async Task ReloadAndBind()
        {
            await _vm.ReloadAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BindBooksCollection();
                _vm.LibraryBooks.CollectionChanged -= LibraryBooks_CollectionChanged;
                _vm.LibraryBooks.CollectionChanged += LibraryBooks_CollectionChanged;
            });
        }

        private void LibraryBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(BindBooksCollection);
        }

        private void BindBooksCollection()
        {
            if (_vm == null || BooksList == null) return;

            if (BooksList.ItemsSource != _vm.LibraryBooks)
                BooksList.ItemsSource = _vm.LibraryBooks;

            int count = _vm.LibraryBooks.Count;

            BooksCountLabel.Text = count switch
            {
                0 => AppResources.LibraryEmpty,
                1 => "1 raamat",
                _ => $"{count} raamatut"
            };

            EmptyView.IsVisible  = count == 0;
            BooksScroll.IsVisible = count > 0;
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnBookTapped(object sender, EventArgs e)
        {
            var view = sender as BindableObject;
            var book = view?.BindingContext as BookWithProgress;
            if (book == null) return;

            var raamat = new Raamat
            {
                Raamat_ID  = book.Raamat_ID,
                Pealkiri   = book.Pealkiri,
                Kirjeldus  = book.Kirjeldus,
                Pilt       = book.Pilt,
                Tekstifail = book.Tekstifail,
                Audiofail  = book.Audiofail
            };

            await Navigation.PushAsync(new BookDetailPage(raamat, ""));
        }

        private async void OnBookMenuTapped(object sender, EventArgs e)
        {
            var border = sender as BindableObject;
            var book = border?.BindingContext as BookWithProgress;
            if (book == null) return;

            var ext = System.IO.Path.GetExtension(book.Tekstifail)?.ToLowerInvariant();
            bool isEpub = ext == ".epub" ||
                          (book.Tekstifail?.ToLower().Contains(".epub") == true);

            var options = new List<string> { "Eemalda lugemisloendist" };
            if (isEpub) options.Add("Märgi loetuks");

            string action = await DisplayActionSheet(
                book.Pealkiri, "Tühista", null, options.ToArray());

            if (action == "Eemalda lugemisloendist")
            {
                bool confirm = await DisplayAlert(
                    "Eemalda",
                    $"Kas soovid \"{book.Pealkiri}\" lugemisloendist eemaldada?",
                    "Eemalda", "Tühista");

                if (confirm)
                {
                    int userId = SessionService.CurrentUser?.Id ?? 0;
                    var progress = await DatabaseService.Instance
                        .GetReadingProgressAsync(userId, book.Raamat_ID);

                    if (progress != null)
                    {
                        progress.CurrentPage = 0;
                        await DatabaseService.Instance.UpdateReadingProgressAsync(progress);
                    }

                    _vm.LibraryBooks.Remove(book);
                    BindBooksCollection();
                }
            }
            else if (action == "Märgi loetuks")
            {
                bool confirm = await DisplayAlert(
                    "Märgi loetuks",
                    $"Kas soovid märkida \"{book.Pealkiri}\" loetuks?",
                    "Jah", "Tühista");

                if (confirm)
                {
                    int userId = SessionService.CurrentUser?.Id ?? 0;
                    var progress = await DatabaseService.Instance
                        .GetReadingProgressAsync(userId, book.Raamat_ID);

                    if (progress != null)
                    {
                        progress.CurrentPage = progress.TotalPages;
                        await DatabaseService.Instance.UpdateReadingProgressAsync(progress);
                    }

                    _vm.LibraryBooks.Remove(book);
                    BindBooksCollection();

                    await DisplayAlert("Suurepärane!",
                        $"\"{book.Pealkiri}\" on märgitud loetuks!", AppResources.OK);
                }
            }
        }
    }
}
