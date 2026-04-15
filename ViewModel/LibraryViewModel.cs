using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace E_Raamatud.ViewModel
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BookWithProgress> LibraryBooks { get; set; } = new();
        public event PropertyChangedEventHandler PropertyChanged;

        public LibraryViewModel()
        {
            _ = LoadLibraryBooksAsync();
        }

        private async Task LoadLibraryBooksAsync()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId == 0) return;

                var libraryEntries = await DatabaseService.Instance.GetLibraryByUserAsync(userId);
                var allBooks = await DatabaseService.Instance.GetBooksAsync();

                LibraryBooks.Clear();
                foreach (var entry in libraryEntries)
                {
                    var book = allBooks.FirstOrDefault(b => b.Raamat_ID == entry.Raamat_ID);
                    if (book == null) continue;

                    var progress = await DatabaseService.Instance.GetReadingProgressAsync(userId, book.Raamat_ID);

                    LibraryBooks.Add(new BookWithProgress
                    {
                        Raamat_ID = book.Raamat_ID,
                        Pealkiri = book.Pealkiri,
                        Pilt = book.Pilt,
                        Tekstifail = book.Tekstifail,
                        Audiofail = book.Audiofail,   // NEW
                        CurrentPage = progress?.CurrentPage ?? 0,
                        TotalPages = progress?.TotalPages ?? 0
                    });
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LibraryBooks)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading library: {ex.Message}");
            }
        }
    }
}
