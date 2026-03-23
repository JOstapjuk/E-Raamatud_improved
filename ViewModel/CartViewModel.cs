using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

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
        public ObservableCollection<CartItemViewModel> CartItems { get; set; } = new();
        public event PropertyChangedEventHandler PropertyChanged;

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPrice))); }
        }

        public ICommand BuyCommand { get; }
        public ICommand RemoveCommand { get; }

        public CartViewModel()
        {
            BuyCommand = new Command(async () => await BuyItemsAsync());
            RemoveCommand = new Command<CartItemViewModel>(async (item) => await RemoveItemAsync(item));
            _ = LoadCartItems();
        }

        private async Task LoadCartItems()
        {
            int userId = SessionService.CurrentUser?.Id ?? 0;
            if (userId == 0) return;

            var basketItems = await DatabaseService.Instance.GetBasketAsync(userId);
            var inCart = basketItems.Where(p => p.Status == "InCart").ToList();
            var books = await DatabaseService.Instance.GetBooksAsync();

            CartItems.Clear();
            foreach (var item in inCart)
            {
                var book = books.FirstOrDefault(b => b.Raamat_ID == item.Raamat_ID);
                CartItems.Add(new CartItemViewModel
                {
                    BasketId = item.Ostukorv_ID,
                    BookId = item.Raamat_ID,
                    BookTitle = book?.Pealkiri ?? "Tundmatu",
                    BookPrice = item.Lõppu_hind,
                    Quantity = item.Kogus,
                    BookImage = book?.Pilt ?? ""
                });
            }

            TotalPrice = CartItems.Sum(i => i.BookPrice * i.Quantity);
        }

        private async Task BuyItemsAsync()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId == 0) return;

                var basketItems = await DatabaseService.Instance.GetBasketAsync(userId);
                var inCart = basketItems.Where(p => p.Status == "InCart").ToList();

                if (inCart.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Ostukorv on tühi.", "OK");
                    return;
                }

                int skippedDuplicates = 0;
                var libraryItems = await DatabaseService.Instance.GetLibraryByUserAsync(userId);

                foreach (var item in inCart)
                {
                    var exists = libraryItems.FirstOrDefault(l => l.Raamat_ID == item.Raamat_ID);
                    if (exists != null)
                    {
                        skippedDuplicates++;
                    }
                    else
                    {
                        await DatabaseService.Instance.InsertLibraryItemAsync(new Library
                        {
                            Kasutaja_ID = userId,
                            Raamat_ID = item.Raamat_ID
                        });
                    }

                    item.Status = "Purchased";
                    item.PurchaseDate = DateTime.Now;
                    await DatabaseService.Instance.UpdateBasketItemAsync(item);
                }

                string msg = skippedDuplicates == 0
                    ? "Raamatud lisatud sinu raamatukogusse!"
                    : $"Mõned raamatud olid juba raamatukogus ({skippedDuplicates}).";

                await Application.Current.MainPage.DisplayAlert("Õnnestus", msg, "OK");
                await LoadCartItems();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ostmine ebaõnnestus: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", "Ostmine ebaõnnestus.", "OK");
            }
        }

        private async Task RemoveItemAsync(CartItemViewModel item)
        {
            if (item == null) return;
            await DatabaseService.Instance.DeleteBasketItemAsync(item.BasketId);
            await LoadCartItems();
        }
    }
}