using Supabase;
using E_Raamatud.Model;

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

        public async Task InsertBookAsync(Raamat book)
            => await _client.From<Raamat>().Insert(book);

        public async Task DeleteBookAsync(int id)
            => await _client.From<Raamat>().Where(b => b.Raamat_ID == id).Delete();

        // Genres
        public async Task<List<Genre>> GetGenresAsync()
            => (await _client.From<Genre>().Get()).Models;

        public async Task InsertGenreAsync(Genre genre)
            => await _client.From<Genre>().Insert(genre);

        public async Task DeleteGenreAsync(int id)
            => await _client.From<Genre>().Where(g => g.Zanr_ID == id).Delete();

        // Basket
        public async Task<List<PurchaseBasket>> GetBasketAsync(int userId)
            => (await _client.From<PurchaseBasket>().Where(b => b.Kasutaja_ID == userId).Get()).Models;

        public async Task<List<PurchaseBasket>> GetAllBasketItemsAsync()
            => (await _client.From<PurchaseBasket>().Get()).Models;

        public async Task InsertBasketItemAsync(PurchaseBasket item)
            => await _client.From<PurchaseBasket>().Insert(item);

        public async Task UpdateBasketItemAsync(PurchaseBasket item)
            => await _client.From<PurchaseBasket>().Update(item);

        public async Task DeleteBasketItemAsync(int id)
            => await _client.From<PurchaseBasket>().Where(b => b.Ostukorv_ID == id).Delete();

        // Library
        public async Task<List<Library>> GetLibraryAsync()
            => (await _client.From<Library>().Get()).Models;

        public async Task<List<Library>> GetLibraryByUserAsync(int userId)
            => (await _client.From<Library>().Where(l => l.Kasutaja_ID == userId).Get()).Models;

        public async Task InsertLibraryItemAsync(Library item)
            => await _client.From<Library>().Insert(item);

        public async Task DeleteLibraryEntryAsync(int id)
            => await _client.From<Library>().Where(l => l.Library_ID == id).Delete();

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