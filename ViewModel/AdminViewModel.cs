using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace E_Raamatud.ViewModel
{
    public class AdminViewModel : BindableObject
    {
        private readonly SQLiteAsyncConnection _database;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Genre> Genres { get; } = new();
        public ObservableCollection<Raamat> Books { get; } = new();
        public ObservableCollection<Library> LibraryEntries { get; } = new();
        public ObservableCollection<PurchaseBasket> PurchaseBasketEntries { get; } = new();

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
            _database = new SQLiteAsyncConnection(System.IO.Path.Combine(FileSystem.AppDataDirectory, "Books.db"));

            LoadDataCommand = new Command(async () => await LoadDataAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());

            DeleteUserCommand = new Command<int>(async (id) => await DeleteUserAsync(id));
            DeleteGenreCommand = new Command<int>(async (id) => await DeleteGenreAsync(id));
            DeleteBookCommand = new Command<int>(async (id) => await DeleteBookAsync(id));
            DeleteLibraryEntryCommand = new Command<int>(async (id) => await DeleteLibraryEntryAsync(id));
            DeletePurchaseBasketEntryCommand = new Command<int>(async (id) => await DeletePurchaseBasketEntryAsync(id));
        }

        public async Task LoadDataAsync()
        {
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Genre>();
            await _database.CreateTableAsync<Raamat>();
            await _database.CreateTableAsync<Library>();
            await _database.CreateTableAsync<PurchaseBasket>();

            Users.Clear();
            foreach (var u in await _database.Table<User>().ToListAsync()) Users.Add(u);

            Genres.Clear();
            foreach (var g in await _database.Table<Genre>().ToListAsync()) Genres.Add(g);

            Books.Clear();
            foreach (var b in await _database.Table<Raamat>().ToListAsync()) Books.Add(b);

            LibraryEntries.Clear();
            foreach (var l in await _database.Table<Library>().ToListAsync()) LibraryEntries.Add(l);

            PurchaseBasketEntries.Clear();
            foreach (var p in await _database.Table<PurchaseBasket>().ToListAsync()) PurchaseBasketEntries.Add(p);
        }

        private async Task DeleteUserAsync(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                await _database.DeleteAsync(user);
                Users.Remove(user);
            }
        }

        private async Task DeleteGenreAsync(int id)
        {
            var genre = Genres.FirstOrDefault(g => g.Zanr_ID == id);
            if (genre != null)
            {
                await _database.DeleteAsync(genre);
                Genres.Remove(genre);
            }
        }

        private async Task DeleteBookAsync(int id)
        {
            var book = Books.FirstOrDefault(b => b.Raamat_ID == id);
            if (book != null)
            {
                await _database.DeleteAsync(book);
                Books.Remove(book);
            }
        }

        private async Task DeleteLibraryEntryAsync(int id)
        {
            var entry = LibraryEntries.FirstOrDefault(e => e.Library_ID == id);
            if (entry != null)
            {
                await _database.DeleteAsync(entry);
                LibraryEntries.Remove(entry);
            }
        }

        private async Task DeletePurchaseBasketEntryAsync(int id)
        {
            var entry = PurchaseBasketEntries.FirstOrDefault(p => p.Ostukorv_ID == id);
            if (entry != null)
            {
                await _database.DeleteAsync(entry);
                PurchaseBasketEntries.Remove(entry);
            }
        }

        private async Task LogoutAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
            if (!confirm) return;

            SessionService.Clear();

            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
