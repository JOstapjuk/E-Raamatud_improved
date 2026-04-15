using Supabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_Raamatud.Services
{
    public class StorageService
    {
        private static StorageService _instance;
        public static StorageService Instance => _instance ??= new StorageService();

        private readonly Client _client;
        private const string BucketName = "audiobooks";

        private StorageService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            };
            _client = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, options);
            _ = _client.InitializeAsync();
        }

        /// <summary>
        /// Uploads a list of local audio file paths to Supabase Storage under a book folder.
        /// Returns a pipe-separated string of public URLs, sorted by filename.
        /// e.g. "https://.../01 - Intro.mp3|https://.../02 - Chapter.mp3"
        /// </summary>
        public async Task<string> UploadAudioChaptersAsync(
            string bookFolderName,
            IEnumerable<string> localFilePaths,
            Action<int, int> onProgress = null)
        {
            var files = localFilePaths
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .OrderBy(p => Path.GetFileName(p))   // sort "01 - ...", "02 - ..." order
                .ToList();

            if (files.Count == 0)
                throw new InvalidOperationException("Ühtegi audiofaili ei leitud.");

            var publicUrls = new List<string>();
            int uploaded = 0;

            foreach (var localPath in files)
            {
                var fileName = Path.GetFileName(localPath);
                // Storage path: "harry-potter/01 - Opening Credits.mp3"
                var storagePath = $"{bookFolderName}/{fileName}";

                var bytes = await File.ReadAllBytesAsync(localPath);
                var mimeType = GetMimeType(fileName);

                await _client.Storage
                    .From(BucketName)
                    .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
                    {
                        ContentType = mimeType,
                        Upsert = true   // overwrite if re-uploading
                    });

                var publicUrl = _client.Storage
                    .From(BucketName)
                    .GetPublicUrl(storagePath);

                publicUrls.Add(publicUrl);
                uploaded++;
                onProgress?.Invoke(uploaded, files.Count);
            }

            return string.Join("|", publicUrls);
        }

        /// <summary>
        /// Uploads a single file (e.g. book cover image) and returns its public URL.
        /// </summary>
        public async Task<string> UploadImageAsync(string localPath, string storagePath)
        {
            if (!File.Exists(localPath)) return localPath; // already a URL or missing

            var bytes = await File.ReadAllBytesAsync(localPath);
            var mimeType = GetMimeType(Path.GetFileName(localPath));

            await _client.Storage
                .From("images")
                .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
                {
                    ContentType = mimeType,
                    Upsert = true
                });

            return _client.Storage.From("images").GetPublicUrl(storagePath);
        }

        private static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".mp3"  => "audio/mpeg",
                ".m4a"  => "audio/mp4",
                ".wav"  => "audio/wav",
                ".ogg"  => "audio/ogg",
                ".aac"  => "audio/aac",
                ".flac" => "audio/flac",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                _       => "application/octet-stream"
            };
        }
    }
}
