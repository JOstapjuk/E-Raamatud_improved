using E_Raamatud.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        try
        {
            if (SessionService.CurrentUser == null)
            {
                ShowError("Kasutaja pole sisse logitud");
                return;
            }

            // Создаём таблицы если их нет
            await _db.CreateTableAsync<PurchaseBasket>();
            await _db.CreateTableAsync<Raamat>();

            var userId = SessionService.CurrentUser.Id;

            var purchasedItems = await _db.Table<PurchaseBasket>()
                                  .Where(pb => pb.Kasutaja_ID == userId && pb.Status == "Purchased")
                                  .ToListAsync();

            var displayList = new List<PurchaseDisplay>();

            foreach (var basketItem in purchasedItems)
            {
                var book = await _db.Table<Raamat>()
                                    .Where(r => r.Raamat_ID == basketItem.Raamat_ID)
                                    .FirstOrDefaultAsync();

                displayList.Add(new PurchaseDisplay
                {
                    BookTitle = book?.Pealkiri ?? $"Raamat ID {basketItem.Raamat_ID}",
                    Quantity = basketItem.Kogus,
                    TotalPrice = basketItem.Lõppu_hind,
                    PurchaseDate = basketItem.PurchaseDate ?? DateTime.Now,
                    BookImage = book?.Pilt
                });
            }

            displayList = displayList.OrderByDescending(x => x.PurchaseDate).ToList();

            int count = displayList.Count;
            decimal totalSpent = displayList.Sum(x => x.TotalPrice);

            if (TotalOrdersLabel != null)
                TotalOrdersLabel.Text = count.ToString();

            if (TotalSpentLabel != null)
                TotalSpentLabel.Text = $"€ {totalSpent:F2}";

            if (OrdersCountLabel != null)
            {
                OrdersCountLabel.Text = count switch
                {
                    0 => "Pole veel oste",
                    1 => "1 ost",
                    _ => $"{count} ostu"
                };
            }

            if (PurchasesList != null)
                PurchasesList.ItemsSource = displayList;

            if (count == 0)
            {
                if (EmptyView != null) EmptyView.IsVisible = true;
                if (PurchasesScroll != null) PurchasesScroll.IsVisible = false;
                if (ErrorView != null) ErrorView.IsVisible = false;
            }
            else
            {
                if (EmptyView != null) EmptyView.IsVisible = false;
                if (PurchasesScroll != null) PurchasesScroll.IsVisible = true;
                if (ErrorView != null) ErrorView.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadPurchaseHistoryAsync error: {ex.Message}");
            ShowError($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        if (EmptyView != null) EmptyView.IsVisible = false;
        if (PurchasesScroll != null) PurchasesScroll.IsVisible = false;
        if (ErrorView != null) ErrorView.IsVisible = true;
        if (ErrorMessage != null) ErrorMessage.Text = message;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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