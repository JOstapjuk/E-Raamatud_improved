using E_Raamatud.Model;
using Microsoft.Maui.Storage;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class AvaldajaViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;

        private string _pealkiri;
        private string _kirjeldus;
        private decimal _hind;
        private Genre _selectedGenre;
        private string _pilt;
        private string _tekstifail;

        public ObservableCollection<Genre> Genres { get; set; }

        public string Pealkiri
        {
            get => _pealkiri;
            set
            {
                if (_pealkiri != value)
                {
                    _pealkiri = value;
                    OnPropertyChanged(nameof(Pealkiri));
                }
            }
        }

        public string Kirjeldus
        {
            get => _kirjeldus;
            set
            {
                if (_kirjeldus != value)
                {
                    _kirjeldus = value;
                    OnPropertyChanged(nameof(Kirjeldus));
                }
            }
        }

        public decimal Hind
        {
            get => _hind;
            set
            {
                if (_hind != value)
                {
                    _hind = value;
                    OnPropertyChanged(nameof(Hind));
                }
            }
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (_selectedGenre != value)
                {
                    _selectedGenre = value;
                    OnPropertyChanged(nameof(SelectedGenre));
                }
            }
        }

        public string Pilt
        {
            get => _pilt;
            set
            {
                if (_pilt != value)
                {
                    _pilt = value;
                    OnPropertyChanged(nameof(Pilt));
                }
            }
        }

        public string Tekstifail
        {
            get => _tekstifail;
            set
            {
                if (_tekstifail != value)
                {
                    _tekstifail = value;
                    OnPropertyChanged(nameof(Tekstifail));
                }
            }
        }

        // Command to add a book
        public ICommand AddBookCommand { get; }

        public AvaldajaViewModel()
        {
            Genres = new ObservableCollection<Genre>();
            AddBookCommand = new Command(async () => await AddBookAsync());

            // Initialize database connection and load genres
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<Raamat>();
            await _database.CreateTableAsync<Genre>();

            // Load genres from DB
            var genres = await _database.Table<Genre>().ToListAsync();
            Genres.Clear();
            foreach (var g in genres)
                Genres.Add(g);
        }

        private async Task AddBookAsync()
        {
            try
            {
                // Validate
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

                // Prepare folders inside app data directory
                string appData = FileSystem.AppDataDirectory;

                string imagesFolder = Path.Combine(appData, "images");
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                string rawFolder = Path.Combine(appData, "raw");
                if (!Directory.Exists(rawFolder))
                    Directory.CreateDirectory(rawFolder);

                // Save image file
                string savedImagePath = null;
                if (!string.IsNullOrEmpty(Pilt) && File.Exists(Pilt))
                {
                    string ext = Path.GetExtension(Pilt);
                    string safeTitle = MakeSafeFilename(Pealkiri);
                    string destFile = Path.Combine(imagesFolder, $"{safeTitle}{ext}");
                    destFile = GetUniqueFilePath(destFile);
                    File.Copy(Pilt, destFile, true);
                    savedImagePath = destFile;
                }

                // Save text file
                string savedTextPath = null;
                if (!string.IsNullOrEmpty(Tekstifail) && File.Exists(Tekstifail))
                {
                    string ext = Path.GetExtension(Tekstifail);
                    string safeTitle = MakeSafeFilename(Pealkiri);
                    string destFile = Path.Combine(rawFolder, $"{safeTitle}{ext}");
                    destFile = GetUniqueFilePath(destFile);
                    File.Copy(Tekstifail, destFile, true);
                    savedTextPath = destFile;
                }

                var raamat = new Raamat
                {
                    Pealkiri = Pealkiri,
                    Kirjeldus = Kirjeldus,
                    Hind = Hind,
                    Avaldaja_ID = SessionService.CurrentUser?.Id ?? 0,
                    Zanr_ID = SelectedGenre.Zanr_ID,
                    Pilt = savedImagePath,
                    Tekstifail = savedTextPath
                };

                await _database.InsertAsync(raamat);

                await App.Current.MainPage.DisplayAlert("Edu", "Raamat lisatud!", "OK");

                Pealkiri = "";
                Kirjeldus = "";
                Hind = 0;
                SelectedGenre = null;
                Pilt = "";
                Tekstifail = "";
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Viga", $"Raamatu lisamisel tekkis viga: {ex.Message}", "OK");
            }
        }

        // Helper to remove invalid filename chars
        private string MakeSafeFilename(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }

        // Helper to get unique file path if file exists by adding (1), (2), ...
        private string GetUniqueFilePath(string path)
        {
            int count = 1;
            string dir = Path.GetDirectoryName(path)!;
            string filename = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            string newPath = path;
            while (File.Exists(newPath))
            {
                newPath = Path.Combine(dir, $"{filename}({count++}){ext}");
            }
            return newPath;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
