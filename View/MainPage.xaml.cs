using E_Raamatud.Model;
using E_Raamatud.Services;
using E_Raamatud.View;
using E_Raamatud.ViewModel;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace E_Raamatud
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (SessionService.CurrentUser == null)
            {
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            WelcomeLabel.Text = $"Tere, {SessionService.CurrentUser.Username}!";
            BindingContext = new BookViewModel();
        }

        private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                    return;

                var selectedBook = e.CurrentSelection[0] as BookWithGenre;
                if (selectedBook == null)
                    return;

                var raamat = new Raamat
                {
                    Raamat_ID = selectedBook.Raamat_ID,
                    Pealkiri = selectedBook.Pealkiri,
                    Kirjeldus = selectedBook.Kirjeldus,
                    Pilt = selectedBook.Pilt,
                    Tekstifail = selectedBook.Tekstifail,
                    Audiofail = selectedBook.Audiofail
                };

                await Navigation.PushAsync(new BookDetailPage(raamat, selectedBook.Zanr_Nimi));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBookSelected: {ex.Message}");
                await DisplayAlert("Viga", "Raamatu laadimisel tekkis probleem.", "OK");
            }
        }

        private async void OnUploadTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddBookPage());
        }

        private async void OnLibraryTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LibraryPage());
        }

        private async void OnUserTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserProfilePage());
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (BooksGridLayout == null) return;

            double width = this.Width;
            if (width <= 0) return;

            int span;
            if (width < 500) span = 2;
            else if (width < 800) span = 3;
            else if (width < 1100) span = 4;
            else if (width < 1400) span = 5;
            else span = 6;

            BooksGridLayout.Span = span;
        }

        private void OnGenreChipTapped(object sender, TappedEventArgs e)
        {
            if (sender is not Border tapped) return;

            foreach (var child in GenresLayout.Children)
            {
                if (child is Border b)
                {
                    b.BackgroundColor = Color.FromArgb("#FFFFFF");
                    b.Stroke = Color.FromArgb("#d5dfd8");
                    if (b.Content is Label lbl)
                        lbl.TextColor = Color.FromArgb("#3a5c52");
                }
            }

            tapped.BackgroundColor = Color.FromArgb("#2d6e68");
            tapped.Stroke = Color.FromArgb("#2d6e68");
            if (tapped.Content is Label tappedLbl)
                tappedLbl.TextColor = Color.FromArgb("#FFFFFF");

            if (BindingContext is BookViewModel vm && tapped.BindingContext is var genre)
            {
                if (vm.SelectGenreCommand is ICommand cmd && cmd.CanExecute(genre))
                    cmd.Execute(genre);
            }
        }

        private void OnFileTypeChipTapped(object sender, TappedEventArgs e)
        {
            if (sender is not Border tapped) return;

            // Reset all file type chips
            foreach (var child in FileTypeLayout.Children)
            {
                if (child is Border b)
                {
                    b.BackgroundColor = Color.FromArgb("#FFFFFF");
                    b.Stroke = Color.FromArgb("#d5dfd8");
                    if (b.Content is Label lbl)
                        lbl.TextColor = Color.FromArgb("#3a5c52");
                }
            }

            // Highlight selected
            tapped.BackgroundColor = Color.FromArgb("#2d6e68");
            tapped.Stroke = Color.FromArgb("#2d6e68");
            if (tapped.Content is Label tappedLbl2)
                tappedLbl2.TextColor = Color.FromArgb("#FFFFFF");

            // Get file type from label text
            string fileType = "Kõik";
            if (tapped.Content is Label label)
            {
                fileType = label.Text switch
                {
                    "EPUB" => "EPUB",
                    "PDF" => "PDF",
                    "TXT" => "TXT",
                    _ => "Kõik"
                };
            }

            if (BindingContext is BookViewModel vm)
                vm.SelectedFileType = fileType;
        }

        private async void OnBookMenuTapped(object sender, TappedEventArgs e)
        {
            var border = sender as BindableObject;
            var book = border?.BindingContext as BookWithGenre;
            if (book == null) return;

            string action = await DisplayActionSheet(
                book.Pealkiri, "Tühista", null,
                "Muuda raamatut", "Kustuta raamat");

            if (action == "Kustuta raamat")
            {
                bool confirm = await DisplayAlert(
                    "Kustuta", $"Kas oled kindel, et soovid kustutada \"{book.Pealkiri}\"?",
                    "Kustuta", "Tühista");

                if (confirm)
                {
                    await DatabaseService.Instance.DeleteBookAsync(book.Raamat_ID);
                    if (BindingContext is BookViewModel vm)
                        vm.Books.Remove(book);
                }
            }
            else if (action == "Muuda raamatut")
            {
                var raamat = new Raamat
                {
                    Raamat_ID = book.Raamat_ID,
                    Pealkiri = book.Pealkiri,
                    Kirjeldus = book.Kirjeldus,
                    Pilt = book.Pilt,
                    Tekstifail = book.Tekstifail,
                    Audiofail = book.Audiofail
                };
                await Navigation.PushAsync(new AddBookPage(raamat));
            }
        }
    }
}