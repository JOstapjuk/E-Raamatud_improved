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
        private Genre _selectedGenre;
        private string _pilt;
        private string _tekstifail;
        private string _statusMessage;
        private bool _isUploading;
        private string _uploadProgress;
        private int? _editingBookId = null;

        public ObservableCollection<string> AudioFiles { get; set; } = new();
        public ObservableCollection<Genre> Genres { get; set; } = new();

        public string Pealkiri { get => _pealkiri; set { _pealkiri = value; OnPropertyChanged(nameof(Pealkiri)); } }
        public string Kirjeldus { get => _kirjeldus; set { _kirjeldus = value; OnPropertyChanged(nameof(Kirjeldus)); } }
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

        public AvaldajaViewModel(Raamat existingBook = null)
        {
            AddBookCommand = new Command(async () => await AddBookAsync());
            RemoveAudioFileCommand = new Command<string>(path =>
            {
                AudioFiles.Remove(path);
                OnPropertyChanged(nameof(AudioFileSummary));
            });

            _ = InitializeAsync(existingBook);
        }

        private async Task InitializeAsync(Raamat existingBook = null)
        {
            var genres = await DatabaseService.Instance.GetGenresAsync();
            Genres.Clear();
            foreach (var g in genres)
                Genres.Add(g);

            if (existingBook != null)
            {
                _editingBookId = existingBook.Raamat_ID;
                Pealkiri = existingBook.Pealkiri;
                Kirjeldus = existingBook.Kirjeldus;
                Pilt = existingBook.Pilt;
                Tekstifail = existingBook.Tekstifail;
                SelectedGenre = Genres.FirstOrDefault(g => g.Zanr_ID == existingBook.Zanr_ID);
            }
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
            if (string.IsNullOrWhiteSpace(Tekstifail))
            {
                await App.Current.MainPage.DisplayAlert("Viga",
                    "Palun vali raamatu fail (EPUB, PDF või TXT).", "OK");
                return;
            }

            IsUploading = true;
            StatusMessage = string.Empty;
            string audiofailValue = null;
            string piltValue = Pilt;
            string tekstifailValue = Tekstifail;

            try
            {
                var userId = SessionService.CurrentUser?.Id ?? 0;
                var ext = System.IO.Path.GetExtension(Tekstifail).ToLowerInvariant();

                if (!string.IsNullOrWhiteSpace(Pilt) &&
                    System.IO.File.Exists(Pilt) &&
                    !Pilt.StartsWith("http"))
                {
                    StatusMessage = "Pildi üleslaadimine...";
                    var imagePath = $"covers/{userId}/{Guid.NewGuid()}{System.IO.Path.GetExtension(Pilt)}";
                    piltValue = await StorageService.Instance.UploadImageAsync(Pilt, imagePath);
                }
                else if (string.IsNullOrWhiteSpace(Pilt) && ext == ".pdf" &&
                         System.IO.File.Exists(Tekstifail))
                {
                    StatusMessage = "PDF kaanepildi genereerimine...";
                    string thumbnailPath = null;

                    #if ANDROID
                        thumbnailPath = await E_Raamatud.Platforms.Android.PdfThumbnailService
                            .ExtractFirstPageAsync(Tekstifail);
                    #endif

                    if (!string.IsNullOrWhiteSpace(thumbnailPath) &&
                        System.IO.File.Exists(thumbnailPath))
                    {
                        var imagePath = $"covers/{userId}/{Guid.NewGuid()}.jpg";
                        piltValue = await StorageService.Instance.UploadImageAsync(thumbnailPath, imagePath);
                        try { System.IO.File.Delete(thumbnailPath); } catch { }
                    }
                }

                if (!string.IsNullOrWhiteSpace(Tekstifail) &&
                    System.IO.File.Exists(Tekstifail) &&
                    !Tekstifail.StartsWith("http"))
                    {
                    StatusMessage = "Raamatu faili üleslaadimine...";
                    var bookFileName = $"books/{userId}/{Guid.NewGuid()}{System.IO.Path.GetExtension(Tekstifail)}";
                    var bookBytes = await System.IO.File.ReadAllBytesAsync(Tekstifail);
                    var mimeType = ext switch
                    {
                        ".epub" => "application/epub+zip",
                        ".pdf" => "application/pdf",
                        ".txt" => "text/plain",
                        _ => "application/octet-stream"
                    };
                    await DatabaseService.Instance.UploadFileAsync(bookFileName, bookBytes, mimeType);
                    tekstifailValue = DatabaseService.Instance.GetFileUrl(bookFileName);
                }

                if (AudioFiles.Count > 0 &&
                    AudioFiles.Any(f => !f.StartsWith("http") && System.IO.File.Exists(f)))
                {
                    StatusMessage = "Audiofailide üleslaadimine...";
                    var folderName = (!string.IsNullOrWhiteSpace(Pealkiri)
                        ? Pealkiri : Guid.NewGuid().ToString())
                        .ToLowerInvariant().Replace(" ", "-").Replace("'", "").Replace("\"", "");

                    audiofailValue = await StorageService.Instance.UploadAudioChaptersAsync(
                        folderName, AudioFiles.Where(f => !f.StartsWith("http")),
                        onProgress: (done, total) =>
                            MainThread.BeginInvokeOnMainThread(() =>
                                UploadProgress = $"Üles laetud {done} / {total} faili..."));
                }

                StatusMessage = "Salvestamine...";

                var book = new Raamat
                {
                    Pealkiri = !string.IsNullOrWhiteSpace(Pealkiri)
                        ? Pealkiri
                        : System.IO.Path.GetFileNameWithoutExtension(Tekstifail),
                    Kirjeldus = Kirjeldus,
                    Zanr_ID = SelectedGenre?.Zanr_ID,
                    User_ID = SessionService.CurrentUser?.Id,
                    Pilt = piltValue,
                    Tekstifail = tekstifailValue,
                    Audiofail = audiofailValue
                };

                if (_editingBookId.HasValue)
                {
                    book.Raamat_ID = _editingBookId.Value;
                    await DatabaseService.Instance.UpdateBookAsync(book);
                }
                else
                {
                    await DatabaseService.Instance.InsertBookAsync(book);
                }

                StatusMessage = string.Empty;
                UploadProgress = string.Empty;
                Pealkiri = string.Empty;
                Kirjeldus = string.Empty;
                Pilt = null;
                Tekstifail = null;
                SelectedGenre = null;
                AudioFiles.Clear();
                OnPropertyChanged(nameof(AudioFileSummary));

                _editingBookId = null;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await App.Current.MainPage.Navigation.PopAsync();
                });
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