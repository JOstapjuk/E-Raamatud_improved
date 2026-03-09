using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using E_Raamatud.Model;
using Microsoft.Maui.Controls;
using SQLite;

namespace E_Raamatud.ViewModel
{
    class BookDetailViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;

        public string Pealkiri => Raamat?.Pealkiri ?? "Unknown";
        public string Kirjeldus => Raamat?.Kirjeldus ?? "No description available";
        public decimal Hind => Raamat?.Hind ?? 0;
        public string Pilt => Raamat?.Pilt ?? "default_image.png";
        public string Zanr_Nimi { get; set; }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
            }
        }

        public ICommand AddToBasketCommand { get; }
        public Raamat Raamat { get; set; }

        public BookDetailViewModel(Raamat selectedBook, string zanrNimi)
        {
            if (selectedBook == null)
                throw new ArgumentNullException(nameof(selectedBook));

            Raamat = selectedBook;
            Zanr_Nimi = zanrNimi ?? "Unknown";

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);

            AddToBasketCommand = new Command(async () => await AddToBasket());

            Task.Run(async () =>
            {
                try
                {
                    await InitializeDatabaseAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Database initialization error: {ex.Message}";
                    Debug.WriteLine($"Database initialization error: {ex.Message}");
                }
            });
        }


        private async Task InitializeDatabaseAsync()
        {
            if (_database != null)
            {
                await _database.CreateTableAsync<PurchaseBasket>().ConfigureAwait(false);
            }
        }

        private async Task AddToBasket()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;

                if (userId <= 0)
                {
                    StatusMessage = "User not logged in!";
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to add items to the basket.", "OK");
                    return;
                }

                if (_database == null)
                {
                    StatusMessage = "Database connection not established";
                    return;
                }

                if (Raamat == null)
                {
                    StatusMessage = "Raamat on null! Ei saa lisada ostukorvi.";
                    return;
                }

                if (Raamat.Raamat_ID == 0)
                {
                    StatusMessage = "Raamat_ID is not set!";
                    Debug.WriteLine("Raamat_ID is 0 or missing.");
                    return;
                }

                var newItem = new PurchaseBasket
                {
                    Kasutaja_ID = userId,
                    Raamat_ID = Raamat.Raamat_ID,
                    Kogus = 1,
                    Lõppu_hind = Raamat.Hind
                };

                await _database.InsertAsync(newItem);
                StatusMessage = "Raamat lisati ostukorvi";
                await Application.Current.MainPage.DisplayAlert("Lisatud", "Raamat lisati ostukorvi!", "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Viga: {ex.Message}";
                Debug.WriteLine($"Error adding to basket: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}