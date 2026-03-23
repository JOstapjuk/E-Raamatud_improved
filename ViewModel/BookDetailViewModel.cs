using E_Raamatud.Model;
using E_Raamatud.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    class BookDetailViewModel : INotifyPropertyChanged
    {
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
            Raamat = selectedBook ?? throw new ArgumentNullException(nameof(selectedBook));
            Zanr_Nimi = zanrNimi ?? "Unknown";
            AddToBasketCommand = new Command(async () => await AddToBasket());
        }

        private async Task AddToBasket()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;

                if (userId <= 0)
                {
                    StatusMessage = "Kasutaja pole sisse logitud!";
                    await Application.Current.MainPage.DisplayAlert("Viga", "Ostukorvi lisamiseks pead olema sisse logitud.", "OK");
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

                await DatabaseService.Instance.InsertBasketItemAsync(newItem);
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