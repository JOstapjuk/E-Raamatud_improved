using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private Genre _selectedGenre;
        private string _searchText;
        private string _selectedFileType = "Kõik";
        private List<Raamat> _allBooks = new();
        private List<Genre> _allGenres = new();

        public ObservableCollection<Genre> Genres { get; set; } = new();
        public ObservableCollection<BookWithGenre> Books { get; set; } = new();

        public ICommand SelectGenreCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SelectFileTypeCommand { get; }

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

        public string SelectedFileType
        {
            get => _selectedFileType;
            set
            {
                _selectedFileType = value;
                OnPropertyChanged(nameof(SelectedFileType));
                FilterBooks();
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
            SelectFileTypeCommand = new Command<string>(ft => SelectedFileType = ft);
            SearchCommand = new Command<string>(searchTerm =>
            {
                SearchText = searchTerm;
                SearchBooks(searchTerm);
            });

            _ = InitAsync();
        }

        private static string TranslateGenre(string estonianName)
        {
            return estonianName?.ToLower() switch
            {
                "romaan"       => AppResources.Genre_Romaan,
                "fantaasia"    => AppResources.Genre_Fantaasia,
                "klassika"     => AppResources.Genre_Klassika,
                "ajalugu"      => AppResources.Genre_Ajalugu,
                "ulme"         => AppResources.Genre_Ulme,
                "noorteraamat" => AppResources.Genre_Noorteraamat,
                "seiklus"      => AppResources.Genre_Seiklus,
                _              => estonianName // fallback: show as-is
            };
        }

        private async Task InitAsync()
        {
            int userId = SessionService.CurrentUser?.Id ?? 0;
            if (userId == 0) return;

            Genres.Clear();
            Genres.Add(new Genre { Zanr_ID = 0, Nimetus = AppResources.AllBooks });

            _allGenres = await DatabaseService.Instance.GetGenresAsync();
            foreach (var genre in _allGenres)
            {
                Genres.Add(new Genre
                {
                    Zanr_ID = genre.Zanr_ID,
                    Nimetus = TranslateGenre(genre.Nimetus)
                });
            }

            _allBooks = await DatabaseService.Instance.GetBooksByUserAsync(userId);

            await LoadAllBooksWithProgress(userId);
        }

        private async Task LoadAllBooksWithProgress(int userId)
        {
            Books.Clear();
            foreach (var b in _allBooks)
            {
                var genre = _allGenres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
                var progress = await DatabaseService.Instance.GetReadingProgressAsync(userId, b.Raamat_ID);
                Books.Add(new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Zanr_Nimi = TranslateGenre(genre?.Nimetus) ?? AppResources.Genre_Unknown,
                    Pilt = b.Pilt,
                    Tekstifail = b.Tekstifail,
                    Audiofail = b.Audiofail,
                    CurrentPage = progress?.CurrentPage ?? 0,
                    TotalPages = progress?.TotalPages ?? 0
                });
            }
        }

        private void FilterBooks()
        {
            var source = _allBooks.AsEnumerable();

            if (SelectedGenre != null && SelectedGenre.Zanr_ID != 0)
                source = source.Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID);

            source = SelectedFileType switch
            {
                "EPUB" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".epub")),
                "PDF" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".pdf")),
                "TXT" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".txt")),
                _ => source
            };

            Books.Clear();
            foreach (var b in source)
            {
                var genre = _allGenres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
                Books.Add(new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Zanr_Nimi = TranslateGenre(genre?.Nimetus) ?? AppResources.Genre_Unknown,
                    Pilt = b.Pilt,
                    Tekstifail = b.Tekstifail,
                    Audiofail = b.Audiofail
                });
            }
        }

        private void SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) { FilterBooks(); return; }

            var source = _allBooks.AsEnumerable();

            if (SelectedGenre != null && SelectedGenre.Zanr_ID != 0)
                source = source.Where(b => b.Zanr_ID == SelectedGenre.Zanr_ID);

            source = SelectedFileType switch
            {
                "EPUB" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".epub")),
                "PDF" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".pdf")),
                "TXT" => source.Where(b => b.Tekstifail != null &&
                    b.Tekstifail.ToLower().Contains(".txt")),
                _ => source
            };

            Books.Clear();
            foreach (var b in source)
            {
                var genre = _allGenres.FirstOrDefault(g => g.Zanr_ID == b.Zanr_ID);
                var bwg = new BookWithGenre
                {
                    Raamat_ID = b.Raamat_ID,
                    Pealkiri = b.Pealkiri,
                    Kirjeldus = b.Kirjeldus,
                    Zanr_Nimi = TranslateGenre(genre?.Nimetus) ?? AppResources.Genre_Unknown,
                    Pilt = b.Pilt,
                    Tekstifail = b.Tekstifail,
                    Audiofail = b.Audiofail
                };
                if (bwg.Pealkiri?.ToLower().Contains(searchTerm.ToLower()) == true ||
                    bwg.Kirjeldus?.ToLower().Contains(searchTerm.ToLower()) == true ||
                    bwg.Zanr_Nimi?.ToLower().Contains(searchTerm.ToLower()) == true)
                    Books.Add(bwg);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
