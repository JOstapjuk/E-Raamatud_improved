using E_Raamatud.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace E_Raamatud.Services
{
    public static class AudioChapterResolver
    {
        private static readonly string[] AudioExtensions = { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" };

        public static List<AudioChapter> Resolve(string audiofail)
        {
            if (string.IsNullOrWhiteSpace(audiofail))
                return new List<AudioChapter>();

            if (audiofail.Contains('|'))
            {
                return audiofail
                    .Split('|')
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select((path, i) => new AudioChapter
                    {
                        Index = i + 1,
                        Title = CleanTitle(Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(path.Split('/').Last()))),
                        FilePath = path.Trim()
                    })
                    .ToList();
            }

            if (Directory.Exists(audiofail))
            {
                return Directory
                    .GetFiles(audiofail)
                    .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f)
                    .Select((path, i) => new AudioChapter
                    {
                        Index = i + 1,
                        Title = CleanTitle(Path.GetFileNameWithoutExtension(path)),
                        FilePath = path
                    })
                    .ToList();
            }

            return new List<AudioChapter>
            {
                new AudioChapter
                {
                    Index = 1,
                    Title = CleanTitle(Path.GetFileNameWithoutExtension(audiofail)),
                    FilePath = audiofail
                }
            };
        }

        private static string CleanTitle(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var trimmed = System.Text.RegularExpressions.Regex
                .Replace(raw, @"^\d+[\s\-\.]+", "").Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? raw : trimmed;
        }
    }
}