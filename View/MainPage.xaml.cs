using E_Raamatud.Model;
using E_Raamatud.ViewModel;
using E_Raamatud.View;
using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_Raamatud
{
    public partial class MainPage : ContentPage
    {
        private SQLiteAsyncConnection _database;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new BookViewModel();

            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
                _database = new SQLiteAsyncConnection(dbPath);
                InitializeDatabaseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            if (_database != null)
            {
                await _database.CreateTableAsync<Raamat>().ConfigureAwait(false);
                await _database.CreateTableAsync<Genre>().ConfigureAwait(false);
                await _database.CreateTableAsync<PurchaseBasket>().ConfigureAwait(false);
            }
        }

        private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                {
                    Debug.WriteLine("No book selected");
                    return;
                }

                var selectedBook = e.CurrentSelection[0] as BookWithGenre;
                if (selectedBook == null)
                {
                    Debug.WriteLine("Selected book is null or not of type BookWithGenre");
                    return;
                }

                Debug.WriteLine($"Selected book: {selectedBook.Pealkiri}");

                if (_database == null)
                {
                    Debug.WriteLine("Database connection not established");
                    return;
                }

                var raamat = await _database.Table<Raamat>()
                    .Where(b => b.Raamat_ID == selectedBook.Raamat_ID)
                    .FirstOrDefaultAsync();

                if (raamat == null)
                {
                    Debug.WriteLine($"No book found with ID {selectedBook.Raamat_ID}");
                    return;
                }

                string genreName = "Tundmatu";
                if (raamat.Zanr_ID > 0)
                {
                    var genre = await _database.Table<Genre>()
                        .Where(g => g.Zanr_ID == raamat.Zanr_ID)
                        .FirstOrDefaultAsync();

                    if (genre != null)
                    {
                        genreName = genre.Nimetus;
                    }
                }

                await Navigation.PushAsync(new BookDetailPage(raamat, genreName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBookSelected: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading the book details.", "OK");
            }
        }

        private async void OnCartTapped(object sender, EventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                await DisplayAlert("Login required", "Please log in to access your cart.", "OK");
                return;
            }

            await Navigation.PushAsync(new CartPage(SessionService.CurrentUser.Id));
        }

        private async void OnLibraryTapped(object sender, EventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                await DisplayAlert("Login required", "Please log in to access your library.", "OK");
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
