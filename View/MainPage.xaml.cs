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

                var books = await DatabaseService.Instance.GetBooksAsync();
                var raamat = books.FirstOrDefault(b => b.Raamat_ID == selectedBook.Raamat_ID);

                if (raamat == null)
                {
                    Debug.WriteLine($"No book found with ID {selectedBook.Raamat_ID}");
                    return;
                }

                string genreName = "Tundmatu";
                if (raamat.Zanr_ID > 0)
                {
                    var genres = await DatabaseService.Instance.GetGenresAsync();
                    var genre = genres.FirstOrDefault(g => g.Zanr_ID == raamat.Zanr_ID);
                    if (genre != null)
                        genreName = genre.Nimetus;
                }

                await Navigation.PushAsync(new BookDetailPage(raamat, genreName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBookSelected: {ex.Message}");
                await DisplayAlert("Viga", "Raamatu laadimisel tekkis probleem.", "OK");
            }
        }

        private async void OnCartTapped(object sender, EventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                await DisplayAlert("Logi sisse", "Ostukorvi kasutamiseks pead olema sisse logitud.", "OK");
                return;
            }
            await Navigation.PushAsync(new CartPage(SessionService.CurrentUser.Id));
        }

        private async void OnLibraryTapped(object sender, EventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                await DisplayAlert("Logi sisse", "Raamatukogu kasutamiseks pead olema sisse logitud.", "OK");
                return;
            }
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

        // ===== Подсветка выбранного жанра =====
        private void OnGenreChipTapped(object sender, TappedEventArgs e)
        {
            if (sender is not Border tapped) return;

            // Сбросить все чипы
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

            // Подсветить выбранный
            tapped.BackgroundColor = Color.FromArgb("#2d6e68");
            tapped.Stroke = Color.FromArgb("#2d6e68");
            if (tapped.Content is Label tappedLbl)
                tappedLbl.TextColor = Color.FromArgb("#FFFFFF");

            // Передать жанр в ViewModel — пусть фильтрует книги
            if (BindingContext is BookViewModel vm && tapped.BindingContext is var genre)
            {
                if (vm.SelectGenreCommand is ICommand cmd && cmd.CanExecute(genre))
                    cmd.Execute(genre);
            }
        }
    }
}