using E_Raamatud.Model;
using System.ComponentModel;

namespace E_Raamatud.ViewModel
{
    public class BookDetailViewModel : INotifyPropertyChanged
    {
        public string Pealkiri => Raamat?.Pealkiri ?? "Unknown";
        public string Kirjeldus => Raamat?.Kirjeldus ?? "No description available";
        public string Zanr_Nimi { get; set; }
        public Raamat Raamat { get; set; }

        public string Pilt => string.IsNullOrWhiteSpace(Raamat?.Pilt)
            ? "placeholder_cover.png"
            : Raamat.Pilt;

        public BookDetailViewModel(Raamat selectedBook, string zanrNimi)
        {
            Raamat = selectedBook ?? throw new ArgumentNullException(nameof(selectedBook));
            Zanr_Nimi = zanrNimi ?? "Unknown";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}