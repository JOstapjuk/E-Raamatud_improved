using E_Raamatud.Model;
using Supabase;
using System.Diagnostics;

namespace E_Raamatud.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private readonly Client _client;

        private const string SupabaseUrl = SupabaseConfig.Url;
        private const string SupabaseKey = SupabaseConfig.AnonKey;

        private DatabaseService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            };
            _client = new Client(SupabaseUrl, SupabaseKey, options);
        }

        public async Task InitializeAsync()
        {
            await _client.InitializeAsync();
        }

        // Users
        public async Task<List<User>> GetUsersAsync()
            => (await _client.From<User>().Get()).Models;

        public async Task<User?> GetUserByCredentialsAsync(string username, string password)
        {
            var result = await _client.From<User>().Get();
            return result.Models.FirstOrDefault(u =>
                u.Username.ToLower() == username.ToLower() && u.Password == password);
        }

        public async Task InsertUserAsync(User user)
            => await _client.From<User>().Insert(user);

        public async Task UpdateUserAsync(User user)
            => await _client.From<User>().Update(user);

        public async Task DeleteUserAsync(int id)
            => await _client.From<User>().Where(u => u.Id == id).Delete();

        public async Task<string?> UploadProfilePictureAsync(int userId, byte[] imageBytes, string fileName)
        {
            try
            {
                var bucketName = "profile-pictures";
                var path = $"{userId}/{fileName}";

                await _client.Storage
                    .From(bucketName)
                    .Upload(imageBytes, path, new Supabase.Storage.FileOptions
                    {
                        Upsert = true,
                        ContentType = "image/jpeg"
                    });

                var publicUrl = _client.Storage
                    .From(bucketName)
                    .GetPublicUrl(path);

                return publicUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload error: {ex.Message}");
                return null;
            }
        }

        // Books
        public async Task<List<Raamat>> GetBooksAsync()
    => (await _client.From<Raamat>().Get()).Models;

        public async Task<List<Raamat>> GetBooksByUserAsync(int userId)
            => (await _client.From<Raamat>().Where(b => b.User_ID == userId).Get()).Models;

        public async Task InsertBookAsync(Raamat book)
            => await _client.From<Raamat>().Insert(book);

        public async Task DeleteBookAsync(int id)
        {
            var books = await _client.From<Raamat>().Where(b => b.Raamat_ID == id).Get();
            var book = books.Models.FirstOrDefault();

            if (book != null)
            {
                if (!string.IsNullOrWhiteSpace(book.Tekstifail) && book.Tekstifail.StartsWith("http"))
                {
                    try
                    {
                        var path = ExtractStoragePath(book.Tekstifail, "books");
                        if (path != null)
                            await _client.Storage.From("books").Remove(new List<string> { path });
                    }
                    catch (Exception ex) { Debug.WriteLine($"Error deleting book file: {ex.Message}"); }
                }

                if (!string.IsNullOrWhiteSpace(book.Pilt) && book.Pilt.StartsWith("http"))
                {
                    try
                    {
                        var path = ExtractStoragePath(book.Pilt, "images");
                        if (path != null)
                            await _client.Storage.From("images").Remove(new List<string> { path });
                    }
                    catch (Exception ex) { Debug.WriteLine($"Error deleting image: {ex.Message}"); }
                }
            }

            await _client.From<Raamat>().Where(b => b.Raamat_ID == id).Delete();
        }

        private string ExtractStoragePath(string publicUrl, string bucketName)
        {
            var marker = $"/object/public/{bucketName}/";
            var idx = publicUrl.IndexOf(marker);
            if (idx < 0) return null;
            return publicUrl.Substring(idx + marker.Length);
        }

        public async Task UploadFileAsync(string storagePath, byte[] bytes, string mimeType)
        {
            await _client.Storage
                .From("books")
                .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
                {
                    ContentType = mimeType,
                    Upsert = true
                });
        }

        public string GetFileUrl(string storagePath)
        {
            return _client.Storage.From("books").GetPublicUrl(storagePath);
        }

        public async Task UpdateBookAsync(Raamat book)
        => await _client.From<Raamat>().Upsert(book);

        // Genres
        public async Task<List<Genre>> GetGenresAsync()
            => (await _client.From<Genre>().Get()).Models;

        public async Task InsertGenreAsync(Genre genre)
            => await _client.From<Genre>().Insert(genre);

        public async Task DeleteGenreAsync(int id)
            => await _client.From<Genre>().Where(g => g.Zanr_ID == id).Delete();

        // Reading Progress
        public async Task<ReadingProgress?> GetReadingProgressAsync(int userId, int bookId)
        {
            var result = await _client.From<ReadingProgress>()
                .Where(p => p.Kasutaja_ID == userId && p.Raamat_ID == bookId)
                .Get();
            return result.Models.FirstOrDefault();
        }

        public async Task InsertReadingProgressAsync(ReadingProgress progress)
            => await _client.From<ReadingProgress>().Insert(progress);

        public async Task UpdateReadingProgressAsync(ReadingProgress progress)
            => await _client.From<ReadingProgress>().Update(progress);
    }
}