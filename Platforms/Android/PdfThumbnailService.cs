using Android.Graphics;
using Android.Graphics.Pdf;

namespace E_Raamatud.Platforms.Android
{
    public static class PdfThumbnailService
    {
        public static async Task<string> ExtractFirstPageAsync(string pdfPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new Java.IO.File(pdfPath);
                    var fd = global::Android.OS.ParcelFileDescriptor.Open(
                        file, global::Android.OS.ParcelFileMode.ReadOnly);

                    using var renderer = new PdfRenderer(fd);
                    using var page = renderer.OpenPage(0);

                    int maxWidth = 600;
                    float scale = maxWidth / (float)page.Width;
                    int thumbWidth = maxWidth;
                    int thumbHeight = (int)(page.Height * scale);

                    var bitmap = Bitmap.CreateBitmap(
                        thumbWidth, thumbHeight, Bitmap.Config.Argb8888);

                    var canvas = new Canvas(bitmap);
                    canvas.DrawColor(global::Android.Graphics.Color.White);

                    page.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

                    var outputPath = System.IO.Path.Combine(
                        FileSystem.CacheDirectory, $"pdf_cover_{Guid.NewGuid()}.jpg");

                    using var stream = new System.IO.FileStream(
                        outputPath,
                        System.IO.FileMode.Create,
                        System.IO.FileAccess.Write);

                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                    bitmap.Recycle();

                    return outputPath;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PDF thumbnail error: {ex.Message}");
                    return null;
                }
            });
        }
    }
}