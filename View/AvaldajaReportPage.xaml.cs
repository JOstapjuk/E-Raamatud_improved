using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;

namespace E_Raamatud.View;

public partial class AvaldajaReportPage : ContentPage
{
    public class BookReport
    {
        public string Pealkiri { get; set; }
        public int KokkuMüüdud { get; set; }
        public decimal KokkuTulu { get; set; }
        public string MüüdudTekst => $"Müüdud: {KokkuMüüdud} tk";
        public string TuluTekst => $"Tulu: {KokkuTulu:F2} €";
    }

    public AvaldajaReportPage(int avaldajaId)
    {
        InitializeComponent();
        _ = LoadReport(avaldajaId);
    }

    private async Task LoadReport(int avaldajaId)
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
                KokkuMüüdud = totalSold,
                KokkuTulu = totalRevenue
            };
        })
        .Where(r => r.KokkuMüüdud > 0)
        .ToList();

        ReportListView.ItemsSource = new ObservableCollection<BookReport>(report);
    }
}