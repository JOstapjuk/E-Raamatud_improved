using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;

namespace E_Raamatud;

public partial class PurchaseHistoryPage : ContentPage
{
    private readonly SQLiteAsyncConnection _db;

    public PurchaseHistoryPage()
    {
        InitializeComponent();
        _db = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "Books.db"));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPurchaseHistoryAsync();
    }

    private async Task LoadPurchaseHistoryAsync()
    {
        if (SessionService.CurrentUser == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: SessionService.CurrentUser is null!");
            await DisplayAlert("Viga", "Kasutaja pole sisse logitud.", "OK");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Current User - ID: {SessionService.CurrentUser.Id}, Username: {SessionService.CurrentUser.Username}");

        // Get only PURCHASED items for purchase history
        var purchasedItems = await _db.Table<PurchaseBasket>()
                               .Where(pb => pb.Kasutaja_ID == SessionService.CurrentUser.Id && pb.Status == "Purchased")
                               .ToListAsync();

        System.Diagnostics.Debug.WriteLine($"Purchased items found for user ID {SessionService.CurrentUser.Id}: {purchasedItems.Count}");

        var displayList = new List<PurchaseDisplay>();

        foreach (var basketItem in purchasedItems)
        {
            var book = await _db.Table<Raamat>()
                                .Where(r => r.Raamat_ID == basketItem.Raamat_ID)
                                .FirstOrDefaultAsync();

            System.Diagnostics.Debug.WriteLine($"BasketItem Raamat_ID={basketItem.Raamat_ID}, Book found: {book?.Pealkiri ?? "null"}");

            if (book != null)
            {
                displayList.Add(new PurchaseDisplay
                {
                    BookTitle = book.Pealkiri,
                    Quantity = basketItem.Kogus,
                    TotalPrice = basketItem.Lõppu_hind,
                    PurchaseDate = basketItem.PurchaseDate ?? DateTime.Now,
                    BookImage = book.Pilt
                });
            }
            else
            {
                displayList.Add(new PurchaseDisplay
                {
                    BookTitle = $"Raamat ID {basketItem.Raamat_ID} puudub",
                    Quantity = basketItem.Kogus,
                    TotalPrice = basketItem.Lõppu_hind,
                    PurchaseDate = basketItem.PurchaseDate ?? DateTime.Now,
                    BookImage = null
                });
            }
        }

        // Sort by purchase date (newest first)
        displayList = displayList.OrderByDescending(x => x.PurchaseDate).ToList();

        PurchaseList.ItemsSource = displayList;

        // Show/hide empty state
        EmptyLabel.IsVisible = displayList.Count == 0;
        PurchaseList.IsVisible = displayList.Count > 0;

        System.Diagnostics.Debug.WriteLine($"Display list created with {displayList.Count} items");
    }

    public class PurchaseDisplay
    {
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string BookImage { get; set; }
        public string FormattedDate => PurchaseDate.ToString("dd.MM.yyyy HH:mm");
    }
}