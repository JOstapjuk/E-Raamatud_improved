using SQLite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using E_Raamatud.Model;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace E_Raamatud.ViewModel
{
    public class CartItemViewModel
    {
        public int BasketId { get; set; }
        public string BookTitle { get; set; }
        public decimal BookPrice { get; set; }
        public int Quantity { get; set; }
        public int BookId { get; set; }
        public string BookImage { get; set; }
    }

    public class CartViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;

        public ObservableCollection<CartItemViewModel> CartItems { get; set; } = new();
        public event PropertyChangedEventHandler PropertyChanged;

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPrice)));
            }
        }

        public ICommand BuyCommand { get; }
        public ICommand RemoveCommand { get; }

        public CartViewModel()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);


            LoadCartItems();

            BuyCommand = new Command(async () => await BuyItemsAsync());
            RemoveCommand = new Command<CartItemViewModel>(async (item) => await RemoveItemAsync(item));

            Task.Run(async () => await _database.CreateTableAsync<Library>());
        }

        private async void LoadCartItems()
        {
            int userId = SessionService.CurrentUser?.Id ?? 0;
            if (userId <= 0)
            {
                CartItems.Clear();
                TotalPrice = 0;
                return;
            }

            // Only load items that are still in cart (not purchased)
            var basketItems = await _database.Table<PurchaseBasket>()
                                             .Where(p => p.Kasutaja_ID == userId && p.Status == "InCart")
                                             .ToListAsync();

            var books = await _database.Table<Raamat>().ToListAsync();

            CartItems.Clear();
            decimal total = 0;

            foreach (var item in basketItems)
            {
                var book = books.FirstOrDefault(b => b.Raamat_ID == item.Raamat_ID);
                if (book != null)
                {
                    CartItems.Add(new CartItemViewModel
                    {
                        BasketId = item.Ostukorv_ID,
                        BookTitle = book.Pealkiri,
                        BookPrice = book.Hind,
                        Quantity = item.Kogus,
                        BookId = item.Raamat_ID,
                        BookImage = book.Pilt
                    });

                    total += book.Hind * item.Kogus;
                }
            }

            TotalPrice = total;
        }

        private async Task BuyItemsAsync()
        {
            try
            {
                var userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Palun logi sisse, et osta raamatuid.", "OK");
                    return;
                }

                // Get only items that are still in cart
                var basketItems = await _database.Table<PurchaseBasket>()
                    .Where(p => p.Kasutaja_ID == userId && p.Status == "InCart")
                    .ToListAsync();

                if (basketItems.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Ostukorv on tühi.", "OK");
                    return;
                }

                int skippedDuplicates = 0;

                foreach (var item in basketItems)
                {
                    var exists = await _database.Table<Library>()
                        .Where(l => l.Kasutaja_ID == userId && l.Raamat_ID == item.Raamat_ID)
                        .FirstOrDefaultAsync();

                    if (exists != null)
                    {
                        skippedDuplicates++;
                    }
                    else
                    {
                        var libraryItem = new Library
                        {
                            Kasutaja_ID = userId,
                            Raamat_ID = item.Raamat_ID
                        };

                        await _database.InsertAsync(libraryItem);
                    }

                    // Mark item as purchased and set purchase date
                    item.Status = "Purchased";
                    item.PurchaseDate = DateTime.Now;
                    await _database.UpdateAsync(item);
                }

                string resultMessage = skippedDuplicates == 0
                    ? "Raamatud lisatud sinu raamatukogusse!"
                    : $"Mõned raamatud olid juba raamatukogus ja neid ei lisatud ({skippedDuplicates}).";

                await Application.Current.MainPage.DisplayAlert("Õnnestus", resultMessage, "OK");

                LoadCartItems(); // This will now only load items with Status = "InCart"
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ostmine ebaõnnestus: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", "Ostmine ebaõnnestus.", "OK");
            }
        }

        private async Task RemoveItemAsync(CartItemViewModel item)
        {
            if (item == null)
                return;

            var basketItem = await _database.Table<PurchaseBasket>()
                .Where(p => p.Ostukorv_ID == item.BasketId)
                .FirstOrDefaultAsync();

            if (basketItem != null)
            {
                await _database.DeleteAsync(basketItem);
                LoadCartItems();
            }
        }
    }
}
