using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using E_Raamatud.Model;
using Microsoft.Maui.Controls;
using SQLite;

namespace E_Raamatud.ViewModel
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;

        public ObservableCollection<Raamat> LibraryBooks { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public LibraryViewModel()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);

            LoadLibraryBooksAsync();
        }

        private async Task LoadLibraryBooksAsync()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return;

                var libraryEntries = await _database.Table<Library>().Where(l => l.Kasutaja_ID == userId).ToListAsync();
                var allBooks = await _database.Table<Raamat>().ToListAsync();

                LibraryBooks.Clear();

                foreach (var entry in libraryEntries)
                {
                    var book = allBooks.FirstOrDefault(b => b.Raamat_ID == entry.Raamat_ID);
                    if (book != null)
                    {
                        LibraryBooks.Add(book);
                    }
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LibraryBooks)));
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error loading library: {ex.Message}");
            }
        }
    }
}
