using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;
        private Genre _selectedGenre;
        private string _searchText;

        public ObservableCollection<Genre> Genres { get; set; }
        public ObservableCollection<BookWithGenre> Books { get; set; }

        public ICommand SelectGenreCommand { get; }
        public ICommand SearchCommand { get; }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (_selectedGenre != value)
                {
                    _selectedGenre = value;
                    OnPropertyChanged(nameof(SelectedGenre));
                    FilterBooks();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public BookViewModel()
        {
            Genres = new ObservableCollection<Genre>();
            Books = new ObservableCollection<BookWithGenre>();

            SelectGenreCommand = new Command<Genre>(genre => SelectedGenre = genre);
            SearchCommand = new Command<string>(searchTerm =>
            {
                SearchText = searchTerm;
                SearchBooks(searchTerm);
            });

            InitAsync();
        }

        private async Task InitAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<Raamat>();
            await _database.CreateTableAsync<Genre>();

            Genres.Clear();
            Genres.Add(new Genre { Zanr_ID = 0, Nimetus = "Kõik raamatud", Kirjeldus = "Kuva kõik raamatud" });

            if (await _database.Table<Genre>().CountAsync() == 0)
                await LoadGenresFromJsonAsync();

            if (await _database.Table<Raamat>().CountAsync() == 0)
                await LoadBooksFromJsonAsync();

            var genreList = await _database.Table<Genre>().ToListAsync();
            foreach (var genre in genreList)
                Genres.Add(genre);

            await LoadAllBooks();
        }

        private async Task LoadGenresFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("genres.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var genres = JsonSerializer.Deserialize<List<Genre>>(json);
            if (genres != null)
                await _database.InsertAllAsync(genres);
        }

        private async Task LoadBooksFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("books.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var books = JsonSerializer.Deserialize<List<Raamat>>(json);
            if (books != null)
                await _database.InsertAllAsync(books);
        }

        private async Task LoadAllBooks()
        {
            var genres = await _database.Table<Genre>().ToListAsync();
            var books = await _database.Table<Raamat>().ToListAsync();

            Books.Clear();
            foreach (var b in books)
            {
                var genre = genres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
                Books.Add(new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Hind = b.Hind,
                    Zanr_Nimi = genre?.Nimetus ?? "Tundmatu",
                    Pilt = b.Pilt
                });
            }
        }

        private async void FilterBooks()
        {
            if (SelectedGenre.Zanr_ID == 0)
            {
                await LoadAllBooks();
                return;
            }

            var books = await _database.Table<Raamat>()
                .Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID)
                .ToListAsync();

            Books.Clear();
            foreach (var b in books)
            {
                Books.Add(new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Hind = b.Hind,
                    Zanr_Nimi = SelectedGenre.Nimetus,
                    Pilt = b.Pilt
                });
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
                SearchBooks(SearchText);
        }

        private async void SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                FilterBooks();
                return;
            }

            var genres = await _database.Table<Genre>().ToListAsync();
            var booksQuery = _database.Table<Raamat>();

            if (SelectedGenre != null && SelectedGenre.Zanr_ID != 0)
                booksQuery = booksQuery.Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID);

            var books = await booksQuery.ToListAsync();

            var filteredBooks = books
                .Select(b => new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Hind = b.Hind,
                    Zanr_Nimi = genres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID)?.Nimetus ?? "Tundmatu",
                    Pilt = b.Pilt
                })
                .Where(b =>
                    b.Pealkiri.ToLower().Contains(searchTerm.ToLower()) ||
                    b.Kirjeldus.ToLower().Contains(searchTerm.ToLower()) ||
                    b.Zanr_Nimi.ToLower().Contains(searchTerm.ToLower()))
                .ToList();

            Books.Clear();
            foreach (var book in filteredBooks)
                Books.Add(book);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
