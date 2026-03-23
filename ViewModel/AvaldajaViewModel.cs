using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class AvaldajaViewModel : INotifyPropertyChanged
    {
        private string _pealkiri;
        private string _kirjeldus;
        private decimal _hind;
        private Genre _selectedGenre;
        private string _pilt;
        private string _tekstifail;

        public ObservableCollection<Genre> Genres { get; set; } = new();

        public string Pealkiri { get => _pealkiri; set { _pealkiri = value; OnPropertyChanged(nameof(Pealkiri)); } }
        public string Kirjeldus { get => _kirjeldus; set { _kirjeldus = value; OnPropertyChanged(nameof(Kirjeldus)); } }
        public decimal Hind { get => _hind; set { _hind = value; OnPropertyChanged(nameof(Hind)); } }
        public Genre SelectedGenre { get => _selectedGenre; set { _selectedGenre = value; OnPropertyChanged(nameof(SelectedGenre)); } }
        public string Pilt { get => _pilt; set { _pilt = value; OnPropertyChanged(nameof(Pilt)); } }
        public string Tekstifail { get => _tekstifail; set { _tekstifail = value; OnPropertyChanged(nameof(Tekstifail)); } }

        public ICommand AddBookCommand { get; }

        public AvaldajaViewModel()
        {
            AddBookCommand = new Command(async () => await AddBookAsync());
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var genres = await DatabaseService.Instance.GetGenresAsync();
            Genres.Clear();
            foreach (var g in genres)
                Genres.Add(g);
        }

        private async Task AddBookAsync()
        {
            if (string.IsNullOrWhiteSpace(Pealkiri))
            {
                await App.Current.MainPage.DisplayAlert("Viga", "Pealkiri on kohustuslik.", "OK");
                return;
            }
            if (SelectedGenre == null)
            {
                await App.Current.MainPage.DisplayAlert("Viga", "Palun vali žanr.", "OK");
                return;
            }

            var book = new Raamat
            {
                Pealkiri = Pealkiri,
                Kirjeldus = Kirjeldus,
                Hind = Hind,
                Zanr_ID = SelectedGenre.Zanr_ID,
                Avaldaja_ID = SessionService.CurrentUser?.Id ?? 0,
                Pilt = Pilt,
                Tekstifail = Tekstifail
            };

            await DatabaseService.Instance.InsertBookAsync(book);
            await App.Current.MainPage.DisplayAlert("Õnnestus", "Raamat edukalt lisatud!", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}