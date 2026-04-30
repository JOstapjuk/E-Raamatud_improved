using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class AdminViewModel : BindableObject
    {
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Genre> Genres { get; } = new();
        public ObservableCollection<Raamat> Books { get; } = new();

        public ICommand LoadDataCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand DeleteGenreCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand DeleteLibraryEntryCommand { get; }
        public ICommand DeletePurchaseBasketEntryCommand { get; }

        public INavigation Navigation { get; set; }

        public AdminViewModel()
        {
            LoadDataCommand = new Command(async () => await LoadDataAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            DeleteUserCommand = new Command<int>(async (id) => await DeleteUserAsync(id));
            DeleteGenreCommand = new Command<int>(async (id) => await DeleteGenreAsync(id));
            DeleteBookCommand = new Command<int>(async (id) => await DeleteBookAsync(id));
        }

        public async Task LoadDataAsync()
        {
            Users.Clear();
            foreach (var u in await DatabaseService.Instance.GetUsersAsync()) Users.Add(u);

            Genres.Clear();
            foreach (var g in await DatabaseService.Instance.GetGenresAsync()) Genres.Add(g);

            Books.Clear();
            foreach (var b in await DatabaseService.Instance.GetBooksAsync()) Books.Add(b);

        }

        private async Task DeleteUserAsync(int id)
        {
            await DatabaseService.Instance.DeleteUserAsync(id);
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user != null) Users.Remove(user);
        }

        private async Task DeleteGenreAsync(int id)
        {
            await DatabaseService.Instance.DeleteGenreAsync(id);
            var genre = Genres.FirstOrDefault(g => g.Zanr_ID == id);
            if (genre != null) Genres.Remove(genre);
        }

        private async Task DeleteBookAsync(int id)
        {
            await DatabaseService.Instance.DeleteBookAsync(id);
            var book = Books.FirstOrDefault(b => b.Raamat_ID == id);
            if (book != null) Books.Remove(book);
        }

        private async Task LogoutAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Välju", "Kas oled kindel, et soovid välja logida?", "Jah", "Ei");
            if (!confirm) return;
            SessionService.Clear();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}