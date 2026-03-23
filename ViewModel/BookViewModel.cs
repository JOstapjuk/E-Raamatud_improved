using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private Genre _selectedGenre;
        private string _searchText;
        private List<Raamat> _allBooks = new();
        private List<Genre> _allGenres = new();

        public ObservableCollection<Genre> Genres { get; set; } = new();
        public ObservableCollection<BookWithGenre> Books { get; set; } = new();

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
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public BookViewModel()
        {
            SelectGenreCommand = new Command<Genre>(genre => SelectedGenre = genre);
            SearchCommand = new Command<string>(searchTerm =>
            {
                SearchText = searchTerm;
                SearchBooks(searchTerm);
            });

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            Genres.Clear();
            Genres.Add(new Genre { Zanr_ID = 0, Nimetus = "Kõik raamatud", Kirjeldus = "Kuva kõik raamatud" });

            _allGenres = await DatabaseService.Instance.GetGenresAsync();

            // Seed from JSON if DB is empty
            if (_allGenres.Count == 0)
            {
                await SeedGenresFromJsonAsync();
                _allGenres = await DatabaseService.Instance.GetGenresAsync();
            }

            _allBooks = await DatabaseService.Instance.GetBooksAsync();

            if (_allBooks.Count == 0)
            {
                await SeedBooksFromJsonAsync();
                _allBooks = await DatabaseService.Instance.GetBooksAsync();
            }

            foreach (var genre in _allGenres)
                Genres.Add(genre);

            LoadAllBooks();
        }

        private async Task SeedGenresFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("genres.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var genres = JsonSerializer.Deserialize<List<Genre>>(json);
            if (genres != null)
                foreach (var g in genres)
                    await DatabaseService.Instance.InsertGenreAsync(g);
        }

        private async Task SeedBooksFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("books.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var books = JsonSerializer.Deserialize<List<Raamat>>(json);
            if (books != null)
                foreach (var b in books)
                    await DatabaseService.Instance.InsertBookAsync(b);
        }

        private void LoadAllBooks()
        {
            Books.Clear();
            foreach (var b in _allBooks)
            {
                var genre = _allGenres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
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

        private void FilterBooks()
        {
            if (SelectedGenre == null || SelectedGenre.Zanr_ID == 0)
            {
                LoadAllBooks();
                return;
            }

            Books.Clear();
            foreach (var b in _allBooks.Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID))
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
        }

        private void SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) { FilterBooks(); return; }

            var source = (SelectedGenre != null && SelectedGenre.Zanr_ID != 0)
                ? _allBooks.Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID)
                : _allBooks;

            Books.Clear();
            foreach (var b in source)
            {
                var genre = _allGenres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
                var bwg = new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Hind = b.Hind,
                    Zanr_Nimi = genre?.Nimetus ?? "Tundmatu",
                    Pilt = b.Pilt
                };
                if (bwg.Pealkiri.ToLower().Contains(searchTerm.ToLower()) ||
                    bwg.Kirjeldus.ToLower().Contains(searchTerm.ToLower()) ||
                    bwg.Zanr_Nimi.ToLower().Contains(searchTerm.ToLower()))
                    Books.Add(bwg);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}