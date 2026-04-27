using E_Raamatud.Model;
using E_Raamatud.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace E_Raamatud.View;

public partial class AvaldajaReportPage : ContentPage
{
    private readonly int _avaldajaId;

    public class BookReport
    {
        public string Pealkiri { get; set; }
        public int KokkuMuudud { get; set; }
        public decimal KokkuTulu { get; set; }
    }

    public AvaldajaReportPage(int avaldajaId)
    {
        InitializeComponent();
        _avaldajaId = avaldajaId;
        _ = LoadReport(avaldajaId);
    }

    private async Task LoadReport(int avaldajaId)
    {
        try
        {
            var books = await DatabaseService.Instance.GetBooksAsync();
            var avaldajaBooks = books.Where(b => b.Avaldaja_ID == avaldajaId).ToList();

            var purchases = await DatabaseService.Instance.GetAllBasketItemsAsync();
            var purchased = purchases.Where(p => p.Status == "Purchased").ToList();

            var report = avaldajaBooks.Select(book =>
            {
                var bookPurchases = purchased.Where(p => p.Raamat_ID == book.Raamat_ID);
                int totalSold = bookPurchases.Sum(p => p.Kogus);
                decimal totalRevenue = bookPurchases.Sum(p => p.Lõppu_hind);
                return new BookReport
                {
                    Pealkiri = book.Pealkiri,
                    KokkuMuudud = totalSold,
                    KokkuTulu = totalRevenue
                };
            })
            .Where(r => r.KokkuMuudud > 0)
            .OrderByDescending(r => r.KokkuTulu)
            .ToList();

            // Сводные данные
            int totalBooksSold = report.Sum(r => r.KokkuMuudud);
            decimal totalRevenueAll = report.Sum(r => r.KokkuTulu);

            if (TotalSoldLabel != null)
                TotalSoldLabel.Text = totalBooksSold.ToString();

            if (TotalRevenueLabel != null)
                TotalRevenueLabel.Text = $"EUR {totalRevenueAll:F2}";

            if (ReportSubtitle != null)
            {
                ReportSubtitle.Text = report.Count switch
                {
                    0 => "Pole veel muuke",
                    1 => "1 raamat muudud",
                    _ => $"{report.Count} raamatut muudud"
                };
            }

            // Привязываем список
            if (ReportListLayout != null)
                ReportListLayout.BindingContext = new ObservableCollection<BookReport>(report);

            // Переключаем видимость
            if (report.Count == 0)
            {
                if (EmptyView != null) EmptyView.IsVisible = true;
                if (ReportScroll != null) ReportScroll.IsVisible = false;
                if (ErrorView != null) ErrorView.IsVisible = false;
            }
            else
            {
                if (EmptyView != null) EmptyView.IsVisible = false;
                if (ReportScroll != null) ReportScroll.IsVisible = true;
                if (ErrorView != null) ErrorView.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadReport error: {ex.Message}");
            ShowError($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        if (EmptyView != null) EmptyView.IsVisible = false;
        if (ReportScroll != null) ReportScroll.IsVisible = false;
        if (ErrorView != null) ErrorView.IsVisible = true;
        if (ErrorMessage != null) ErrorMessage.Text = message;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}