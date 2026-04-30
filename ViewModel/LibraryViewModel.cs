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
        }

        public async Task ReloadAsync()
        {
            await LoadLibraryBooksAsync();
        }

        private async Task LoadLibraryBooksAsync()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId == 0) return;

                var allBooks = await DatabaseService.Instance.GetBooksByUserAsync(userId);

                LibraryBooks.Clear();
                foreach (var book in allBooks)
                {
                    var progress = await DatabaseService.Instance
                        .GetReadingProgressAsync(userId, book.Raamat_ID);

                    int currentPage = progress?.CurrentPage ?? 0;
                    int totalPages = progress?.TotalPages ?? 0;

                    bool isFinished = totalPages > 0 && currentPage >= totalPages;
                    bool isReading = currentPage > 0 && !isFinished;
                    if (!isReading) continue;

                    LibraryBooks.Add(new BookWithProgress
                    {
                        Raamat_ID = book.Raamat_ID,
                        Pealkiri = book.Pealkiri,
                        Kirjeldus = book.Kirjeldus,
                        Pilt = book.Pilt,
                        Tekstifail = book.Tekstifail,
                        Audiofail = book.Audiofail,
                        CurrentPage = currentPage,
                        TotalPages = totalPages
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