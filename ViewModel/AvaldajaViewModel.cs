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
        private string _statusMessage;
        private bool _isUploading;
        private string _uploadProgress;

        public ObservableCollection<string> AudioFiles { get; set; } = new();
        public ObservableCollection<Genre> Genres { get; set; } = new();

        public string Pealkiri { get => _pealkiri; set { _pealkiri = value; OnPropertyChanged(nameof(Pealkiri)); } }
        public string Kirjeldus { get => _kirjeldus; set { _kirjeldus = value; OnPropertyChanged(nameof(Kirjeldus)); } }
        public decimal Hind { get => _hind; set { _hind = value; OnPropertyChanged(nameof(Hind)); } }
        public Genre SelectedGenre { get => _selectedGenre; set { _selectedGenre = value; OnPropertyChanged(nameof(SelectedGenre)); } }
        public string Pilt { get => _pilt; set { _pilt = value; OnPropertyChanged(nameof(Pilt)); } }
        public string Tekstifail { get => _tekstifail; set { _tekstifail = value; OnPropertyChanged(nameof(Tekstifail)); } }

        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); } }
        public bool IsUploading { get => _isUploading; set { _isUploading = value; OnPropertyChanged(nameof(IsUploading)); } }
        public string UploadProgress { get => _uploadProgress; set { _uploadProgress = value; OnPropertyChanged(nameof(UploadProgress)); } }

        public string AudioFileSummary =>
            AudioFiles.Count == 0 ? "Audiofaile pole valitud" : $"{AudioFiles.Count} audiofaili valitud";

        public ICommand AddBookCommand { get; }
        public ICommand RemoveAudioFileCommand { get; }

        public AvaldajaViewModel()
        {
            AddBookCommand = new Command(async () => await AddBookAsync());
            RemoveAudioFileCommand = new Command<string>(path =>
            {
                AudioFiles.Remove(path);
                OnPropertyChanged(nameof(AudioFileSummary));
            });
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var genres = await DatabaseService.Instance.GetGenresAsync();
            Genres.Clear();
            foreach (var g in genres)
                Genres.Add(g);
        }

        public void AddAudioFiles(IEnumerable<string> paths)
        {
            foreach (var p in paths)
                if (!AudioFiles.Contains(p))
                    AudioFiles.Add(p);
            OnPropertyChanged(nameof(AudioFileSummary));
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

            IsUploading = true;
            StatusMessage = string.Empty;
            string audiofailValue = null;

            try
            {
                if (AudioFiles.Count > 0)
                {
                    StatusMessage = "Audiofailide üleslaadimine...";
                    var folderName = Pealkiri.ToLowerInvariant().Replace(" ", "-").Replace("'", "").Replace("\"", "");
                    audiofailValue = await StorageService.Instance.UploadAudioChaptersAsync(
                        folderName, AudioFiles,
                        onProgress: (done, total) =>
                            MainThread.BeginInvokeOnMainThread(() =>
                                UploadProgress = $"Üles laetud {done} / {total} faili..."));
                }

                StatusMessage = "Raamatu salvestamine...";

                var book = new Raamat
                {
                    Pealkiri = Pealkiri,
                    Kirjeldus = Kirjeldus,
                    Hind = Hind,
                    Zanr_ID = SelectedGenre.Zanr_ID,
                    Avaldaja_ID = SessionService.CurrentUser?.Id ?? 0,
                    Pilt = Pilt,
                    Tekstifail = Tekstifail,
                    Audiofail = audiofailValue
                };

                await DatabaseService.Instance.InsertBookAsync(book);

                StatusMessage = string.Empty;
                UploadProgress = string.Empty;
                AudioFiles.Clear();
                OnPropertyChanged(nameof(AudioFileSummary));

                await App.Current.MainPage.DisplayAlert("Õnnestus", "Raamat edukalt lisatud!", "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Viga: {ex.Message}";
            }
            finally
            {
                IsUploading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}