using E_Raamatud.Model;
using E_Raamatud.Services;
using E_Raamatud.View;
using E_Raamatud.ViewModel;
using Microsoft.Maui.Controls;
using System.Diagnostics;

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
    }
}